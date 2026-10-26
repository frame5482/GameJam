using UnityEngine;
using UnityEngine.VFX;

namespace SpaceGarage
{
    // Put on a part prefab, with the VFX in a separate child so disabling the effect never disables the part.
    public sealed class ShipBoostEffect : MonoBehaviour
    {
        [Tooltip("Normal exhaust child, enabled while flying without boost.")]
        [SerializeField] private GameObject normalEffectRoot = null;
        [Tooltip("Existing effect child. Takes priority over Effect Prefab.")]
        [SerializeField] private GameObject effectRoot = null;
        [Tooltip("Optional: instantiate this once when the part is assembled, instead of supplying an existing child.")]
        [SerializeField] private GameObject effectPrefab = null;
        [SerializeField] private Transform effectSpawnPoint = null;
        private VisualEffect[] graphs = new VisualEffect[0];
        private VisualEffect[] normalGraphs = new VisualEffect[0];
        private bool initialized;
        private bool playing;
        private bool flying;
        private bool stateApplied;

        private void Awake() => Initialize();
        private void OnEnable() => SetFlightState(true, false);

        private void Initialize()
        {
            if (initialized) return;
            initialized = true;
            if (effectRoot == null && effectPrefab != null)
            {
                var mount = effectSpawnPoint != null ? effectSpawnPoint : transform;
                if (mount != transform && !mount.IsChildOf(transform))
                    Debug.LogWarning("Boost Effect Spawn Point must belong to this part.", this);
                else
                {
                    effectRoot = Instantiate(effectPrefab, mount, false);
                    effectRoot.transform.localPosition = Vector3.zero;
                    effectRoot.transform.localRotation = Quaternion.identity;
                }
            }
            ValidateRoot(ref effectRoot);
            ValidateRoot(ref normalEffectRoot);
            if (effectRoot != null && normalEffectRoot != null &&
                (effectRoot.transform.IsChildOf(normalEffectRoot.transform) || normalEffectRoot.transform.IsChildOf(effectRoot.transform)))
            {
                Debug.LogWarning("Normal and boost roots must be separate children, not the same object or nested inside one another.", this);
                normalEffectRoot = null;
            }
            if (effectRoot != null) graphs = effectRoot.GetComponentsInChildren<VisualEffect>(true);
            if (normalEffectRoot != null) normalGraphs = normalEffectRoot.GetComponentsInChildren<VisualEffect>(true);
            SetEffect(effectRoot, graphs, false);
            SetEffect(normalEffectRoot, normalGraphs, false);
        }

        private void ValidateRoot(ref GameObject root)
        {
            if (root == null) return;
            if (root != gameObject && root.transform.IsChildOf(transform)) return;
            Debug.LogWarning("Engine Effect Root must be a separate child of this part, not the part or ship root.", this);
            root = null;
        }

        public void SetBoosting(bool active) => SetFlightState(true, active);

        public void SetFlightState(bool flightActive, bool boostActive)
        {
            Initialize();
            flightActive &= isActiveAndEnabled;
            boostActive &= flightActive;
            if (stateApplied && flying == flightActive && playing == boostActive) return;
            stateApplied = true;
            flying = flightActive;
            playing = boostActive;
            bool normalActive = flightActive && !boostActive;
            if (!normalActive) SetEffect(normalEffectRoot, normalGraphs, false);
            if (!boostActive) SetEffect(effectRoot, graphs, false);
            if (normalActive) SetEffect(normalEffectRoot, normalGraphs, true);
            if (boostActive) SetEffect(effectRoot, graphs, true);
        }

        private static void SetEffect(GameObject root, VisualEffect[] effects, bool active)
        {
            if (root == null) return;
            if (active)
            {
                root.SetActive(true);
                foreach (var graph in effects)
                    if (graph != null && graph.isActiveAndEnabled) graph.Play();
            }
            else
            {
                foreach (var graph in effects) if (graph != null) graph.Stop();
                root.SetActive(false);
            }
        }

        private void OnDisable() => SetFlightState(false, false);
    }
}
