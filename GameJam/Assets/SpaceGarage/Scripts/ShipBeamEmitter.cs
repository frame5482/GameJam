using UnityEngine;
using UnityEngine.VFX;

namespace SpaceGarage
{
    // One persistent VFX instance per muzzle; Size alone controls its visible beam.
    internal sealed class ShipBeamEmitter
    {
        private GameObject root;
        private VisualEffect effect;
        private VisualEffectAsset asset;
        private static readonly int Size = Shader.PropertyToID("Size");

        public void Show(VisualEffectAsset graph, Transform parent, Transform muzzle, float size)
        {
            if (graph == null) { Hide(); return; }
            if (root == null || asset != graph)
            {
                Dispose();
                root = new GameObject("WeaponBeam");
                root.SetActive(false);
                root.transform.SetParent(parent, false);
                effect = root.AddComponent<VisualEffect>();
                asset = graph;
                effect.visualEffectAsset = graph;
                if (!effect.HasFloat(Size))
                {
                    Debug.LogWarning("Weapon beam VFX must expose a float named Size.", parent);
                    Dispose(); return;
                }
                effect.SetFloat(Size, 0);
                root.SetActive(true);
                effect.Play();
            }
            root.transform.SetPositionAndRotation(muzzle.position, muzzle.rotation);
            effect.SetFloat(Size, size);
        }

        public void Hide()
        {
            if (effect != null && effect.HasFloat(Size)) effect.SetFloat(Size, 0);
        }
        public void Dispose()
        {
            Hide();
            if (root != null) { root.SetActive(false); Object.Destroy(root); }
            root = null; effect = null; asset = null;
        }
    }
}
