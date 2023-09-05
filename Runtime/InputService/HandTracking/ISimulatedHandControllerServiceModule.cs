// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityToolkit.Input.Hands;

namespace RealityToolkit.DeviceSimulation.InputService.HandTracking
{
    public interface ISimulatedHandControllerServiceModule : ISimulatedControllerServiceModule, IHandControllerServiceModule
    {
        /// <summary>
        /// Gets the simulated hand controller pose animation speed controlling
        /// how fast the hand will translate from one pose to another.
        /// </summary>
        float HandPoseAnimationSpeed { get; }
    }
}