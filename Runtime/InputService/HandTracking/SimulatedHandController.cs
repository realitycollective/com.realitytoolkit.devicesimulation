// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Definitions.Utilities;
using RealityToolkit.Definitions.Controllers;
using RealityToolkit.Definitions.Devices;
using RealityToolkit.InputSystem.Hands;

namespace RealityToolkit.DeviceSimulation.InputService.HandTracking
{
    /// <summary>
    /// Hand controller type for simulated hand controllers.
    /// </summary>
    [System.Runtime.InteropServices.Guid("435C4F16-8E23-4228-B2B0-5FCE09A97043")]
    public class SimulatedHandController : HandController
    {
        /// <inheritdoc />
        public SimulatedHandController() : base() { }

        /// <inheritdoc />
        public SimulatedHandController(IHandControllerServiceModule serviceModule, TrackingState trackingState, Handedness controllerHandedness, MixedRealityControllerMappingProfile controllerMappingProfile)
            : base(serviceModule, trackingState, controllerHandedness, controllerMappingProfile)
        {
            trackedHandJointPoseProvider = new SimulatedTrackedHandJointPoseProvider();
        }
    }
}