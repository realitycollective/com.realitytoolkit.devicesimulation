// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Definitions.Utilities;
using RealityCollective.ServiceFramework.Attributes;
using RealityCollective.ServiceFramework.Definitions.Platforms;
using RealityCollective.ServiceFramework.Services;
using RealityToolkit.Definitions.Controllers.Hands;
using RealityToolkit.Definitions.Devices;
using RealityToolkit.Input.Controllers.Hands;
using RealityToolkit.Input.Definitions;
using RealityToolkit.Input.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityToolkit.DeviceSimulation.InputService.HandTracking
{
    /// <summary>
    /// Hand controller type for simulated hand controllers.
    /// </summary>
    [RuntimePlatform(typeof(EditorPlatform))]
    [System.Runtime.InteropServices.Guid("07512FFC-5128-434C-B7BF-5CD7CA8EF853")]
    public class SimulatedHandControllerServiceModule : BaseSimulatedControllerServiceModule, ISimulatedHandControllerServiceModule
    {
        /// <inheritdoc />
        public SimulatedHandControllerServiceModule(string name, uint priority, SimulatedHandControllerServiceModuleProfile profile, IInputService parentService)
            : base(name, priority, profile, parentService)
        {
            if (!ServiceManager.Instance.TryGetServiceProfile<IInputService, InputServiceProfile>(out var inputServiceProfile))
            {
                throw new ArgumentException($"Unable to get a valid {nameof(InputServiceProfile)}!");
            }

            HandPoseAnimationSpeed = profile.HandPoseAnimationSpeed;

            HandPhysicsEnabled = profile.HandPhysicsEnabled != inputServiceProfile.HandControllerSettings.HandPhysicsEnabled
                ? profile.HandPhysicsEnabled
                : inputServiceProfile.HandControllerSettings.HandPhysicsEnabled;

            UseTriggers = profile.UseTriggers != inputServiceProfile.HandControllerSettings.UseTriggers
                ? profile.UseTriggers
                : inputServiceProfile.HandControllerSettings.UseTriggers;

            BoundsMode = profile.BoundsMode != inputServiceProfile.HandControllerSettings.BoundsMode
                ? profile.BoundsMode
                : inputServiceProfile.HandControllerSettings.BoundsMode;

            var isGrippingThreshold = profile.GripThreshold != inputServiceProfile.HandControllerSettings.GripThreshold
                ? profile.GripThreshold
                : inputServiceProfile.HandControllerSettings.GripThreshold;

            if (profile.TrackedPoses != null && profile.TrackedPoses.Count > 0)
            {
                TrackedPoses = profile.TrackedPoses.Count != inputServiceProfile.HandControllerSettings.TrackedPoses.Count
                    ? profile.TrackedPoses
                    : inputServiceProfile.HandControllerSettings.TrackedPoses;
            }
            else
            {
                TrackedPoses = inputServiceProfile.HandControllerSettings.TrackedPoses;
            }

            if (TrackedPoses == null || TrackedPoses.Count == 0)
            {
                throw new ArgumentException($"Failed to start {name}! {nameof(TrackedPoses)} not set");
            }

            leftHandConverter = new SimulatedHandDataConverter(
                Handedness.Left,
                TrackedPoses,
                HandPoseAnimationSpeed,
                JitterAmount,
                DefaultDistance);

            rightHandConverter = new SimulatedHandDataConverter(
                Handedness.Right,
                TrackedPoses,
                HandPoseAnimationSpeed,
                JitterAmount,
                DefaultDistance);

            postProcessor = new HandDataPostProcessor(TrackedPoses, isGrippingThreshold);
        }

        private readonly SimulatedHandDataConverter leftHandConverter;
        private readonly SimulatedHandDataConverter rightHandConverter;
        private readonly HandDataPostProcessor postProcessor;

        /// <inheritdoc />
        public float HandPoseAnimationSpeed { get; }

        /// <inheritdoc />
        public bool HandPhysicsEnabled { get; set; }

        /// <inheritdoc />
        public bool UseTriggers { get; set; }

        /// <inheritdoc />
        public HandBoundsLOD BoundsMode { get; set; }

        private IReadOnlyList<HandControllerPoseProfile> TrackedPoses { get; }

        /// <inheritdoc />
        protected override void UpdateSimulatedController(ISimulatedController simulatedController)
        {
            // Ignore updates if the simulated controllers are not tracked, but only visible.
            if (simulatedController.ControllerHandedness == Handedness.Left && !LeftControllerIsTracked)
            {
                return;
            }
            else if (simulatedController.ControllerHandedness == Handedness.Right && !RightControllerIsTracked)
            {
                return;
            }

            var simulatedHandController = (HandController)simulatedController;
            var converter = simulatedHandController.ControllerHandedness == Handedness.Left
                ? leftHandConverter
                : rightHandConverter;

            var simulatedHandData = converter.GetSimulatedHandData(
                simulatedController.GetPosition(DepthMultiplier),
                simulatedController.GetDeltaRotation(RotationSpeed));

            simulatedHandData = postProcessor.PostProcess(simulatedHandController.ControllerHandedness, simulatedHandData);
            simulatedHandController.UpdateController(simulatedHandData);
        }

        /// <inheritdoc />
        protected override ISimulatedController CreateAndRegisterSimulatedController(Handedness handedness)
        {
            SimulatedHandController controller;

            try
            {
                controller = new SimulatedHandController(this, TrackingState.Tracked, handedness, GetControllerMappingProfile(typeof(SimulatedHandController), handedness));
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create {nameof(SimulatedHandController)}!\n{e}");
                return null;
            }

            controller.TryRenderControllerModel();

            InputService?.RaiseSourceDetected(controller.InputSource, controller);
            AddController(controller);
            return controller;
        }

        protected override void RemoveController(Handedness handedness)
        {
            if (handedness == Handedness.Left)
            {
                leftHandConverter.ResetConverter();
            }
            else if (handedness == Handedness.Right)
            {
                rightHandConverter.ResetConverter();
            }

            base.RemoveController(handedness);
        }
    }
}