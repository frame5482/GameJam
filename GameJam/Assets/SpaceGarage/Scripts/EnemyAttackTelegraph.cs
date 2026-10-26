using UnityEngine;

namespace SpaceGarage
{
    public sealed class EnemyAttackTelegraph : MonoBehaviour
    {
        [SerializeField] private LineRenderer warningLine = null;
        [SerializeField, Min(0.001f)] private float width = 0.035f;
        [SerializeField] private Color warningColor = new Color(1, 0.25f, 0.05f, 1);
        private Material ownedMaterial;

        private void Awake()
        {
            if (warningLine == null)
            {
                var child = new GameObject("AttackTelegraph");
                child.transform.SetParent(transform, false);
                warningLine = child.AddComponent<LineRenderer>();
                var shader = Shader.Find("Universal Render Pipeline/Unlit");
                if (shader != null)
                {
                    ownedMaterial = new Material(shader);
                    ownedMaterial.SetColor("_BaseColor", warningColor);
                    warningLine.sharedMaterial = ownedMaterial;
                }
            }
            warningLine.useWorldSpace = true;
            warningLine.positionCount = 2;
            warningLine.startWidth = warningLine.endWidth = width;
            warningLine.startColor = warningLine.endColor = warningColor;
            warningLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            warningLine.enabled = false;
        }

        public void Show(Vector3 origin, Vector3 target)
        {
            if (warningLine == null) return;
            warningLine.enabled = true;
            warningLine.SetPosition(0, origin);
            warningLine.SetPosition(1, target);
        }

        public void Hide() { if (warningLine != null) warningLine.enabled = false; }
        private void OnDisable() => Hide();
        private void OnDestroy() { if (ownedMaterial != null) Destroy(ownedMaterial); }
    }
}
