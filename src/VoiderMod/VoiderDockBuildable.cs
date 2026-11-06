using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using Nautilus.Utility;
using UnityEngine;

namespace VoiderMod
{
    // The Voider base dock, registered as a real Habitat Builder piece so it
    // lives in the world like any other base module (build it, place it, it
    // stays). Same procedural geometry as the "spawndock" test placement, but
    // with a placement collider and the Constructable wiring the builder tool
    // needs. The pod-dock/drain logic still comes in Phase 2.
    public static class VoiderDockBuildable
    {
        public static PrefabInfo Info { get; } = PrefabInfo.WithTechType(
                "VoiderBaseDock",
                "Voider Base Dock",
                "A Precursor docking ring for the Voider's cockpit pod. Segmented clamps, twin camera masts, and a sealed hatch chamber that drains when the pod is docked.")
            // the vanilla moonpool icon is the closest fit on the builder wheel
            .WithIcon(SpriteManager.Get(TechType.BaseMoonpool));

        public static void Register()
        {
            CustomPrefab prefab = new CustomPrefab(Info);

            prefab.SetGameObject(() =>
            {
                GameObject dock = VoiderDockBuilder.BuildDock();

                // placement bounds for the habitat builder (the builder tool
                // raycasts against a collider to place and rotate the piece)
                BoxCollider bounds = dock.AddComponent<BoxCollider>();
                bounds.center = new Vector3(0f, 0f, -3f);
                bounds.size = new Vector3(12f, 13f, 24f);

                // exterior seafloor placement, rotatable, counts as base structure
                ConstructableFlags flags = ConstructableFlags.Ground
                    | ConstructableFlags.Base
                    | ConstructableFlags.Outside
                    | ConstructableFlags.Rotatable;
                PrefabUtils.AddConstructable(dock, Info.TechType, flags, dock);

                return dock;
            });

            // show it in the habitat builder's Exterior Modules tab
            prefab.SetPdaGroupCategory(TechGroup.ExteriorModules, TechCategory.ExteriorModule);

            prefab.SetRecipe(new RecipeData(
                new Ingredient(TechType.Titanium, 8),
                new Ingredient(TechType.PlasteelIngot, 2),
                new Ingredient(TechType.Lead, 2),
                new Ingredient(TechType.AdvancedWiringKit, 1)));

            prefab.Register();
        }
    }
}
