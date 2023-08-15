﻿// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Definitions.Utilities;
using RealityToolkit.Definitions.Controllers;
using RealityToolkit.Input.Hands;
using RealityToolkit.Input.Hands.Poses;
using System.Collections.Generic;
using UnityEngine;

namespace RealityToolkit.DeviceSimulation.InputService.HandTracking
{
    public class SimulatedHandControllerServiceModuleProfile : SimulatedControllerServiceModuleProfile
    {
        [SerializeField]
        [Tooltip("Gesture interpolation per second")]
        private float handPoseAnimationSpeed = 8.0f;

        /// <summary>
        /// Pose interpolation per second.
        /// </summary>
        public float HandPoseAnimationSpeed => handPoseAnimationSpeed;

        [SerializeField]
        [Range(.5f, 1f)]
        [Tooltip("Threshold in range [0.5, 1] that defines when a hand is considered to be grabing.")]
        private float gripThreshold = .8f;

        /// <summary>
        /// Threshold in range [0, 1] that defines when a hand is considered to be grabing.
        /// </summary>
        public float GripThreshold => gripThreshold;

        [SerializeField]
        [Tooltip("If set, hands will be setup with colliders and a rigidbody to work with Unity's physics system.")]
        private bool handPhysicsEnabled = false;

        /// <summary>
        /// If set, hands will be setup with colliders and a rigidbody to work with Unity's physics system.
        /// </summary>
        public bool HandPhysicsEnabled => handPhysicsEnabled;

        [SerializeField]
        [Tooltip("If set, hand colliders will be setup as triggers.")]
        private bool useTriggers = false;

        /// <summary>
        /// If set, hand colliders will be setup as triggers.
        /// </summary>
        public bool UseTriggers => useTriggers;

        [SerializeField]
        [Tooltip("Set the bounds mode to use for calculating hand bounds.")]
        private HandBoundsLOD boundsMode = HandBoundsLOD.Low;

        /// <summary>
        /// Set the bounds mode to use for calculating hand bounds.
        /// </summary>
        public HandBoundsLOD BoundsMode => boundsMode;

        [SerializeField]
        [Tooltip("Tracked hand poses for pose detection.")]
        private List<HandControllerPoseProfile> trackedPoses = new List<HandControllerPoseProfile>();

        /// <summary>
        /// Tracked hand poses for pose detection.
        /// </summary>
        public IReadOnlyList<HandControllerPoseProfile> TrackedPoses => trackedPoses;

        public override ControllerDefinition[] GetDefaultControllerOptions()
        {
            return new[]
            {
                new ControllerDefinition(typeof(SimulatedHandController), Handedness.Left),
                new ControllerDefinition(typeof(SimulatedHandController), Handedness.Right),
            };
        }
    }
}