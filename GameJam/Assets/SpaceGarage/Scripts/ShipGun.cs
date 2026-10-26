using UnityEngine;

namespace SpaceGarage
{
    public sealed class ShipGun : MonoBehaviour
    {
        [SerializeField] private ShipProjectile projectilePrefab = null;
        [SerializeField] private Transform muzzle = null;
        [SerializeField, Min(0.01f)] private float shotsPerSecond = 8f;
        [SerializeField, Min(0)] private float damageMultiplier = 1f;
        [SerializeField, Min(0.01f)] private float projectileSpeed = 25f;
        [SerializeField, Min(0.01f)] private float projectileLifetime = 3f;
        private float nextShot;
        private bool held;
        private float attack;
        private Transform owner;

        public void SetTrigger(bool pressed, float shipAttack, Transform shipOwner)
        { held = pressed; attack = shipAttack; owner = shipOwner; }

        private void LateUpdate()
        {
            if (!held || projectilePrefab == null || Time.time < nextShot) return;
            var origin = muzzle != null ? muzzle : transform;
            var projectile = Instantiate(projectilePrefab, origin.position, origin.rotation);
            projectile.Initialize(owner, attack * damageMultiplier, projectileSpeed, projectileLifetime);
            nextShot = Time.time + 1f / Mathf.Max(0.01f, shotsPerSecond);
        }

        private void OnDisable() => held = false;
    }
}
