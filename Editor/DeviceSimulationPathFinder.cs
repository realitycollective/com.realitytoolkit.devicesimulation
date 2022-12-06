// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using RealityToolkit.Editor.Utilities;

namespace RealityToolkit.DeviceSimulation.Editor
{
    /// <summary>
    /// Dummy scriptable object used to find the relative path of the com.realitytoolkit.devicesimulation package.
    /// </summary>
    /// <inheritdoc cref="IPathFinder" />
    public class DeviceSimulationPathFinder : ScriptableObject, IPathFinder
    {
        /// <inheritdoc />
        public string Location => $"/Editor/{nameof(DeviceSimulationPathFinder)}.cs";
    }
}