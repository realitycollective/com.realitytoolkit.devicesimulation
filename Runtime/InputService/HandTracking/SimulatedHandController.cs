// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Definitions.Utilities;
using RealityToolkit.Definitions.Controllers;
using RealityToolkit.Definitions.Devices;
using RealityToolkit.Input.Controllers.Hands;
using RealityToolkit.Input.Extensions;
using RealityToolkit.Input.Interfaces.Modules;
using UnityEngine;

namespace RealityToolkit.DeviceSimulation.InputService.HandTracking
{
    /// <summary>
    /// Hand controller type for simulated hand controllers.
    /// </summary>
    [System.Runtime.InteropServices.Guid("435C4F16-8E23-4228-B2B0-5FCE09A97043")]
    public class SimulatedHandController : HandController, ISimulatedController
    {
        /// <inheritdoc />
        public SimulatedHandController() : base() { }

        /// <inheritdoc />
        public SimulatedHandController(IControllerServiceModule controllerDataProvider, TrackingState trackingState, Handedness controllerHandedness, ControllerMappingProfile controllerMappingProfile)
            : base(controllerDataProvider, trackingState, controllerHandedness, controllerMappingProfile)
        { }

        /// <inheritdoc />
        public override InteractionMapping[] DefaultInteractions { get; } =
        {
            // 6 DoF pose of the spatial pointer ("far interaction pointer").
            new InteractionMapping("Spatial Pointer Pose", AxisType.SixDof, DeviceInputType.SpatialPointer),
            // Select / pinch button press / release.
            new InteractionMapping("Select", AxisType.Digital, DeviceInputType.Select),
            // Hand in pointing pose yes/no?
            new InteractionMapping("Point", AxisType.Digital, DeviceInputType.ButtonPress),
            // Grip / grab button press / release.
            new InteractionMapping("Grip", AxisType.Digital, DeviceInputType.TriggerPress),
            // 6 DoF grip pose ("Where to put things when grabbing something?")
            new InteractionMapping("Grip Pose", AxisType.SixDof, DeviceInputType.SpatialGrip),
            // 6 DoF index finger tip pose (mainly for "near interaction pointer").
            new InteractionMapping("Index Finger Pose", AxisType.SixDof, DeviceInputType.IndexFinger),
            
            // Simulation specifics...
            new InteractionMapping("Yaw Clockwise", AxisType.Digital, DeviceInputType.ButtonPress, KeyCode.E),
            new InteractionMapping("Yaw Counter Clockwise", AxisType.Digital, DeviceInputType.ButtonPress, KeyCode.Q),
            new InteractionMapping("Pitch Clockwise", AxisType.Digital, DeviceInputType.ButtonPress, KeyCode.F),
            new InteractionMapping("Pitch Counter Clockwise", AxisType.Digital, DeviceInputType.ButtonPress, KeyCode.R),
            new InteractionMapping("Roll Clockwise", AxisType.Digital, DeviceInputType.ButtonPress, KeyCode.X),
            new InteractionMapping("Roll Counter Clockwise", AxisType.Digital, DeviceInputType.ButtonPress, KeyCode.Z),
            new InteractionMapping("Move Away (Depth)", AxisType.Digital, DeviceInputType.ButtonPress, KeyCode.PageUp),
            new InteractionMapping("Move Closer (Depth)", AxisType.Digital, DeviceInputType.ButtonPress, KeyCode.PageDown)
        };

        /// <inheritdoc />
        public Vector3 GetDeltaRotation(float rotationSpeed)
        {
            UpdateSimulationMappings();

            float rotationDelta = rotationSpeed * Time.deltaTime;
            Vector3 rotationDeltaEulerAngles = Vector3.zero;

            if (Interactions[6].BoolData)
            {
                rotationDeltaEulerAngles.y = rotationDelta;
            }

            if (Interactions[7].BoolData)
            {
                rotationDeltaEulerAngles.y = -rotationDelta;
            }

            if (Interactions[8].BoolData)
            {
                rotationDeltaEulerAngles.x = -rotationDelta;
            }

            if (Interactions[9].BoolData)
            {
                rotationDeltaEulerAngles.x = rotationDelta;
            }

            if (Interactions[10].BoolData)
            {
                rotationDeltaEulerAngles.z = -rotationDelta;
            }

            if (Interactions[11].BoolData)
            {
                rotationDeltaEulerAngles.z = rotationDelta;
            }

            return rotationDeltaEulerAngles;
        }

        /// <inheritdoc />
        public Vector3 GetPosition(float depthMultiplier)
        {
            UpdateSimulationMappings();

            Vector3 mousePosition = UnityEngine.Input.mousePosition;

            if (Interactions[12].BoolData)
            {
                mousePosition.z += Time.deltaTime * depthMultiplier;
            }

            if (Interactions[13].BoolData)
            {
                mousePosition.z -= Time.deltaTime * depthMultiplier;
            }

            return mousePosition;
        }

        private void UpdateSimulationMappings()
        {
            for (int i = 0; i < Interactions?.Length; i++)
            {
                var interactionMapping = Interactions[i];

                switch (interactionMapping.InputType)
                {
                    case DeviceInputType.ButtonPress:
                        interactionMapping.BoolData = UnityEngine.Input.GetKey(interactionMapping.KeyCode);
                        interactionMapping.RaiseInputAction(InputSource, ControllerHandedness);
                        break;
                }
            }
        }
    }
}