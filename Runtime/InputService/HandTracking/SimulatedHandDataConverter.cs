// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Definitions.Utilities;
using RealityCollective.ServiceFramework.Services;
using RealityToolkit.CameraService.Interfaces;
using RealityToolkit.Definitions.Controllers.Hands;
using RealityToolkit.Definitions.Devices;
using RealityToolkit.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RealityToolkit.DeviceSimulation.InputService.HandTracking
{
    /// <summary>
    /// Hand controller type for simulated hand controllers.
    /// </summary>
    public sealed class SimulatedHandDataConverter
    {
        private static ICameraService cameraService = null;

        private static ICameraService CameraService
            => cameraService ?? (cameraService = ServiceManager.Instance.GetService<ICameraService>());

        private static Camera playerCamera = null;

        private static Camera PlayerCamera
        {
            get
            {
                if (playerCamera == null)
                {
                    playerCamera = CameraService != null ? CameraService.CameraRig.RigCamera : Camera.main;
                }

                return playerCamera;
            }
        }

        public SimulatedHandDataConverter(Handedness handedness,
            IReadOnlyList<HandControllerPoseProfile> trackedPoses,
            float handPoseAnimationSpeed,
            float jitterAmount,
            float defaultDistance)
        {
            this.handPoseAnimationSpeed = handPoseAnimationSpeed;
            this.jitterAmount = jitterAmount;
            this.defaultDistance = defaultDistance;
            this.handedness = handedness;
            poseDefinitions = trackedPoses ?? throw new ArgumentException($"{nameof(trackedPoses)} must be provided");

            // Initialize available simulated hand poses and find the configured default pose.
            SimulatedHandControllerPose.Initialize(trackedPoses);

            if (SimulatedHandControllerPose.DefaultHandPose == null)
            {
                throw new ArgumentException("There is no default simulated hand pose defined!");
            }

            initialPose = SimulatedHandControllerPose.GetPoseByName(SimulatedHandControllerPose.DefaultHandPose.Id);
            pose = new SimulatedHandControllerPose(initialPose);

            // Start the timestamp stopwatches
            lastUpdatedStopWatch = new StopWatch();
            lastUpdatedStopWatch.Reset();
            handUpdateStopWatch = new StopWatch();
            handUpdateStopWatch.Reset();

            ResetConverter();
        }

        private readonly Handedness handedness;
        private readonly float handPoseAnimationSpeed;
        private readonly float jitterAmount;
        private readonly float defaultDistance;
        private readonly IReadOnlyList<HandControllerPoseProfile> poseDefinitions;
        private readonly StopWatch handUpdateStopWatch;
        private readonly StopWatch lastUpdatedStopWatch;

        private float currentPoseBlending = 0.0f;
        private float targetPoseBlending = 0.0f;
        private Vector3 screenPosition;
        private readonly SimulatedHandControllerPose initialPose;
        private SimulatedHandControllerPose previousPose;
        private SimulatedHandControllerPose targetPose;

        /// <summary>
        /// Gets the hands position in screen space.
        /// </summary>
        public Vector3 ScreenPosition => screenPosition;

        private SimulatedHandControllerPose pose;

        /// <summary>
        /// Currently used simulation hand pose.
        /// </summary>
        public SimulatedHandControllerPose Pose => pose;

        /// <summary>
        /// The currently targeted hand pose, reached when <see cref="TargetPoseBlending"/>
        /// reaches 1.
        /// </summary>
        private SimulatedHandControllerPose TargetPose
        {
            get => targetPose;
            set
            {
                if (!string.Equals(value.Id, targetPose.Id))
                {
                    targetPose = value;
                    targetPoseBlending = 0.0f;
                }
            }
        }

        /// <summary>
        /// Linear interpolation state between current pose and target pose.
        /// Will get clamped to [current,1] where 1 means the hand has reached the target pose.
        /// </summary>
        public float TargetPoseBlending
        {
            get => targetPoseBlending;
            private set => targetPoseBlending = Mathf.Clamp(value, targetPoseBlending, 1.0f);
        }

        /// <summary>
        /// Current rotation of the hand.
        /// </summary>
        public Vector3 HandRotateEulerAngles { get; private set; } = Vector3.zero;

        /// <summary>
        /// Current random offset to simulate tracking inaccuracy.
        /// </summary>
        public Vector3 JitterOffset { get; private set; } = Vector3.zero;

        /// <summary>
        /// Gets simulated hand data for a <see cref="HandController"/>.
        /// </summary>
        /// <param name="position">The simulated camera space position of the hand controller.</param>
        /// <param name="deltaRotation">The rotation delta applied to the hand since last update.</param>
        /// <returns>Updated simulated hand data.</returns>
        public HandData GetSimulatedHandData(Vector3 position, Vector3 deltaRotation)
        {
            // Read keyboard / mouse input to determine the root pose delta since last frame.
            var rootPoseDelta = new Pose(position, Quaternion.Euler(deltaRotation));

            // Calculate pose changes and compute timestamp for hand tracking update.
            var poseAnimationDelta = handPoseAnimationSpeed * Time.deltaTime;
            var timeStamp = handUpdateStopWatch.TimeStamp;

            // Update simulated hand states using collected data.
            var newTargetPose = GetTargetHandPose();

            HandleSimulationInput(rootPoseDelta);

            if (!string.Equals(newTargetPose.Id, Pose.Id))
            {
                previousPose = Pose;
                TargetPose = newTargetPose;
            }

            TargetPoseBlending += poseAnimationDelta;

            var handData = UpdatePoseFrame();
            handData.UpdatedAt = timeStamp;
            handData.TrackingState = TrackingState.Tracked;

            return handData;
        }

        private void HandleSimulationInput(Pose handRootPose)
        {
            var mousePos = UnityEngine.Input.mousePosition;
            screenPosition = new Vector3(mousePos.x, mousePos.y, defaultDistance)
            {
                // Apply position delta x / y in screen space, but depth (z) offset in world space
                x = handRootPose.position.x,
                y = handRootPose.position.y
            };

            var newWorldPoint = PlayerCamera.ScreenToWorldPoint(ScreenPosition);
            newWorldPoint += PlayerCamera.transform.forward * handRootPose.position.z;
            screenPosition = PlayerCamera.WorldToScreenPoint(newWorldPoint);

            // The provided hand root pose rotation is just a delta from the
            // previous frame, so we need to determine the final rotation still.
            HandRotateEulerAngles += handRootPose.rotation.eulerAngles;
            JitterOffset = Random.insideUnitSphere * jitterAmount;
        }

        private HandData UpdatePoseFrame()
        {
            if (TargetPoseBlending > currentPoseBlending)
            {
                float range = Mathf.Clamp01(1.0f - currentPoseBlending);
                float lerpFactor = range > 0.0f ? (TargetPoseBlending - currentPoseBlending) / range : 1.0f;
                SimulatedHandControllerPose.Lerp(ref pose, previousPose, TargetPose, lerpFactor);
            }

            currentPoseBlending = TargetPoseBlending;
            var rotation = Quaternion.Euler(HandRotateEulerAngles);
            var position = PlayerCamera.ScreenToWorldPoint(ScreenPosition + JitterOffset);

            // At this point we know the hand's root pose in world space and
            // need to translate to the camera rig's local coordinate space.
            var rootPose = new Pose(position, rotation);
            var rigTransform = CameraService != null
                ? CameraService.CameraRig.RigTransform
                : Camera.main.transform.parent;
            rootPose.position = rigTransform.InverseTransformPoint(rootPose.position);
            rootPose.rotation = Quaternion.Inverse(rigTransform.rotation) * rigTransform.rotation * rootPose.rotation;

            // Compute joint poses relative to root pose.
            var jointPoses = ComputeJointPoses(Pose, handedness);

            return new HandData(rootPose, jointPoses);
        }

        /// <summary>
        /// Computes local poses from camera-space joint data.
        /// </summary>
        private Pose[] ComputeJointPoses(SimulatedHandControllerPose pose, Handedness handedness)
        {
            var cameraRotation = PlayerCamera.transform.rotation;
            var jointPoses = new Pose[HandData.JointCount];

            for (int i = 0; i < HandData.JointCount; i++)
            {
                // Initialize from local offsets
                var localPosition = pose.LocalJointPoses[i].position;
                var localRotation = pose.LocalJointPoses[i].rotation;

                // Pose offset are for right hand, mirror on X axis if left hand is needed
                if (handedness == Handedness.Left)
                {
                    localPosition.x = -localPosition.x;
                    localRotation.y = -localRotation.y;
                    localRotation.z = -localRotation.z;
                }

                // Apply camera transform
                localPosition = cameraRotation * localPosition;
                localRotation = cameraRotation * localRotation;

                jointPoses[i] = new Pose(localPosition, localRotation);
            }

            return jointPoses;
        }

        /// <summary>
        /// Selects a hand pose to simulate, while its input keycode is pressed.
        /// </summary>
        /// <returns>Default pose if no other fitting user UnityEngine.Input.</returns>
        private SimulatedHandControllerPose GetTargetHandPose()
        {
            for (int i = 0; i < poseDefinitions.Count; i++)
            {
                var result = poseDefinitions[i];

                if (UnityEngine.Input.GetKey(result.KeyCode))
                {
                    return SimulatedHandControllerPose.GetPoseByName(result.Id);
                }
            }

            return SimulatedHandControllerPose.GetPoseByName(SimulatedHandControllerPose.DefaultHandPose.Id);
        }

        public void ResetConverter()
        {
            screenPosition = Vector3.zero;
            HandRotateEulerAngles = Vector3.zero;
            JitterOffset = Vector3.zero;

            // reset to the initial pose.
            TargetPoseBlending = 1.0f;

            if (SimulatedHandControllerPose.TryGetPoseByName(initialPose.Id, out var result))
            {
                pose = new SimulatedHandControllerPose(result);
                previousPose = pose;
                TargetPose = pose;
            }
        }
    }
}
