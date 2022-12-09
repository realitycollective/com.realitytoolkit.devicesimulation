// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityToolkit.Definitions.Utilities;
using RealityToolkit.InputSystem.Hands;
using System.Collections.Generic;
using UnityEngine.XR;

namespace RealityToolkit.DeviceSimulation.InputService.HandTracking
{
    public class SimulatedTrackedHandJointPoseProvider : ITrackedHandJointPoseProvider
    {
        public void UpdateHandJoints(InputDevice inputDevice, ref MixedRealityPose[] jointPoses, ref Dictionary<TrackedHandJoint, MixedRealityPose> jointPosesDictionary)
        {
            throw new System.NotImplementedException();
        }
    }
}
