using System.Collections.Generic;
using UnityEngine;

namespace SpaceGarage
{
    public sealed class CustomizationPreviewPart : MonoBehaviour
    {
        public ShipPartSlot Slot { get; private set; }
        private Transform visual;
        private Vector3 rest;
        private bool selected;
        private float lift;
        private sealed class Surface
        {
            public Renderer renderer;
            public int index;
            public MaterialPropertyBlock original, highlighted;
        }
        private readonly List<Surface> surfaces = new List<Surface>();
        public void Initialize(ShipPartSlot slot, Transform model)
        {
            Slot = slot; visual = model; rest = model.localPosition;
            foreach (var renderer in model.GetComponentsInChildren<Renderer>(true))
            {
                if (!(renderer is MeshRenderer) && !(renderer is SkinnedMeshRenderer)) continue;
                var materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    var material = materials[i]; if (material == null) continue;
                    var original = new MaterialPropertyBlock(); var highlight = new MaterialPropertyBlock();
                    renderer.GetPropertyBlock(original, i); renderer.GetPropertyBlock(highlight, i);
                    foreach (string property in new[] { "_BaseColor", "_Color" })
                        if (material.HasProperty(property))
                        {
                            Color color = material.GetColor(property); float alpha = color.a;
                            color *= 1.35f; color.a = alpha; highlight.SetColor(property, color);
                        }
                    if (material.HasProperty("_EmissionColor")) highlight.SetColor("_EmissionColor", material.GetColor("_EmissionColor") + new Color(0.08f, 0.2f, 0.25f));
                    surfaces.Add(new Surface { renderer = renderer, index = i, original = original, highlighted = highlight });
                }
            }
        }
        public void SetSelected(bool value)
        {
            selected = value;
            foreach (var surface in surfaces)
                if (surface.renderer != null) surface.renderer.SetPropertyBlock(value ? surface.highlighted : surface.original, surface.index);
        }
        private void LateUpdate()
        {
            if (visual == null) return;
            lift = Mathf.MoveTowards(lift, selected ? 0.15f : 0, Time.deltaTime * 0.8f);
            visual.localPosition = rest + (visual.parent != null ? visual.parent.InverseTransformVector(Vector3.up * lift) : Vector3.up * lift);
        }
        private void OnDestroy() => SetSelected(false);
    }
}
