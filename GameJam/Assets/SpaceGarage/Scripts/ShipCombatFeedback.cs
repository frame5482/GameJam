using UnityEngine;

namespace SpaceGarage
{
    public sealed class ShipCombatFeedback : MonoBehaviour
    {
        [SerializeField] private GameObject hitPrefab = null;
        [SerializeField] private GameObject hitMonsterPrefab = null;
        [SerializeField] private DamagePopup damageNumberPrefab = null;
        [SerializeField, Min(0.01f)] private float hitLifetime = 2f;
        private static ShipCombatFeedback current;

        private void OnEnable()
        {
            if (current != null && current != this)
                Debug.LogWarning("More than one ShipCombatFeedback is enabled. The most recently enabled one will receive hit events.", this);
            current = this;
        }
        private void OnDisable() { if (current == this) current = null; }

        public static ShipHitSource SourceOf(Transform owner)
        {
            if (owner == null) return ShipHitSource.Unknown;
            if (owner.GetComponentInParent<ShipController>() != null || owner.CompareTag("Player")) return ShipHitSource.Player;
            if (owner.GetComponentInParent<Enemy>() != null) return ShipHitSource.Enemy;
            return ShipHitSource.Unknown;
        }

        public static void Report(ShipHitSource source, bool targetIsPlayer, float appliedDamage, Vector3 point, Vector3 normal)
        {
            if (current == null || appliedDamage <= 0) return;
            if (source == ShipHitSource.Player && !targetIsPlayer)
            {
                current.SpawnHit(current.hitPrefab, point, normal);
                if (current.damageNumberPrefab != null)
                {
                    var popup = Instantiate(current.damageNumberPrefab, point, current.damageNumberPrefab.transform.rotation);
                    popup.Initialize(appliedDamage, Camera.main);
                }
            }
            else if (source == ShipHitSource.Enemy && targetIsPlayer)
                current.SpawnHit(current.hitMonsterPrefab, point, normal);
        }

        private void SpawnHit(GameObject prefab, Vector3 point, Vector3 normal)
        {
            if (prefab == null) return;
            Quaternion rotation = normal.sqrMagnitude > 0.000001f
                ? Quaternion.FromToRotation(Vector3.forward, normal.normalized) * prefab.transform.rotation
                : prefab.transform.rotation;
            var instance = Instantiate(prefab, point, rotation);
            Destroy(instance, Mathf.Max(0.01f, hitLifetime));
        }
    }
}
