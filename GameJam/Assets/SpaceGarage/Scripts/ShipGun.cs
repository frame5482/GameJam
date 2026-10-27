using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

namespace SpaceGarage
{
    // Aim after Cinemachine has updated the rendered camera for this frame.
    [DefaultExecutionOrder(1100)]
    public sealed class ShipGun : MonoBehaviour
    {
        [SerializeField] private ShipWeaponType weaponType = ShipWeaponType.Projectile;
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
        [Header("Laser / Rail Gun")]
        [SerializeField] private VisualEffectAsset cataclysmBeam = null;
        [SerializeField, Min(0)] private float laserSize = 10;
        [SerializeField, Min(0)] private float railSize = 50;
        [SerializeField, Min(0.01f)] private float railGrowDuration = 0.5f;
        [SerializeField, ColorUsage(true, true)] private Color laserColor = new Color(89.4541015625f, 5.4741287231f, 0f, 1f);
        [SerializeField, ColorUsage(true, true)] private Color laserSecondaryColor = new Color(0f, 117.625579834f, 109.947380066f, 1f);
        [SerializeField, ColorUsage(true, true)] private Color railColor = new Color(0f, 45f, 110f, 1f);
        [SerializeField, ColorUsage(true, true)] private Color railSecondaryColor = new Color(0f, 45f, 110f, 1f);
        [Tooltip("Time between laser hits. Each hit uses ship Attack x Damage Multiplier, like a projectile.")]
        [SerializeField, Min(0.01f)] private float laserDamageInterval = 0.1f;
        [SerializeField, Min(0.01f)] private float laserRange = 10;
        [SerializeField, Min(0)] private float laserDamageRadius = 3f;
        [SerializeField, Min(0.01f)] private float railRange = 50;
        [Tooltip("Beam contact radius at full Rail Size. Scales with the beam growth; 0 uses a thin ray.")]
        [SerializeField, Min(0)] private float railDamageRadius = 3f;
        [Header("Plasma / Getsuka projectiles")]
        [SerializeField] private GameObject plasmaProjectilePrefab = null;
        [SerializeField] private GameObject getsukaProjectilePrefab = null;
        [Tooltip("Used when the supplied heavy projectile has no IShipProjectile script.")]
        [SerializeField, Min(0.001f)] private float heavyProjectileHitRadius = 0.8f;
        private float nextShot;
        private bool held;
        private float attack;
        private Transform owner;
        private Transform aiTarget;
        private bool aiControlled;
        private readonly ShipWeaponCycle cycle = new ShipWeaponCycle();
        private readonly ShipBeamEmitter beam1 = new ShipBeamEmitter();
        private readonly ShipBeamEmitter beam2 = new ShipBeamEmitter();
        private float laserDamageTime;
        private bool laserFiring;
        private readonly HashSet<IShipDamageable> railHitTargets = new HashSet<IShipDamageable>();
        private float railGrowth = 1f;

        public void SetTrigger(bool pressed, float shipAttack, Transform shipOwner)
        { held = pressed; attack = shipAttack; owner = shipOwner; aiControlled = false; aiTarget = null; }

        public void SetAITarget(bool pressed, float shipAttack, Transform shipOwner, Transform target)
        { held = pressed; attack = shipAttack; owner = shipOwner; aiControlled = true; aiTarget = target; }

        private void LateUpdate()
        {
            if (weaponType != ShipWeaponType.Laser) { laserFiring = false; laserDamageTime = 0; }
            if (weaponType != ShipWeaponType.Projectile) { UpdateSpecialWeapon(); return; }
            HideBeams();
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
            UISfxManager.PlayWeaponShot(weaponType, transform);
            nextShot = Time.time + 1f / Mathf.Max(0.01f, shotsPerSecond);
        }

        private void UpdateSpecialWeapon()
        {
            var playerShip = owner != null ? owner.GetComponent<ShipController>() : null;
            var enemyShip = owner != null ? owner.GetComponent<Enemy>() : null;
            if ((playerShip != null && !playerShip.IsAlive) || (enemyShip != null && !enemyShip.IsAlive))
            { cycle.StopPulse(); HideBeams(); laserDamageTime = 0; return; }
            if (weaponType == ShipWeaponType.Laser)
            {
                if (!held || cataclysmBeam == null || Time.deltaTime <= 0)
                { HideBeams(); laserFiring = false; laserDamageTime = 0; return; }
                ShowBeams(laserSize);
                UISfxManager.SetLaserLoop(this, true);
                if (!laserFiring)
                {
                    laserFiring = true;
                    laserDamageTime = 0;
                    DamageBeams(laserRange, attack * damageMultiplier);
                }
                laserDamageTime += Time.deltaTime;
                float interval = Mathf.Max(0.01f, laserDamageInterval);
                while (laserDamageTime >= interval)
                {
                    DamageBeams(laserRange, attack * damageMultiplier);
                    laserDamageTime -= interval;
                }
                return;
            }
            if (weaponType == ShipWeaponType.RailGun)
            {
                if (Time.deltaTime > 0 && held && cataclysmBeam != null && cycle.TryFire(Time.time, 1, 1))
                { railHitTargets.Clear(); UISfxManager.PlayWeaponShot(weaponType, transform); }
                if (Time.deltaTime > 0 && cycle.IsPulsing(Time.time) && (!aiControlled || held))
                {
                    railGrowth = Mathf.Clamp01((Time.time - cycle.PulseStartedAt) / Mathf.Max(0.01f, railGrowDuration));
                    ShowBeams(railSize * railGrowth);
                    if (railGrowth > 0) DamageBeams(railRange, attack * damageMultiplier);
                }
                else { HideBeams(); if (aiControlled && !held) cycle.StopPulse(); }
                return;
            }
            HideBeams();
            var prefab = weaponType == ShipWeaponType.Plasma ? plasmaProjectilePrefab : getsukaProjectilePrefab;
            if (!held || prefab == null || !TryGetAimPoint(out Vector3 point)) return;
            if (!cycle.TryFire(Time.time, 0, weaponType == ShipWeaponType.Plasma ? 2 : 3)) return;
            UISfxManager.PlayWeaponShot(weaponType, transform);
            if (muzzle == null && muzzle2 == null) Fire(prefab, transform, point, true);
            else
            {
                if (muzzle != null) Fire(prefab, muzzle, point, true);
                if (muzzle2 != null && muzzle2 != muzzle) Fire(prefab, muzzle2, point, true);
            }
        }

        private void ShowBeams(float size)
        {
            Color color = weaponType == ShipWeaponType.RailGun ? railColor : laserColor;
            Color secondary = weaponType == ShipWeaponType.RailGun ? railSecondaryColor : laserSecondaryColor;
            float pivot = weaponType == ShipWeaponType.RailGun ? -0.5f : 0f;
            if (muzzle == null && muzzle2 == null) { beam1.Show(cataclysmBeam, transform, transform, size, color, secondary, BeamRotation(transform), pivot); beam2.Hide(); }
            else
            {
                if (muzzle != null) beam1.Show(cataclysmBeam, transform, muzzle, size, color, secondary, BeamRotation(muzzle), pivot); else beam1.Hide();
                if (muzzle2 != null && muzzle2 != muzzle) beam2.Show(cataclysmBeam, transform, muzzle2, size, color, secondary, BeamRotation(muzzle2), pivot); else beam2.Hide();
            }
        }
        private void HideBeams() { beam1.Hide(); beam2.Hide(); UISfxManager.StopLoop(this); }

        private Vector3 BeamDirection(Transform origin) => origin.forward;
        private Quaternion BeamRotation(Transform origin) => origin.rotation;

        private void DamageBeams(float range, float damage)
        {
            if (damage <= 0) return;
            if (muzzle == null && muzzle2 == null) DamageBeam(transform, range, damage);
            else
            {
                if (muzzle != null) DamageBeam(muzzle, range, damage);
                if (muzzle2 != null && muzzle2 != muzzle) DamageBeam(muzzle2, range, damage);
            }
        }
        private void DamageBeam(Transform origin, float range, float damage)
        {
            Vector3 direction = BeamDirection(origin);
            float radius = weaponType == ShipWeaponType.RailGun
                ? Mathf.Max(0, railDamageRadius) * Mathf.Clamp01(railGrowth)
                : weaponType == ShipWeaponType.Laser ? Mathf.Max(0, laserDamageRadius) : 0;
            radius = Mathf.Min(radius, Mathf.Max(0, range));
            // Raycasts omit colliders containing their origin. A moving muzzle can enter
            // the enemy hitbox during a held beam; keep applying ticks while inside it.
            foreach (var overlap in Physics.OverlapSphere(origin.position, Mathf.Max(0.001f, radius), aimLayers, QueryTriggerInteraction.Collide))
            {
                if (!CanAimAt(overlap)) continue;
                Vector3 point = overlap.ClosestPoint(origin.position);
                Vector3 offset = point - origin.position;
                if (offset.sqrMagnitude > Mathf.Max(0.000001f, radius * radius) || Vector3.Dot(offset, direction) < -0.001f) continue;
                ApplyBeamDamage(overlap, damage, point, -direction);
                return;
            }
            RaycastHit nearest = default;
            float distance = float.PositiveInfinity;
            var hits = radius > 0
                ? Physics.SphereCastAll(origin.position, radius, direction, Mathf.Max(0, range - radius), aimLayers, QueryTriggerInteraction.Collide)
                : Physics.RaycastAll(origin.position, direction, range, aimLayers, QueryTriggerInteraction.Collide);
            foreach (var hit in hits)
            {
                if (!CanAimAt(hit.collider) || hit.distance >= distance) continue;
                nearest = hit; distance = hit.distance;
            }
            if (nearest.collider == null) return;
            ApplyBeamDamage(nearest.collider, damage, nearest.point, nearest.normal);
        }
        private void ApplyBeamDamage(Collider targetCollider, float damage, Vector3 point, Vector3 normal)
        {
            foreach (var behaviour in targetCollider.GetComponentsInParent<MonoBehaviour>())
                if (behaviour is IShipDamageable target)
                {
                    if (weaponType == ShipWeaponType.RailGun && !railHitTargets.Add(target)) return;
                    if (target is IShipHitReceiver receiver)
                        receiver.TakeHit(damage, ShipCombatFeedback.SourceOf(owner), point, normal);
                    else target.TakeDamage(damage);
                    break;
                }
        }
        private bool CanAimAt(Collider collider)
        {
            if (collider.isTrigger && collider.GetComponent<ShipDamageHitbox>() == null) return false;
            Transform ship = owner != null ? owner : transform;
            if (collider.transform == ship || collider.transform.IsChildOf(ship)) return false;
            foreach (var behaviour in collider.GetComponentsInParent<MonoBehaviour>())
                if (behaviour is IShipProjectile) return false;
            return true;
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

        private void Fire(GameObject prefab, Transform origin, Vector3 aimPoint, bool heavy = false)
        {
            Vector3 direction = aimPoint - origin.position;
            Quaternion rotation = direction.sqrMagnitude > 0.000001f
                ? Quaternion.FromToRotation(origin.forward, direction.normalized) * origin.rotation
                : origin.rotation;
            if (heavy && weaponType == ShipWeaponType.Getsuka && direction.sqrMagnitude > 0.000001f)
                rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            var projectile = Instantiate(prefab, origin.position, rotation);
            bool hasReceiver = false;
            foreach (var behaviour in projectile.GetComponentsInChildren<MonoBehaviour>(true))
                if (behaviour is IShipProjectile receiver)
                { hasReceiver = true; receiver.Initialize(owner, attack * damageMultiplier, projectileSpeed, projectileLifetime); }
            if (heavy && !hasReceiver)
            {
                var motion = projectile.AddComponent<ShipProjectile>();
                motion.SetHitRadius(heavyProjectileHitRadius);
                motion.Initialize(owner, attack * damageMultiplier, projectileSpeed, projectileLifetime);
            }
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            if ((weaponType == ShipWeaponType.Laser || weaponType == ShipWeaponType.RailGun) && cataclysmBeam == null)
                cataclysmBeam = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualEffectAsset>("Assets/VFX/JET/BulletBeam/CataclysmBeam.vfx");
            if (weaponType == ShipWeaponType.Getsuka && getsukaProjectilePrefab == null)
                getsukaProjectilePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VFX/Getsuka/Getsuka.prefab");
#endif
            if (projectilePrefabObject == null && projectilePrefab != null)
                projectilePrefabObject = projectilePrefab.gameObject;
            if (projectilePrefabObject != null) projectilePrefab = null;
        }

        private void OnDisable()
        {
            UISfxManager.StopLoop(this);
            held = false; aiTarget = null; aiControlled = false;
            railHitTargets.Clear();
            cycle.StopPulse(); laserDamageTime = 0; laserFiring = false;
            beam1.Dispose(); beam2.Dispose();
        }
    }
}
