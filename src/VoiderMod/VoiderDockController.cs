using System.Collections;
using UnityEngine;
using VehicleFramework;

namespace VoiderMod
{
    // Phase 2: the dock sequence. A VoiderPod that swims into the ring and
    // slows down is captured, aligned, and walked nose-first through the tube
    // into the sealed hatch chamber. The chamber door seals behind it and the
    // chamber drains: the water plane drops away, the interior lights warm up,
    // and the air comes on (a real OxygenArea, so the player can breathe).
    // "undockpod" reverses the whole thing: air off, door open, clamps release,
    // and the pod is pushed gently back out through the ring.
    //
    // Everything is driven by code and Unity primitives; no custom 3D models.
    public class VoiderDockController : MonoBehaviour
    {
        private enum DockState { Idle, Engaging, Docked, Undocking }

        private DockState _state = DockState.Idle;
        private Transform _dockRoot;
        private GameObject _pod;              // the docked pod, parented to the dock
        private Vector3 _podOriginalScale;

        private Transform _clampTop;
        private Transform _clampBottom;
        private Transform _hatchDoor;
        private GameObject _chamberWater;
        private GameObject _dockStatusLight;
        private GameObject _chamberAir;
        private Renderer[] _chamberLights;

        private const float AlignSpeed = 2.2f;   // m/s: slow enough to be captured
        private const float EngageRadius = 7.5f;

        private void Start()
        {
            _dockRoot = transform;

            // capture volume: a trigger sphere around the ring, so the dock
            // only grabs a pod that actually flew into the mouth
            SphereCollider trigger = gameObject.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 0f, 1.5f);
            trigger.radius = EngageRadius;

            _clampTop = transform.Find("ClampArmTop");
            _clampBottom = transform.Find("ClampArmBottom");
            _hatchDoor = transform.Find("HatchDoor");
            _chamberWater = transform.Find("ChamberWater") != null ? transform.Find("ChamberWater").gameObject : null;
            _dockStatusLight = transform.Find("DockStatusLight") != null ? transform.Find("DockStatusLight").gameObject : null;
            _chamberAir = transform.Find("ChamberAir") != null ? transform.Find("ChamberAir").gameObject : null;
            _chamberLights = CollectLights("ChamberLight");

            SetStatusLight(false);
            if (_chamberAir != null && !_chamberAir.activeSelf) { /* air starts off */ }
        }

        private Renderer[] CollectLights(string prefix)
        {
            System.Collections.Generic.List<Renderer> found = new System.Collections.Generic.List<Renderer>();
            foreach (Transform child in transform)
            {
                if (child.name.StartsWith(prefix)) found.Add(child.GetComponent<Renderer>());
            }
            return found.ToArray();
        }

        private void OnTriggerEnter(Collider other)
        {
            TryEngage(other);
        }

        // a pod that came in hot gets a second chance once it slows down
        private void OnTriggerStay(Collider other)
        {
            TryEngage(other);
        }

        // walk up the hierarchy by hand: the pod's colliders sit on child
        // objects and the stub API here does not carry GetComponentInParents
        private static ModVehicle FindVehicle(Collider other)
        {
            Transform t = other.transform;
            while (t != null)
            {
                ModVehicle mv = t.GetComponent<ModVehicle>();
                if (mv != null) return mv;
                t = t.parent;
            }
            return null;
        }

        private void TryEngage(Collider other)
        {
            if (_state != DockState.Idle) return;
            ModVehicle vehicle = FindVehicle(other);
            if (vehicle == null || !(vehicle is VoiderPod)) return;

            Rigidbody body = vehicle.GetComponent<Rigidbody>();
            if (body == null || body.velocity.magnitude > AlignSpeed) return;

            StartCoroutine(DockSequence(vehicle.gameObject, body));
        }

        // --- the full dock sequence ---
        private IEnumerator DockSequence(GameObject pod, Rigidbody body)
        {
            _state = DockState.Engaging;

            // hold the pod still and hand it to the dock
            body.isKinematic = true;
            _podOriginalScale = pod.transform.localScale;
            pod.transform.SetParent(_dockRoot, true);
            _pod = pod;

            // open the chamber door so the pod has somewhere to go
            yield return SlideDoor(true, 0.8f);

            // walk the pod nose-first: entry -> ring plane -> tube -> chamber
            // the pod's nose (+Z local) faces the dock's -Z, matching the
            // sketches: it flies INTO the ring and out through its back
            Quaternion dockPose = _dockRoot.rotation * Quaternion.Euler(0f, 180f, 0f);

            Vector3 entry = _dockRoot.TransformPoint(new Vector3(0f, 0f, 9f));
            Vector3 ring = _dockRoot.TransformPoint(new Vector3(0f, 0f, 1.2f));
            Vector3 tube = _dockRoot.TransformPoint(new Vector3(0f, 0f, -5.6f));
            Vector3 berth = _dockRoot.TransformPoint(new Vector3(0f, 0f, -11.4f));

            yield return Glide(pod.transform, entry, ring, dockPose, 1.4f);
            yield return Glide(pod.transform, ring, tube, dockPose, 1.2f);
            yield return Glide(pod.transform, tube, berth, dockPose, 1.2f);

            // the pod is in: seal the door behind it, then clamp it down
            yield return SlideDoor(false, 0.8f);
            yield return Clamp(0.6f);

            // chamber drains: water drops away, lights warm up, air comes on
            yield return Drain(true, 1.0f);

            SetStatusLight(true);
            _state = DockState.Docked;
        }

        // --- undock: everything in reverse ---
        public IEnumerator UndockSequence()
        {
            if (_state != DockState.Docked || _pod == null && transform.Find("VoiderPod(Clone)") == null && FindDockedPod() == null)
            {
                // fall through only if there is truly nothing docked
                if (_state != DockState.Docked) yield break;
            }

            _state = DockState.Undocking;
            GameObject pod = _pod != null ? _pod : FindDockedPod();
            if (pod == null)
            {
                _state = DockState.Idle;
                yield break;
            }

            yield return Drain(false, 0.6f);
            yield return ClampOpen(0.6f);
            yield return SlideDoor(true, 0.8f);

            Quaternion dockPose = _dockRoot.rotation * Quaternion.Euler(0f, 180f, 0f);
            Vector3 berth = _dockRoot.TransformPoint(new Vector3(0f, 0f, -11.4f));
            Vector3 ring = _dockRoot.TransformPoint(new Vector3(0f, 0f, 1.2f));
            Vector3 entry = _dockRoot.TransformPoint(new Vector3(0f, 0f, 9.5f));

            yield return Glide(pod.transform, berth, ring, dockPose, 1.2f);
            yield return Glide(pod.transform, ring, entry, dockPose, 1.2f);

            yield return SlideDoor(false, 0.8f);
            SetStatusLight(false);

            // release the pod back to the world, pointing away from the dock
            pod.transform.SetParent(null, true);
            Rigidbody body = pod.GetComponent<Rigidbody>();
            if (body != null)
            {
                body.isKinematic = false;
                body.velocity = _dockRoot.forward * 3.5f;
            }
            _pod = null;
            _state = DockState.Idle;
        }

        // console-facing entry point so the undock command never touches
        // coroutine internals directly
        public void StartUndock()
        {
            StartCoroutine(UndockSequence());
        }

        private GameObject FindDockedPod()
        {
            foreach (Transform child in _dockRoot)
            {
                if (child.name.StartsWith("VoiderPod")) return child.gameObject;
            }
            return null;
        }

        // --- helpers ---
        private IEnumerator Glide(Transform pod, Vector3 from, Vector3 to, Quaternion rot, float seconds)
        {
            float t = 0f;
            while (t < seconds)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / seconds));
                pod.transform.position = Vector3.Lerp(from, to, k);
                pod.transform.rotation = rot;
                yield return null;
            }
            pod.transform.position = to;
            pod.transform.rotation = rot;
        }

        private IEnumerator SlideDoor(bool open, float seconds)
        {
            if (_hatchDoor == null) yield break;
            Vector3 closed = new Vector3(0f, 0f, -9.3f);
            Vector3 opened = new Vector3(0f, 5.4f, -9.3f);
            Vector3 from = _hatchDoor.localPosition;
            Vector3 to = open ? opened : closed;
            float t = 0f;
            while (t < seconds)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / seconds));
                _hatchDoor.localPosition = Vector3.Lerp(from, to, k);
                yield return null;
            }
            _hatchDoor.localPosition = to;
        }

        private IEnumerator Clamp(float seconds)
        {
            if (_clampTop == null || _clampBottom == null) yield break;
            Vector3 topFrom = _clampTop.localPosition;
            Vector3 bottomFrom = _clampBottom.localPosition;
            Vector3 topTo = new Vector3(0f, 1.9f, 0f);
            Vector3 bottomTo = new Vector3(0f, -1.9f, 0f);
            float t = 0f;
            while (t < seconds)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / seconds));
                _clampTop.localPosition = Vector3.Lerp(topFrom, topTo, k);
                _clampBottom.localPosition = Vector3.Lerp(bottomFrom, bottomTo, k);
                yield return null;
            }
            _clampTop.localPosition = topTo;
            _clampBottom.localPosition = bottomTo;
        }

        private IEnumerator ClampOpen(float seconds)
        {
            if (_clampTop == null || _clampBottom == null) yield break;
            Vector3 topFrom = _clampTop.localPosition;
            Vector3 bottomFrom = _clampBottom.localPosition;
            Vector3 topTo = new Vector3(0f, 3.1f, 0f);
            Vector3 bottomTo = new Vector3(0f, -3.1f, 0f);
            float t = 0f;
            while (t < seconds)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / seconds));
                _clampTop.localPosition = Vector3.Lerp(topFrom, topTo, k);
                _clampBottom.localPosition = Vector3.Lerp(bottomFrom, bottomTo, k);
                yield return null;
            }
            _clampTop.localPosition = topTo;
            _clampBottom.localPosition = bottomTo;
        }

        private IEnumerator Drain(bool drained, float seconds)
        {
            if (_chamberWater != null) _chamberWater.SetActive(!drained);
            if (_chamberAir != null)
            {
                if (drained && _chamberAir.GetComponent<OxygenArea>() == null) _chamberAir.AddComponent<OxygenArea>();
                if (!drained)
                {
                    OxygenArea air = _chamberAir.GetComponent<OxygenArea>();
                    if (air != null) Object.Destroy(air);
                }
            }
            if (_chamberLights != null)
            {
                // drained chamber: warm interior lights; flooded: cold cyan
                Color warm = new Color(1f, 0.85f, 0.6f);
                Color cold = new Color(0f, 0.9f, 0.95f);
                foreach (Renderer r in _chamberLights)
                {
                    if (r != null) r.material.color = drained ? warm : cold;
                }
            }
            yield return new WaitForSeconds(seconds);
        }

        private void SetStatusLight(bool docked)
        {
            if (_dockStatusLight == null) return;
            Renderer r = _dockStatusLight.GetComponent<Renderer>();
            if (r != null) r.material.color = docked ? new Color(0.1f, 0.95f, 0.35f) : new Color(0.9f, 0.15f, 0.1f);
        }
    }
}
