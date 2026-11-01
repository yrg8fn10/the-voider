// The Voider - vehicle definition (scaffold).
// The hull is generated procedurally in code (Phase 2), so no custom 3D model is needed.
using UnityEngine;

namespace VoiderMod
{
    public static class VoiderSub
    {
        // Wire this into VehicleFramework's ModVehicle during Phase 1.
        public static void Build()
        {
            // Deck plan (see docs/THE_VOIDER_DESIGN.md):
            //   Deck 1 (keel): docking bay, Seamoth / Prawn dock
            //   Deck 2: engine room
            //   Deck 3: crew quarters
            //   Deck 4: captain's quarters + bridge at the bow
            //   Deck 5: greenhouse dome
            // Razor blades: hull arrays + prow blade, one-tap lethal to leviathan-class on contact.
        }

        // Procedural hull proof-of-concept lives here in Phase 2.
        static Mesh BuildHull()
        {
            return null; // TODO Phase 2
        }
    }
}
