using UnityEngine;

namespace SpaceGarage
{
    [ExecuteAlways, DisallowMultipleComponent]
    public sealed class ShipToonOutline : MonoBehaviour
    {
        [SerializeField, Min(0)] private float outlineWorldWidth = 0.012f;
        private void OnEnable() => Apply();
        private void OnValidate() => Apply();

        public void SetOutlineWidth(float worldWidth)
        {
            outlineWorldWidth = Mathf.Max(0, worldWidth);
            Apply();
        }

        private void Apply()
        {
            foreach (var renderer in GetComponentsInChildren<Renderer>(true))
            {
                if (!(renderer is MeshRenderer) && !(renderer is SkinnedMeshRenderer)) continue;
                Vector3 scale = renderer.transform.lossyScale;
                float largestScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
                if (largestScale < 0.00001f) continue;
                var materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    var material = materials[i];
                    if (material == null || material.shader == null || material.shader.name != "Toon/Toon") continue;
                    var block = new MaterialPropertyBlock();
                    renderer.GetPropertyBlock(block, i);
                    block.SetFloat("_Outline_Width", outlineWorldWidth * 1000f / largestScale);
                    renderer.SetPropertyBlock(block, i);
                }
            }
        }
    }
}
