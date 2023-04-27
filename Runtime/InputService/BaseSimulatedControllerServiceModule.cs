// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Definitions.Utilities;
using RealityCollective.Extensions;
using RealityToolkit.Input.Controllers;
using RealityToolkit.Input.Interfaces;
using RealityToolkit.Input.Interfaces.Modules;
using RealityToolkit.Utilities;
using System;
using UnityEngine;

namespace RealityToolkit.DeviceSimulation.InputService
{
    /// <summary>
    /// Base <see cref="ISimulatedControllerServiceModule"/> implementation for controller simulation service modules.
    /// </summary>
    public abstract class BaseSimulatedControllerServiceModule : BaseControllerServiceModule, ISimulatedControllerServiceModule
    {
        /// <inheritdoc />
        protected BaseSimulatedControllerServiceModule(string name, uint priority, SimulatedControllerServiceModuleProfile profile, IInputService parentService)
            : base(name, priority, profile, parentService)
        {
            if (profile.IsNull())
            {
                throw new NullReferenceException($"A {nameof(SimulatedControllerServiceModuleProfile)} is required for {name}");
            }

            SimulatedUpdateFrequency = profile.SimulatedUpdateFrequency;
            ControllerHideTimeout = profile.ControllerHideTimeout;
            DefaultDistance = profile.DefaultDistance;
            DepthMultiplier = profile.DepthMultiplier;
            JitterAmount = profile.JitterAmount;
            ToggleLeftPersistentKey = profile.ToggleLeftPersistentKey;
            LeftControllerTrackedKey = profile.LeftControllerTrackedKey;
            ToggleRightPersistentKey = profile.ToggleRightPersistentKey;
            RightControllerTrackedKey = profile.RightControllerTrackedKey;
            RotationSpeed = profile.RotationSpeed;
        }

        private StopWatch simulatedUpdateStopWatch;
        private long lastSimulatedUpdateTimeStamp = 0;

        /// <summary>
        /// Gets or sets whether the left controller is currently set to be always visible
        /// no matter the simulated tracking state.
        /// </summary>
        protected bool LeftControllerIsAlwaysVisible { get; set; }

        /// <summary>
        /// Gets or sets whether the right controller is currently set to be always visible
        /// no matter the simulated tracking state.
        /// </summary>
        protected bool RightControllerIsAlwaysVisible { get; set; }

        /// <summary>
        /// Gets or sets whether the left controller is currently tracked by simulation.
        /// </summary>
        protected bool LeftControllerIsTracked { get; set; }

        /// <summary>
        /// Gets or sets whether the right controller is currently tracked by simulation.
        /// </summary>
        protected bool RightControllerIsTracked { get; set; }

        /// <inheritdoc />
        public double SimulatedUpdateFrequency { get; }

        /// <inheritdoc />
        public float ControllerHideTimeout { get; }

        /// <inheritdoc />
        public float DefaultDistance { get; }

        /// <inheritdoc />
        public float DepthMultiplier { get; }

        /// <inheritdoc />
        public float JitterAmount { get; }

        /// <inheritdoc />
        public KeyCode ToggleLeftPersistentKey { get; }

        /// <inheritdoc />
        public KeyCode LeftControllerTrackedKey { get; }

        /// <inheritdoc />
        public KeyCode ToggleRightPersistentKey { get; }

        /// <inheritdoc />
        public KeyCode RightControllerTrackedKey { get; }

        /// <inheritdoc />
        public float RotationSpeed { get; }

        /// <inheritdoc />
        public override void Enable()
        {
            base.Enable();

            simulatedUpdateStopWatch = new StopWatch();
            simulatedUpdateStopWatch.Reset();
        }

        /// <inheritdoc />
        public override void Update()
        {
            base.Update();

            RefreshSimulatedDevices();

            // Update all active simulated controllers.
            for (int i = 0; i < ActiveControllers.Count; i++)
            {
                UpdateSimulatedController((ISimulatedController)ActiveControllers[i]);
            }
        }

        /// <inheritdoc />
        public override void Disable()
        {
            RemoveAllControllers();
            base.Disable();
        }

        private void RefreshSimulatedDevices()
        {
            var currentTime = simulatedUpdateStopWatch.Current;
            var msSinceLastUpdate = currentTime.Subtract(new DateTime(lastSimulatedUpdateTimeStamp)).TotalMilliseconds;

            if (msSinceLastUpdate > SimulatedUpdateFrequency)
            {
                if (UnityEngine.Input.GetKeyDown(ToggleLeftPersistentKey))
                {
                    LeftControllerIsAlwaysVisible = !LeftControllerIsAlwaysVisible;
                }

                if (UnityEngine.Input.GetKeyDown(LeftControllerTrackedKey))
                {
                    LeftControllerIsTracked = true;
                }

                if (UnityEngine.Input.GetKeyUp(LeftControllerTrackedKey))
                {
                    LeftControllerIsTracked = false;
                }

                if (LeftControllerIsAlwaysVisible || LeftControllerIsTracked)
                {
                    if (!TryGetController(Handedness.Left, out _))
                    {
                        CreateAndRegisterSimulatedController(Handedness.Left);
                    }
                }
                else
                {
                    RemoveController(Handedness.Left);
                }

                if (UnityEngine.Input.GetKeyDown(ToggleRightPersistentKey))
                {
                    RightControllerIsAlwaysVisible = !RightControllerIsAlwaysVisible;
                }

                if (UnityEngine.Input.GetKeyDown(RightControllerTrackedKey))
                {
                    RightControllerIsTracked = true;
                }

                if (UnityEngine.Input.GetKeyUp(RightControllerTrackedKey))
                {
                    RightControllerIsTracked = false;
                }

                if (RightControllerIsAlwaysVisible || RightControllerIsTracked)
                {
                    if (!TryGetController(Handedness.Right, out _))
                    {
                        CreateAndRegisterSimulatedController(Handedness.Right);
                    }
                }
                else
                {
                    RemoveController(Handedness.Right);
                }

                lastSimulatedUpdateTimeStamp = currentTime.Ticks;
            }
        }

        /// <summary>
        /// Removes the simulated controller and unregisters it for a given hand, if it exists.
        /// </summary>
        /// <param name="handedness">Handedness of the controller to remove.</param>
        protected virtual void RemoveController(Handedness handedness)
        {
            if (TryGetController(handedness, out var controller))
            {
                InputService?.RaiseSourceLost(controller.InputSource, controller);
                RemoveController(controller);
            }
        }

        /// <summary>
        /// Removes and unregisters all currently active simulated controllers.
        /// </summary>
        protected void RemoveAllControllers()
        {
            while (ActiveControllers.Count > 0)
            {
                // It's important here to pass the handedness. Passing the controller
                // will execute the base RemoveController implementation.
                RemoveController(ActiveControllers[0].ControllerHandedness);
            }
        }

        /// <summary>
        /// Gets a simulated controller instance for a hand if it exists.
        /// </summary>
        /// <param name="handedness">Handedness to lookup.</param>
        /// <param name="controller">Controller instance if found.</param>
        /// <returns>True, if instance exists for given handedness.</returns>
        protected bool TryGetController(Handedness handedness, out BaseController controller)
        {
            for (int i = 0; i < ActiveControllers.Count; i++)
            {
                if (ActiveControllers[i] is BaseController existingController &&
                    existingController.ControllerHandedness == handedness)
                {
                    controller = existingController;
                    return true;
                }
            }

            controller = default;
            return false;
        }

        /// <summary>
        /// Updates the provided simulated controller instance.
        /// </summary>
        /// <param name="simulatedController">Controller to update.</param>
        protected virtual void UpdateSimulatedController(ISimulatedController simulatedController)
        {
            simulatedController.UpdateController();
        }

        /// <summary>
        /// Asks the concrete simulation data create and register a new simulated controller.
        /// </summary>
        /// <param name="handedness">The handedness of the controller to create.</param>
        protected abstract ISimulatedController CreateAndRegisterSimulatedController(Handedness handedness);
    }
}