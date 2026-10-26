using UnityEngine;

namespace SpaceGarage
{
    // Sweeps along the path to avoid tunnelling. No Rigidbody is required on the projectile.
    public sealed class ShipProjectile : MonoBehaviour, IShipProjectile
    {
        [SerializeField, Min(0.001f)] private float hitRadius = 0.12f;
        [SerializeField] private LayerMask hitLayers = ~0;
        private Transform owner;
        private float damage;
        private float speed;
        private float expiresAt;
        private bool initialized;
        private ShipHitSource hitSource;

        public void Initialize(Transform source, float attack, float velocity, float lifetime)
        {
            owner = source; damage = Mathf.Max(0, attack); speed = Mathf.Max(0, velocity);
            hitSource = ShipCombatFeedback.SourceOf(source);
            expiresAt = Time.time + Mathf.Max(0.01f, lifetime); initialized = true;
        }

        private void Update()
        {
            if (!initialized) return;
            if (Time.time >= expiresAt) { Destroy(gameObject); return; }
            float distance = speed * Time.deltaTime;
            Collider nearest = null;
            float nearestDistance = float.PositiveInfinity;
            Vector3 hitPoint = transform.position;
            Vector3 hitNormal = -transform.forward;
            foreach (var overlap in Physics.OverlapSphere(transform.position, hitRadius, hitLayers, QueryTriggerInteraction.Collide))
                if (CanHit(overlap))
                {
                    nearest = overlap; nearestDistance = 0;
                    hitPoint = overlap.ClosestPoint(transform.position);
                    Vector3 normal = transform.position - hitPoint;
                    hitNormal = normal.sqrMagnitude > 0.000001f ? normal.normalized : -transform.forward;
                    break;
                }
            if (nearest == null && distance > 0)
            {
                foreach (var hit in Physics.SphereCastAll(transform.position, hitRadius, transform.forward, distance,
                             hitLayers, QueryTriggerInteraction.Collide))
                    if (CanHit(hit.collider) && hit.distance < nearestDistance)
                    { nearest = hit.collider; nearestDistance = hit.distance; hitPoint = hit.point; hitNormal = hit.normal; }
            }
            if (nearest != null)
            {
                foreach (var behaviour in nearest.GetComponentsInParent<MonoBehaviour>())
                    if (behaviour is IShipDamageable target)
                    {
                        if (target is IShipHitReceiver receiver) receiver.TakeHit(damage, hitSource, hitPoint, hitNormal);
                        else target.TakeDamage(damage);
                        break;
                    }
                Destroy(gameObject);
                return;
            }
            transform.position += transform.forward * distance;
        }

        private bool CanHit(Collider target)
        {
            if (target.isTrigger && target.GetComponent<ShipDamageHitbox>() == null) return false;
            if (target.transform == transform || target.transform.IsChildOf(transform)) return false;
            if (owner != null && (target.transform == owner || target.transform.IsChildOf(owner))) return false;
            if (target.GetComponentInParent<ShipProjectile>() != null) return false;
            return true;
        }
    }
}
