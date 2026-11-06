using BepInEx;
using UnityEngine;
using VehicleFramework.Admin;

namespace VoiderMod
{
    // The Voider - mod entry point. Registers the sub with VehicleFramework
    // as soon as BepInEx loads, so it is spawnable in any save.
    [BepInPlugin(PluginGuid, "The Voider", PluginVersion)]
    [BepInDependency("com.mikjaw.subnautica.vehicleframework.mod", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.snmodding.nautilus", BepInDependency.DependencyFlags.HardDependency)]
    public class VoiderPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.osagonresearch.thevoider";
        public const string PluginVersion = "0.1.1";

        private void Awake()
        {
            GameObject vehicleRoot = new GameObject("Voider");
            vehicleRoot.AddComponent<VoiderVehicle>();
            VehicleRegistrar.RegisterVehicleLater(vehicleRoot.GetComponent<VoiderVehicle>(), verbose: true);
            GameObject dockConsole = new GameObject("VoiderDockConsole");
            dockConsole.AddComponent<VoiderDockConsole>();
            DontDestroyOnLoad(dockConsole);
            Logger.LogInfo("[Voider] plugin loaded, registering the sub with VehicleFramework.");
        }
    }
}
