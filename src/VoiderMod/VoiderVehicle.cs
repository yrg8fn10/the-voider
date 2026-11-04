using System.Collections.Generic;
using UnityEngine;
using VehicleFramework;
using VehicleFramework.VehicleTypes;
using VehicleFramework.VehicleBuilding;

namespace VoiderMod
{
    // The Voider, Phase 2 blockout: a real, spawnable Subnautica submarine whose
    // hull is generated entirely in code (no custom 3D models, ever).
    // This is the playable skeleton. The five-deck walkable interior and the
    // razor-blade damage system build on this hull next.
    public class VoiderVehicle : Submarine
    {
        private GameObject _model;
        private GameObject[] _collisionModel;
        private VehiclePilotSeat _pilotSeat;
        private bool _pilotSeatBuilt;
        private List<VehicleHatchStruct> _hatches;

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

        // --- identity and balance ---
        public override string Description => "A gargantuan Precursor razor-class submarine. Its hull blades cut leviathan-class creatures in a single pass.";
        public override string EncyclopediaEntry =>
            "The Voider.\n\n" +
            "A razor-type combat submarine built by the Precursors. " +
            "It is gargantuan, its hull bristles with blade arrays, and the prow blade cuts " +
            "through leviathan-class creatures in a single pass.\n\n" +
            "Assessment: do not get in front of it.";
        public override PilotingStyleEnum PilotingStyle => PilotingStyleEnum.Cyclops;
        public override int Mass => 20000;
        public override int MaxHealth => 2000;
        public override int BaseCrushDepth => 500;
        public override int NumModules => 8;
        public override bool CanLeviathanGrab { get; set; } = true;
        public override Dictionary<TechType, int> Recipe => new Dictionary<TechType, int>
        {
            { TechType.Titanium, 15 },
            { TechType.PlasteelIngot, 6 },
            { TechType.Glass, 3 },
            { TechType.Lead, 2 },
            { TechType.AdvancedWiringKit, 1 }
        };

        // --- helpers ---
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

        // --- the hull, drawn in code ---
        private GameObject BuildModel()
        {
            _model = new GameObject("VoiderModel");
            _model.transform.SetParent(transform, false);

            Material hullMat = MakeMaterial(new Color(0.16f, 0.18f, 0.20f));
            Material bladeMat = MakeMaterial(new Color(0.75f, 0.78f, 0.82f));
            Material accentMat = MakeMaterial(new Color(0f, 0.9f, 0.95f));

            // main hull: a long capsule running along Z, prow at +Z
            AddPrimitive(_model.transform, PrimitiveType.Capsule, "Hull",
                new Vector3(0f, 0f, 0f), new Vector3(8f, 30f, 8f), Quaternion.Euler(90f, 0f, 0f), hullMat);

            // prow blade wedge
            AddPrimitive(_model.transform, PrimitiveType.Cube, "ProwBlade",
                new Vector3(0f, 0f, 14.5f), new Vector3(6f, 1.2f, 4f), Quaternion.identity, bladeMat);

            // hull blade arrays: razor blades along both flanks
            for (int i = 0; i < 6; i++)
            {
                float z = 8f - i * 3.4f;
                float t = Mathf.Abs(z) / 9f;
                float x = 3.6f + t * 0.6f;
                Quaternion tilt = Quaternion.Euler(0f, 0f, 14f);
                AddPrimitive(_model.transform, PrimitiveType.Cube, "BladeL" + i,
                    new Vector3(-x, 0f, z), new Vector3(0.5f, 2.2f, 3.0f), tilt, bladeMat);
                AddPrimitive(_model.transform, PrimitiveType.Cube, "BladeR" + i,
                    new Vector3(x, 0f, z), new Vector3(0.5f, 2.2f, 3.0f), tilt, bladeMat);
            }

            // greenhouse dome
            AddPrimitive(_model.transform, PrimitiveType.Sphere, "GreenhouseDome",
                new Vector3(0f, 4.6f, -6f), new Vector3(5.5f, 3.6f, 5.5f), Quaternion.identity, accentMat);

            // bridge cockpit bubble at the bow
            AddPrimitive(_model.transform, PrimitiveType.Sphere, "Bridge",
                new Vector3(0f, 3.0f, 11.5f), new Vector3(4.4f, 2.6f, 4.4f), Quaternion.identity, accentMat);

            // props at the stern
            AddPrimitive(_model.transform, PrimitiveType.Cube, "PropV",
                new Vector3(0f, 0f, -15.2f), new Vector3(0.6f, 5f, 0.8f), Quaternion.identity, bladeMat);
            AddPrimitive(_model.transform, PrimitiveType.Cube, "PropH",
                new Vector3(0f, 0f, -15.2f), new Vector3(5f, 0.6f, 0.8f), Quaternion.identity, bladeMat);

            return _model;
        }

        private GameObject[] BuildCollision()
        {
            List<GameObject> colliders = new List<GameObject>();
            GameObject main = new GameObject("VoiderCollisionHull");
            main.transform.SetParent(transform, false);
            main.transform.localPosition = Vector3.zero;
            BoxCollider hullBox = main.AddComponent<BoxCollider>();
            hullBox.size = new Vector3(7.5f, 7.5f, 29f);
            colliders.Add(main);

            GameObject prow = new GameObject("VoiderCollisionProw");
            prow.transform.SetParent(transform, false);
            prow.transform.localPosition = new Vector3(0f, 0f, 14.5f);
            BoxCollider prowBox = prow.AddComponent<BoxCollider>();
            prowBox.size = new Vector3(5.5f, 2f, 4.5f);
            colliders.Add(prow);

            _collisionModel = colliders.ToArray();
            return _collisionModel;
        }

        // --- cockpit ---
        private VehiclePilotSeat BuildPilotSeat()
        {
            Material accentMat = MakeMaterial(new Color(0f, 0.9f, 0.95f));

            GameObject seat = new GameObject("VoiderPilotSeat");
            seat.transform.SetParent(transform, false);
            seat.transform.localPosition = new Vector3(0f, 2.4f, 11.5f);
            AddPrimitive(seat.transform, PrimitiveType.Cube, "SeatBase",
                new Vector3(0f, 0f, 0f), new Vector3(1.0f, 0.5f, 1.0f), Quaternion.identity, accentMat);
            AddPrimitive(seat.transform, PrimitiveType.Cube, "SeatBack",
                new Vector3(0f, 0.7f, -0.5f), new Vector3(1.0f, 1.4f, 0.25f), Quaternion.identity, accentMat);

            GameObject sitLocation = new GameObject("VoiderSitLocation");
            sitLocation.transform.SetParent(seat.transform, false);
            sitLocation.transform.localPosition = new Vector3(0f, 0.55f, 0f);

            GameObject exit = new GameObject("VoiderSeatExit");
            exit.transform.SetParent(seat.transform, false);
            exit.transform.localPosition = new Vector3(0f, 1.6f, -1.2f);

            _pilotSeat = new VehiclePilotSeat(seat, sitLocation, null, null, exit.transform);
            return _pilotSeat;
        }

        // --- the hatch: top of the hull, behind the cockpit ---
        private List<VehicleHatchStruct> BuildHatches()
        {
            Material accentMat = MakeMaterial(new Color(0f, 0.9f, 0.95f));

            GameObject hatch = new GameObject("VoiderHatch");
            hatch.transform.SetParent(transform, false);
            hatch.transform.localPosition = new Vector3(0f, 4.4f, -2f);
            AddPrimitive(hatch.transform, PrimitiveType.Cube, "HatchFrame",
                new Vector3(0f, 0f, 0f), new Vector3(1.8f, 0.5f, 1.8f), Quaternion.identity, accentMat);

            GameObject entry = new GameObject("VoiderHatchEntry");
            entry.transform.SetParent(hatch.transform, false);
            entry.transform.localPosition = new Vector3(0f, 1.8f, 0f);

            GameObject exit = new GameObject("VoiderHatchExit");
            exit.transform.SetParent(hatch.transform, false);
            exit.transform.localPosition = new Vector3(0f, -3.0f, 0f);

            _hatches = new List<VehicleHatchStruct>
            {
                new VehicleHatchStruct(hatch, entry.transform, exit.transform, entry.transform)
            };
            return _hatches;
        }
    }
}
