using UnityEngine;
using UnityEngine.InputSystem;

namespace SpaceGarage
{
    // Aim after Cinemachine has updated the rendered camera for this frame.
    [DefaultExecutionOrder(1100)]
    public sealed class ShipGun : MonoBehaviour
    {
        // Preserve component references serialized by older gun prefabs.
        [SerializeField, HideInInspector] private ShipProjectile projectilePrefab = null;
        [SerializeField, InspectorName("Projectile Prefab")] private GameObject projectilePrefabObject = null;
        [SerializeField, InspectorName("Muzzle 1")] private Transform muzzle = null;
        [SerializeField] private Transform muzzle2 = null;
        [SerializeField, Min(0.01f)] private float shotsPerSecond = 8f;
        [SerializeField, Min(0)] private float damageMultiplier = 1f;
        [SerializeField, Min(0.01f)] private float projectileSpeed = 25f;
        [SerializeField, Min(0.01f)] private float projectileLifetime = 3f;
        [Header("Mouse aim")]
        [Tooltip("Actual rendering Camera, not the CinemachineCamera. Empty uses Camera.main.")]
        [SerializeField] private Camera aimCamera = null;
        [SerializeField] private LayerMask aimLayers = ~0;
        [SerializeField, Min(1)] private float aimDistance = 1000f;
        private float nextShot;
        private bool held;
        private float attack;
        private Transform owner;
        private Transform aiTarget;
        private bool aiControlled;

        public void SetTrigger(bool pressed, float shipAttack, Transform shipOwner)
        { held = pressed; attack = shipAttack; owner = shipOwner; aiControlled = false; aiTarget = null; }

        public void SetAITarget(bool pressed, float shipAttack, Transform shipOwner, Transform target)
        { held = pressed; attack = shipAttack; owner = shipOwner; aiControlled = true; aiTarget = target; }

        private void LateUpdate()
        {
            var prefab = projectilePrefabObject != null ? projectilePrefabObject :
                projectilePrefab != null ? projectilePrefab.gameObject : null;
            if (!held || prefab == null || Time.time < nextShot) return;
            if (!TryGetAimPoint(out Vector3 aimPoint)) return;
            if (muzzle == null && muzzle2 == null) Fire(prefab, transform, aimPoint);
            else
            {
                if (muzzle != null) Fire(prefab, muzzle, aimPoint);
                if (muzzle2 != null && muzzle2 != muzzle) Fire(prefab, muzzle2, aimPoint);
            }
            nextShot = Time.time + 1f / Mathf.Max(0.01f, shotsPerSecond);
        }

        private bool TryGetAimPoint(out Vector3 point)
        {
            point = default;
            if (aiControlled)
            {
                if (aiTarget == null || !aiTarget.gameObject.activeInHierarchy) return false;
                point = aiTarget.position;
                return true;
            }
            var camera = aimCamera != null ? aimCamera : Camera.main;
            if (camera == null || Mouse.current == null) return false;
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            if (!camera.pixelRect.Contains(mousePosition)) return false;
            Ray ray = camera.ScreenPointToRay(mousePosition);
            float distance = Mathf.Max(1, aimDistance);
            point = ray.GetPoint(distance);
            Transform ship = owner != null ? owner : transform;
            foreach (var hit in Physics.RaycastAll(ray, distance, aimLayers, QueryTriggerInteraction.Collide))
            {
                if (hit.collider.isTrigger && hit.collider.GetComponent<ShipDamageHitbox>() == null) continue;
                if (hit.transform == ship || hit.transform.IsChildOf(ship)) continue;
                bool isProjectile = false;
                foreach (var behaviour in hit.collider.GetComponentsInParent<MonoBehaviour>())
                    if (behaviour is IShipProjectile) { isProjectile = true; break; }
                if (isProjectile || hit.distance >= distance) continue;
                distance = hit.distance;
                point = hit.point;
            }
            return true;
        }

        private void Fire(GameObject prefab, Transform origin, Vector3 aimPoint)
        {
            Vector3 direction = aimPoint - origin.position;
            Quaternion rotation = direction.sqrMagnitude > 0.000001f
                ? Quaternion.FromToRotation(origin.forward, direction.normalized) * origin.rotation
                : origin.rotation;
            var projectile = Instantiate(prefab, origin.position, rotation);
            foreach (var behaviour in projectile.GetComponentsInChildren<MonoBehaviour>(true))
                if (behaviour is IShipProjectile receiver)
                    receiver.Initialize(owner, attack * damageMultiplier, projectileSpeed, projectileLifetime);
        }

        private void OnValidate()
        {
            if (projectilePrefabObject == null && projectilePrefab != null)
                projectilePrefabObject = projectilePrefab.gameObject;
            if (projectilePrefabObject != null) projectilePrefab = null;
        }

        private void OnDisable() { held = false; aiTarget = null; aiControlled = false; }
    }
}
