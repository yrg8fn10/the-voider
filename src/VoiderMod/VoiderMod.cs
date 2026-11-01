// The Voider - mod entry point (scaffold).
// TODO Phase 1: align with the VehicleFramework API and the chosen mod loader
// (QModManager or BepInEx). VehicleFramework: https://github.com/PrimeSonic/VehicleFramework
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
