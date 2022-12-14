// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Definitions.Utilities;
using RealityCollective.ServiceFramework.Attributes;
using RealityCollective.ServiceFramework.Definitions.Platforms;
using RealityCollective.ServiceFramework.Services;
using RealityToolkit.Definitions.Controllers.Hands;
using RealityToolkit.Definitions.Devices;
using RealityToolkit.InputSystem.Controllers.Hands;
using RealityToolkit.InputSystem.Definitions;
using RealityToolkit.InputSystem.Interfaces;
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
        public SimulatedHandControllerServiceModule(string name, uint priority, SimulatedHandControllerServiceModuleProfile profile, IMixedRealityInputSystem parentService)
            : base(name, priority, profile, parentService)
        {
            if (!ServiceManager.Instance.TryGetServiceProfile<IMixedRealityInputSystem, MixedRealityInputSystemProfile>(out var inputSystemProfile))
            {
                throw new ArgumentException($"Unable to get a valid {nameof(MixedRealityInputSystemProfile)}!");
            }

            HandPoseAnimationSpeed = profile.HandPoseAnimationSpeed;

            RenderingMode = profile.RenderingMode != inputSystemProfile.RenderingMode
                ? profile.RenderingMode
                : inputSystemProfile.RenderingMode;

            HandPhysicsEnabled = profile.HandPhysicsEnabled != inputSystemProfile.HandPhysicsEnabled
                ? profile.HandPhysicsEnabled
                : inputSystemProfile.HandPhysicsEnabled;

            UseTriggers = profile.UseTriggers != inputSystemProfile.UseTriggers
                ? profile.UseTriggers
                : inputSystemProfile.UseTriggers;

            BoundsMode = profile.BoundsMode != inputSystemProfile.BoundsMode
                ? profile.BoundsMode
                : inputSystemProfile.BoundsMode;

            var isGrippingThreshold = profile.GripThreshold != inputSystemProfile.GripThreshold
                ? profile.GripThreshold
                : inputSystemProfile.GripThreshold;

            if (profile.TrackedPoses != null && profile.TrackedPoses.Count > 0)
            {
                TrackedPoses = profile.TrackedPoses.Count != inputSystemProfile.TrackedPoses.Count
                    ? profile.TrackedPoses
                    : inputSystemProfile.TrackedPoses;
            }
            else
            {
                TrackedPoses = inputSystemProfile.TrackedPoses;
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

        /// <inheritdoc />
        public HandRenderingMode RenderingMode { get; set; }

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

            var simulatedHandController = (MixedRealityHandController)simulatedController;
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

            InputSystem?.RaiseSourceDetected(controller.InputSource, controller);
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