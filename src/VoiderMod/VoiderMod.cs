// The Voider - mod entry point (scaffold).
// TODO Phase 1: align with the VehicleFramework API on the modern modding stack
// (BepInEx + Nautilus; QModManager is retired). VehicleFramework: https://github.com/PrimeSonic/VehicleFramework
// The BepInPlugin attribute and plugin entry point land in Phase 1.
using System;

namespace VoiderMod
{
    public static class VoiderMain
    {
        public static void Init()
        {
            Console.WriteLine("[Voider] initializing...");
            // Phase 1: register the Voider with VehicleFramework here.
            // VoiderSub.Register();
        }
    }
}
