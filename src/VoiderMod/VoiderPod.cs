using System.Collections.Generic;
using UnityEngine;
using VehicleFramework;
using VehicleFramework.VehicleTypes;
using VehicleFramework.VehicleBuilding;

namespace VoiderMod
{
    // The Voider's detachable cockpit pod, Phase 2. In the concept sketches the
    // cockpit pod separates from the Voider hull and flies home on its own,
    // docking nose-first at the base dock (Below Zero seatruck-dock style).
    // This is that pod as its own small VehicleFramework vehicle: a compact
    // precursor cockpit sphere with a docking nose cone, spawnable on its own
    // so the detach is represented at blockout level (the full mid-flight
    // detach from the moving Voider hull comes later).
    public class VoiderPod : Submarine
    {
        private GameObject _model;
        private GameObject[] _collisionModel;
        private VehiclePilotSeat _pilotSeat;
        private bool _pilotSeatBuilt;
        private List<VehicleHatchStruct> _hatches;

        public static GameObject BlueprintRoot;

        public override GameObject VehicleModel => _model ?? (_model = BuildModel());
        public override GameObject[] CollisionModel => _collisionModel ?? (_collisionModel = BuildCollision());
        public override VehiclePilotSeat PilotSeat
        {
            get
            {
                if (!_pilotSeatBuilt) { _pilotSeat = BuildPilotSeat(); _pilotSeatBuilt = true; }
                return _pilotSeat;
            }
        }
        public override List<VehicleHatchStruct> Hatches => _hatches ?? (_hatches = BuildHatches());

        private Sprite _craftingSprite;
        public override Sprite CraftingSprite
        {
            get { return _craftingSprite ?? (_craftingSprite = BuildCraftingSprite()); }
        }

        private static Sprite BuildCraftingSprite()
        {
            const int size = 128;
            Color32 bg = new Color32(8, 12, 20, 255);
            Color32 cyan = new Color32(0, 224, 255, 255);
            Color32 white = new Color32(235, 250, 255, 255);
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color32[] px = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float cx = x - (size - 1) * 0.5f;
                    float cy = y - (size - 1) * 0.5f;
                    Color32 c = new Color32(0, 0, 0, 0);
                    float r = Mathf.Sqrt(cx * cx + cy * cy);
                    if (r <= 60f) { c = bg; }
                    if (r <= 40f) { c = cyan; }
                    if (r <= 13f && cx > 6f) { c = white; } // the nose cone, pointing right
                    px[y * size + x] = c;
                }
            }
            tex.SetPixels32(px);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        // --- identity and balance ---
        public override string Description => "The Voider's detachable cockpit pod. It flies home on its own and docks nose-first at a Voider Base Dock.";
        public override string EncyclopediaEntry =>
            "Voider Cockpit Pod.\n\n" +
            "The cockpit pod separates from the Voider hull and flies home on its own, " +
            "sliding nose-first through the dock ring into a sealed chamber that drains " +
            "the moment the pod is locked in.\n\n" +
            "Assessment: small, quick, and very hard to hit.";
        public override PilotingStyleEnum PilotingStyle => PilotingStyleEnum.Seamoth;
        public override int Mass => 900;
        public override int MaxHealth => 250;
        public override int BaseCrushDepth => 400;
        public override int NumModules => 1;
        public override bool CanLeviathanGrab { get; set; } = false;
        public override Dictionary<TechType, int> Recipe => new Dictionary<TechType, int>
        {
            { TechType.Titanium, 3 },
            { TechType.Glass, 2 },
            { TechType.Lubricant, 1 }
        };

        private static Material MakeMaterial(Color color)
        {
            Shader shader = Shader.Find("MarmosetUBER");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Legacy Shaders/Diffuse");
            Material mat = new Material(shader);
            mat.color = color;
            return mat;
        }

        private static GameObject AddPrimitive(Transform parent, PrimitiveType type, string name, Vector3 localPos, Vector3 localScale, Quaternion localRot, Material mat)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            Object.Destroy(go.GetComponent<Collider>());
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = localRot;
            go.transform.localScale = localScale;
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            return go;
        }

        // --- the pod, drawn in code: nose points +Z ---
        private GameObject BuildModel()
        {
            _model = new GameObject("VoiderPodModel");
            _model.transform.SetParent(transform, false);

            Material hullMat = MakeMaterial(new Color(0.16f, 0.18f, 0.20f));
            Material steelMat = MakeMaterial(new Color(0.75f, 0.78f, 0.82f));
            Material accentMat = MakeMaterial(new Color(0f, 0.9f, 0.95f));
            Material glassMat = MakeMaterial(new Color(0.05f, 0.12f, 0.17f));

            // cockpit sphere
            AddPrimitive(_model.transform, PrimitiveType.Sphere, "PodHull",
                new Vector3(0f, 0f, 0f), new Vector3(2.4f, 2.1f, 2.7f), Quaternion.identity, hullMat);

            // forward canopy glass
            AddPrimitive(_model.transform, PrimitiveType.Sphere, "PodCanopy",
                new Vector3(0f, 0.25f, 0.95f), new Vector3(1.6f, 1.2f, 1.1f), Quaternion.identity, glassMat);

            // docking nose cone: the part that slides through the ring
            AddPrimitive(_model.transform, PrimitiveType.Cylinder, "PodNose",
                new Vector3(0f, 0f, 1.75f), new Vector3(1.1f, 1.2f, 1.1f), Quaternion.Euler(90f, 0f, 0f), steelMat);

            // accent band around the mid
            AddPrimitive(_model.transform, PrimitiveType.Cylinder, "PodBand",
                new Vector3(0f, 0f, -0.2f), new Vector3(4.9f, 0.14f, 4.9f), Quaternion.Euler(90f, 0f, 0f), accentMat);

            // twin engines at the stern
            AddPrimitive(_model.transform, PrimitiveType.Cylinder, "EngineL",
                new Vector3(-0.8f, -0.15f, -1.45f), new Vector3(0.55f, 0.9f, 0.55f), Quaternion.Euler(90f, 0f, 0f), steelMat);
            AddPrimitive(_model.transform, PrimitiveType.Cylinder, "EngineR",
                new Vector3(0.8f, -0.15f, -1.45f), new Vector3(0.55f, 0.9f, 0.55f), Quaternion.Euler(90f, 0f, 0f), steelMat);

            // side rail fins, echo of the Voider's razor profile
            AddPrimitive(_model.transform, PrimitiveType.Cube, "FinL",
                new Vector3(-1.35f, 0f, -0.5f), new Vector3(0.25f, 1.1f, 1.8f), Quaternion.Euler(0f, 0f, 12f), steelMat);
            AddPrimitive(_model.transform, PrimitiveType.Cube, "FinR",
                new Vector3(1.35f, 0f, -0.5f), new Vector3(0.25f, 1.1f, 1.8f), Quaternion.Euler(0f, 0f, -12f), steelMat);

            return _model;
        }

        private GameObject[] BuildCollision()
        {
            GameObject main = new GameObject("VoiderPodCollision");
            main.transform.SetParent(transform, false);
            main.transform.localPosition = Vector3.zero;
            BoxCollider box = main.AddComponent<BoxCollider>();
            box.size = new Vector3(3.2f, 3.0f, 4.4f);
            _collisionModel = new GameObject[] { main };
            return _collisionModel;
        }

        // --- cockpit: single seat, center of the sphere ---
        private VehiclePilotSeat BuildPilotSeat()
        {
            Material accentMat = MakeMaterial(new Color(0f, 0.9f, 0.95f));

            GameObject seat = new GameObject("VoiderPodSeat");
            seat.transform.SetParent(transform, false);
            seat.transform.localPosition = new Vector3(0f, -0.35f, -0.55f);
            AddPrimitive(seat.transform, PrimitiveType.Cube, "SeatBase",
                new Vector3(0f, 0f, 0f), new Vector3(0.9f, 0.4f, 0.9f), Quaternion.identity, accentMat);
            AddPrimitive(seat.transform, PrimitiveType.Cube, "SeatBack",
                new Vector3(0f, 0.6f, -0.45f), new Vector3(0.9f, 1.3f, 0.22f), Quaternion.identity, accentMat);

            GameObject sitLocation = new GameObject("VoiderPodSitLocation");
            sitLocation.transform.SetParent(seat.transform, false);
            sitLocation.transform.localPosition = new Vector3(0f, 0.5f, 0f);

            GameObject exit = new GameObject("VoiderPodSeatExit");
            exit.transform.SetParent(seat.transform, false);
            exit.transform.localPosition = new Vector3(0f, 1.4f, -1.1f);

            _pilotSeat = new VehiclePilotSeat(seat, sitLocation, null, null, exit.transform);
            return _pilotSeat;
        }

        // --- the hatch: top of the pod ---
        private List<VehicleHatchStruct> BuildHatches()
        {
            Material accentMat = MakeMaterial(new Color(0f, 0.9f, 0.95f));

            GameObject hatch = new GameObject("VoiderPodHatch");
            hatch.transform.SetParent(transform, false);
            hatch.transform.localPosition = new Vector3(0f, 1.0f, -0.3f);
            AddPrimitive(hatch.transform, PrimitiveType.Cube, "HatchFrame",
                new Vector3(0f, 0f, 0f), new Vector3(1.3f, 0.35f, 1.3f), Quaternion.identity, accentMat);

            GameObject entry = new GameObject("VoiderPodHatchEntry");
            entry.transform.SetParent(hatch.transform, false);
            entry.transform.localPosition = new Vector3(0f, 1.2f, 0f);

            GameObject exit = new GameObject("VoiderPodHatchExit");
            exit.transform.SetParent(hatch.transform, false);
            exit.transform.localPosition = new Vector3(0f, -2.0f, 0f);

            _hatches = new List<VehicleHatchStruct>
            {
                new VehicleHatchStruct(hatch, entry.transform, exit.transform, entry.transform)
            };
            return _hatches;
        }
    }
}
