using UnityEngine;
using UnityEngine.VFX;

namespace SpaceGarage
{
    // Put on a part prefab, with the VFX in a separate child so disabling the effect never disables the part.
    public sealed class ShipBoostEffect : MonoBehaviour
    {
        [Tooltip("Existing effect child. Takes priority over Effect Prefab.")]
        [SerializeField] private GameObject effectRoot = null;
        [Tooltip("Optional: instantiate this once when the part is assembled, instead of supplying an existing child.")]
        [SerializeField] private GameObject effectPrefab = null;
        [SerializeField] private Transform effectSpawnPoint = null;
        private VisualEffect[] graphs = new VisualEffect[0];
        private bool initialized;
        private bool playing;

        private void Awake() => Initialize();

        private void Initialize()
        {
            if (initialized) return;
            initialized = true;
            if (effectRoot == null && effectPrefab != null)
            {
                var mount = effectSpawnPoint != null ? effectSpawnPoint : transform;
                if (mount != transform && !mount.IsChildOf(transform))
                { Debug.LogWarning("Boost Effect Spawn Point must belong to this part.", this); return; }
                effectRoot = Instantiate(effectPrefab, mount, false);
                effectRoot.transform.localPosition = Vector3.zero;
                effectRoot.transform.localRotation = Quaternion.identity;
            }
            if (effectRoot == null) return;
            if (effectRoot == gameObject || !effectRoot.transform.IsChildOf(transform))
            {
                Debug.LogWarning("Boost Effect Root must be a separate child of this part, not the part or ship root.", this);
                effectRoot = null;
                return;
            }
            graphs = effectRoot.GetComponentsInChildren<VisualEffect>(true);
            foreach (var graph in graphs) graph.Stop();
            effectRoot.SetActive(false);
        }

        public void SetBoosting(bool active)
        {
            Initialize();
            if (effectRoot == null) return;
            active &= isActiveAndEnabled;
            if (playing == active) return;
            playing = active;
            if (active)
            {
                effectRoot.SetActive(true);
                foreach (var graph in graphs)
                    if (graph != null && graph.isActiveAndEnabled) graph.Play();
            }
            else
            {
                foreach (var graph in graphs) if (graph != null) graph.Stop();
                effectRoot.SetActive(false);
            }
        }

        private void OnDisable() => SetBoosting(false);
    }
}
