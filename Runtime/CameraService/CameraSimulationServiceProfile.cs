
using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Interfaces;
using UnityEngine;

namespace RealityToolkit.DeviceSimulation.CameraService
{
    [CreateAssetMenu(menuName = "CameraSimulationServiceProfile", fileName = "CameraSimulationServiceProfile", order = (int)CreateProfileMenuItemIndices.ServiceConfig)]
    public class CameraSimulationServiceProfile : BaseServiceProfile<IServiceModule>
    { }
}
