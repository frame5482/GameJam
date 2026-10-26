using UnityEngine;
using UnityEngine.VFX;

namespace SpaceGarage
{
    // One persistent VFX instance per muzzle; both size properties toggle together.
    internal sealed class ShipBeamEmitter
    {
        private GameObject root;
        private VisualEffect effect;
        private VisualEffectAsset asset;
        private static readonly int Size = Shader.PropertyToID("Size");
        private static readonly int Size1 = Shader.PropertyToID("Size 1");

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
                if (!effect.HasFloat(Size) || !effect.HasFloat(Size1))
                {
                    Debug.LogWarning("Weapon beam VFX must expose floats named Size and Size 1.", parent);
                    Dispose(); return;
                }
                effect.SetFloat(Size, 0);
                effect.SetFloat(Size1, 0);
                root.SetActive(true);
                effect.Play();
            }
            root.transform.SetPositionAndRotation(muzzle.position, muzzle.rotation);
            effect.SetFloat(Size, size);
            effect.SetFloat(Size1, size);
        }

        public void Hide()
        {
            if (effect != null && effect.HasFloat(Size)) effect.SetFloat(Size, 0);
            if (effect != null && effect.HasFloat(Size1)) effect.SetFloat(Size1, 0);
        }
        public void Dispose()
        {
            Hide();
            if (root != null) { root.SetActive(false); Object.Destroy(root); }
            root = null; effect = null; asset = null;
        }
    }
}
