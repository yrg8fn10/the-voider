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
        public const string PluginVersion = "0.1.4";

        private void Awake()
        {
            GameObject vehicleRoot = new GameObject("Voider");
            vehicleRoot.AddComponent<VoiderVehicle>();
            // The plugin loads in the main-menu scene, so the sub's blueprint
            // object would be destroyed the moment a save loads, leaving the
            // MVB entry pointing at nothing. Keep the blueprint alive across
            // scene changes; VF instantiates fresh copies from it when crafted.
            UnityEngine.Object.DontDestroyOnLoad(vehicleRoot);
            VehicleRegistrar.RegisterVehicleLater(vehicleRoot.GetComponent<VoiderVehicle>(), verbose: true);
            GameObject podRoot = new GameObject("VoiderPod");
            podRoot.AddComponent<VoiderPod>();
            // same rule as the sub: keep the pod's blueprint alive across scene
            // changes so VF can instantiate fresh copies when it is crafted
            UnityEngine.Object.DontDestroyOnLoad(podRoot);
            VoiderPod.BlueprintRoot = podRoot;
            VehicleRegistrar.RegisterVehicleLater(podRoot.GetComponent<VoiderPod>(), verbose: true);
            GameObject dockConsole = new GameObject("VoiderDockConsole");
            dockConsole.AddComponent<VoiderDockConsole>();
            DontDestroyOnLoad(dockConsole);
            VoiderDockBuildable.Register();
            Logger.LogInfo("[Voider] plugin loaded, registering the sub and the cockpit pod with VehicleFramework.");
        }
    }
}
