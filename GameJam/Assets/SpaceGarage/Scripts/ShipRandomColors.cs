using UnityEngine;

namespace SpaceGarage
{
    public static class ShipRandomColors
    {
        public static Color Sample(float alpha = 1)
        {
            Color color = Random.ColorHSV(0f, 1f, 0.45f, 0.85f, 0.7f, 1f);
            color.a = alpha;
            return color;
        }

        public static void Apply(GameObject part)
        {
            foreach (var renderer in part.GetComponentsInChildren<Renderer>(true))
            {
                if (!(renderer is MeshRenderer) && !(renderer is SkinnedMeshRenderer)) continue;
                var materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    var material = materials[i];
                    if (material == null) continue;
                    var block = new MaterialPropertyBlock(); renderer.GetPropertyBlock(block, i);
                    float alpha = material.HasProperty("_BaseColor") ? material.GetColor("_BaseColor").a :
                        material.HasProperty("_Color") ? material.GetColor("_Color").a : 1;
                    Color color = Sample(alpha);
                    if (material.HasProperty("_BaseColor")) block.SetColor("_BaseColor", color);
                    if (material.HasProperty("_Color")) block.SetColor("_Color", color);
                    renderer.SetPropertyBlock(block, i);
                }
            }
        }
    }
}
