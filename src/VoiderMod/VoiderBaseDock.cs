using System.Collections;
using UnityEngine;

namespace VoiderMod
{
    // The Voider base dock, Phase 1 structure blockout.
    // Built entirely in code from Ava's concept sketches: a ring-shaped dock
    // with segmented clamp tabs, two clamp arms, a hex frame outline, a
    // "next dock" strip carrying cam1 and cam2 camera housings, and a docking
    // tube that runs to a sealed hatch chamber (seatruck-dock style: the
    // chamber drains once the cockpit pod is docked).
    //
    // Phase 1 = the structure itself, spawnable with the console command
    // "spawndock" for placement testing. Phase 2 = the pod detach / dock /
    // drain logic and the interior. No custom 3D models, ever: everything is
    // Unity primitives generated here.
    public static class VoiderDockBuilder
    {
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

        // The pod approaches along +Z and docks nose-first through the ring.
        public static GameObject BuildDock()
        {
            GameObject root = new GameObject("VoiderBaseDock");

            Material hullMat = MakeMaterial(new Color(0.16f, 0.18f, 0.20f));
            Material steelMat = MakeMaterial(new Color(0.75f, 0.78f, 0.82f));
            Material accentMat = MakeMaterial(new Color(0f, 0.9f, 0.95f));
            Material warnMat = MakeMaterial(new Color(0.9f, 0.15f, 0.1f));

            // --- the dock ring: 12 segments in a circle, opening facing +Z ---
            const int ringSegments = 12;
            const float ringRadius = 4.2f;
            for (int i = 0; i < ringSegments; i++)
            {
                float angle = (360f / ringSegments) * i;
                Vector3 pos = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * ringRadius, Mathf.Sin(angle * Mathf.Deg2Rad) * ringRadius, 0f);
                Quaternion rot = Quaternion.Euler(0f, 0f, angle);
                AddPrimitive(root.transform, PrimitiveType.Cube, "RingSeg" + i,
                    pos, new Vector3(2.3f, 1.1f, 1.6f), rot, hullMat);

                // segmented clamp tabs: small angled pads on the ring's inner face
                Vector3 inward = -pos.normalized;
                AddPrimitive(root.transform, PrimitiveType.Cube, "ClampTab" + i,
                    pos + inward * 0.75f, new Vector3(0.9f, 0.4f, 1.2f), rot, steelMat);
            }

            // --- two clamp arms: top and bottom, reaching toward the axis ---
            AddPrimitive(root.transform, PrimitiveType.Cube, "ClampArmTop",
                new Vector3(0f, 3.1f, 0f), new Vector3(1.4f, 2.6f, 1.8f), Quaternion.identity, steelMat);
            AddPrimitive(root.transform, PrimitiveType.Cube, "ClampArmBottom",
                new Vector3(0f, -3.1f, 0f), new Vector3(1.4f, 2.6f, 1.8f), Quaternion.identity, steelMat);

            // --- hex frame outline behind the ring: 6 fat braces at 60 degrees ---
            const int hexSegments = 6;
            const float hexRadius = 5.4f;
            for (int i = 0; i < hexSegments; i++)
            {
                float angle = (360f / hexSegments) * i + 30f;
                Vector3 pos = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * hexRadius, Mathf.Sin(angle * Mathf.Deg2Rad) * hexRadius, 1.8f);
                Quaternion rot = Quaternion.Euler(0f, 0f, angle);
                AddPrimitive(root.transform, PrimitiveType.Cube, "HexBrace" + i,
                    pos, new Vector3(3.1f, 0.9f, 1.0f), rot, hullMat);
            }

            // --- "next dock" strip above the ring, carrying cam1 and cam2 ---
            AddPrimitive(root.transform, PrimitiveType.Cube, "NextDockStrip",
                new Vector3(0f, 5.6f, 0.9f), new Vector3(6.5f, 0.7f, 0.9f), Quaternion.identity, hullMat);
            AddPrimitive(root.transform, PrimitiveType.Cube, "NextDockLabel",
                new Vector3(-1.8f, 5.6f, 1.4f), new Vector3(1.6f, 0.25f, 0.06f), Quaternion.identity, accentMat);
            AddPrimitive(root.transform, PrimitiveType.Cylinder, "Cam1",
                new Vector3(1.6f, 5.6f, 1.6f), new Vector3(0.5f, 0.45f, 0.5f), Quaternion.Euler(90f, 0f, 0f), steelMat);
            AddPrimitive(root.transform, PrimitiveType.Cylinder, "Cam2",
                new Vector3(2.6f, 5.6f, 1.6f), new Vector3(0.5f, 0.45f, 0.5f), Quaternion.Euler(90f, 0f, 0f), steelMat);

            // --- red guideline stripe along the ring's top ---
            AddPrimitive(root.transform, PrimitiveType.Cube, "GuideStripe",
                new Vector3(0f, 4.9f, 0.85f), new Vector3(7.2f, 0.16f, 0.08f), Quaternion.identity, warnMat);

            // --- docking tube: the pod flies nose-first through this ---
            AddPrimitive(root.transform, PrimitiveType.Cylinder, "DockTube",
                new Vector3(0f, 0f, -5.6f), new Vector3(2.9f, 4.4f, 2.9f), Quaternion.Euler(90f, 0f, 0f), hullMat);
            AddPrimitive(root.transform, PrimitiveType.Cylinder, "DockTubeInner",
                new Vector3(0f, 0f, -5.6f), new Vector3(2.5f, 4.6f, 2.5f), Quaternion.Euler(90f, 0f, 0f), MakeMaterial(new Color(0.08f, 0.09f, 0.11f)));

            // --- sealed hatch chamber at the tube's end: drains when docked ---
            AddPrimitive(root.transform, PrimitiveType.Cube, "HatchChamber",
                new Vector3(0f, 0f, -11.4f), new Vector3(8f, 8f, 4.4f), Quaternion.identity, hullMat);
            AddPrimitive(root.transform, PrimitiveType.Cube, "HatchChamberInner",
                new Vector3(0f, 0f, -11.4f), new Vector3(6.8f, 6.8f, 3.6f), Quaternion.identity, MakeMaterial(new Color(0.10f, 0.12f, 0.14f)));
            AddPrimitive(root.transform, PrimitiveType.Cube, "HatchDoor",
                new Vector3(0f, 0f, -9.3f), new Vector3(5.2f, 5.2f, 0.35f), Quaternion.identity, steelMat);
            AddPrimitive(root.transform, PrimitiveType.Cube, "HatchAccent",
                new Vector3(0f, 2.9f, -9.1f), new Vector3(2.4f, 0.3f, 0.12f), Quaternion.identity, accentMat);

            // --- Phase 2 anchors: where the pod aligns, and the chamber center ---
            GameObject anchor = new GameObject("DockAnchor");
            anchor.transform.SetParent(root.transform, false);
            anchor.transform.localPosition = new Vector3(0f, 0f, 1.2f);
            GameObject chamber = new GameObject("HatchAnchor");
            chamber.transform.SetParent(root.transform, false);
            chamber.transform.localPosition = new Vector3(0f, 0f, -11.4f);

            return root;
        }
    }

    // Console wiring: "spawndock" places the structure 25 m ahead of the player,
    // facing them, for placement testing. Respawning replaces the previous copy.
    public class VoiderDockConsole : MonoBehaviour
    {
        private static GameObject _currentDock;

        public IEnumerator Start()
        {
            // the game's console exists only once a save is loaded
            yield return new WaitUntil(() => DevConsole.instance != null);
            DevConsole.RegisterConsoleCommand("spawndock", OnConsoleCommand_spawndock, false, false);
        }

        public void OnConsoleCommand_spawndock(NotificationCenter.Notification _)
        {
            if (_currentDock != null) Object.Destroy(_currentDock);

            Transform player = Player.main.transform;
            Vector3 pos = player.position + player.forward * 25f;
            pos.y -= 2f;

            _currentDock = VoiderDockBuilder.BuildDock();
            _currentDock.transform.position = pos;
            Vector3 look = player.position;
            look.y = pos.y;
            _currentDock.transform.LookAt(look);
        }
    }
}
