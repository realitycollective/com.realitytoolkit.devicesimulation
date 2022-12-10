// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Attributes;
using RealityCollective.ServiceFramework.Definitions.Platforms;
using RealityToolkit.InputSystem.Hands;
using RealityToolkit.InputSystem.Interfaces;
using UnityEngine;

namespace RealityToolkit.DeviceSimulation.InputService.HandTracking
{
    /// <summary>
    /// Hand controller type for simulated hand controllers.
    /// </summary>
    [RuntimePlatform(typeof(EditorPlatform))]
    [System.Runtime.InteropServices.Guid("07512FFC-5128-434C-B7BF-5CD7CA8EF853")]
    public class SimulatedHandControllerServiceModule : BaseHandControllerServiceModule<SimulatedHandController>, ISimulatedHandControllerServiceModule
    {
        /// <inheritdoc />
        public SimulatedHandControllerServiceModule(string name, uint priority, SimulatedHandControllerServiceModuleProfile profile, IMixedRealityInputSystem parentService)
            : base(name, priority, profile, parentService) { }
    }
}