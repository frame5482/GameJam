using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace SpaceGarage
{
    [RequireComponent(typeof(Rigidbody), typeof(EnemyAttackTelegraph))]
    public sealed class Enemy : MonoBehaviour, IShipHitReceiver
    {
        [Header("Base stats plus the four equipped parts")]
        [SerializeField] private ShipStats baseStats = new ShipStats
        { attack = 5, defense = 0, speed = 10, handling = 60, health = 40 };
        [SerializeField] private ShipPartStats gunPart;
        [SerializeField] private ShipPartStats armorPlatePart;
        [SerializeField] private ShipPartStats wingPart;
        [SerializeField] private ShipPartStats jetPart;
        [Header("One material awarded on defeat, from workshop pools")]
        [SerializeField] private RepairMaterialPool[] dropMaterialPools = new RepairMaterialPool[0];
        [Header("Part spawn points")]
        [SerializeField] private Transform gunMount = null;
        [SerializeField] private Transform armorPlateMount = null;
        [SerializeField] private Transform wingMount = null;
        [SerializeField] private Transform jetMount = null;
        [Header("Chase and fire")]
        [SerializeField, Min(0)] private float fireRange = 30f;
        [SerializeField, Range(0, 180)] private float fireAngle = 15f;
        [SerializeField, Range(0, 1)] private float closeRangeSpeedMultiplier = 0.25f;
        [SerializeField, Min(0.01f)] private float speedResponseTime = 0.6f;
        [SerializeField, Min(0)] private float attackWarningTime = 0.35f;
        [SerializeField, Min(0.01f)] private float fireBurstDuration = 1f;
        [SerializeField, Min(0)] private float firePauseDuration = 1.5f;
        [SerializeField] private Animator animator = null;
        [Header("Front-facing retreat fire (available to every enemy)")]
        [SerializeField, Min(0)] private float frontKeepDistance = 20f;
        [SerializeField, Min(0.01f)] private float frontDistanceResponse = 5f;
        [Header("Enter the player's view when nearby")]
        [SerializeField] private Camera playerCamera = null;
        [SerializeField, Min(0)] private float screenEntryRange = 45f;
        [SerializeField, Min(1)] private float screenEntryForwardDistance = 15f;
        [SerializeField, Range(0, 0.4f)] private float screenEdgeMargin = 0.08f;
        [SerializeField, Range(0.01f, 0.45f)] private float screenReturnMargin = 0.2f;
        [Header("Hit reaction")]
        [SerializeField, Min(0)] private float hitReactionDuration = 0.15f;
        [SerializeField, Min(0)] private float hitKnockbackSpeed = 3f;
        public UnityEvent died = new UnityEvent();
        public static event System.Action<Enemy, ShipHitSource> Defeated;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetDefeatedListeners() => Defeated = null;

        public ShipStats Stats { get; private set; }
        public float Health { get; private set; }
        public bool IsAlive => configured && Health > 0;
        private Rigidbody body;
        private readonly ShipBarrierState barrier = new ShipBarrierState();
        public float Barrier => barrier.Current;
        private EnemyAttackTelegraph telegraph;
        private Transform target;
        private ShipController targetShip;
        private ShipGun[] guns = new ShipGun[0];
        private ShipBoostEffect[] engineEffects = new ShipBoostEffect[0];
        private readonly GameObject[] parts = new GameObject[4];
        private sealed class Surface
        {
            public Renderer renderer;
            public int index;
            public MaterialPropertyBlock original, flash;
        }
        private readonly List<Surface> surfaces = new List<Surface>();
        private bool configured, flashing, firing;
        private float nextTargetSearch, hitEndsAt;
        private readonly EnemyFireCycle fireCycle = new EnemyFireCycle();
        private Vector3 flightVelocity, flightVelocitySmoothing;
        private Vector3 knockback;
        private bool enteringView;
        private Vector2 entryViewport;
        public bool IsEnteringView => enteringView;
        public bool IsRetreating { get; private set; }

        private void Awake()
        {
            CacheComponents();
            if (GetComponent<ShipController>() != null)
            {
                Debug.LogError("Enemy prefab must not also contain ShipController.", this);
                enabled = false;
            }
        }

        private void CacheComponents()
        {
            if (body == null) body = GetComponent<Rigidbody>();
            body.useGravity = false;
            body.constraints = RigidbodyConstraints.None;
            if (telegraph == null) telegraph = GetComponent<EnemyAttackTelegraph>();
            if (telegraph == null) telegraph = gameObject.AddComponent<EnemyAttackTelegraph>();
        }

        private void Start()
        {
            if (!configured) Initialize(gunPart, armorPlatePart, wingPart, jetPart, null);
        }

        public bool Initialize(ShipPartStats gun, ShipPartStats armor, ShipPartStats wing, ShipPartStats jet, Transform player)
        {
            if (!Valid(gun, ShipPartSlot.Gun) || !Valid(armor, ShipPartSlot.ArmorPlate) ||
                !Valid(wing, ShipPartSlot.Wing) || !Valid(jet, ShipPartSlot.Jet))
            { Debug.LogWarning("Enemy needs four valid part assets with matching slots and prefabs.", this); return false; }
            CacheComponents();
            StopFiring();
            RestoreFlash();
            for (int i = 0; i < parts.Length; i++)
                if (parts[i] != null) { parts[i].SetActive(false); Destroy(parts[i]); parts[i] = null; }
            gunPart = gun; armorPlatePart = armor; wingPart = wing; jetPart = jet;
            var palette = ShipRandomColors.CreatePalette();
            CreatePart(gun, gunMount, palette); CreatePart(armor, armorPlateMount, palette);
            CreatePart(wing, wingMount, palette); CreatePart(jet, jetMount, palette);
            Stats = baseStats + gun.stats + armor.stats + wing.stats + jet.stats;
            Stats = ShipPartTraits.Apply(Stats, armor, wing, jet, false);
            Health = Mathf.Max(1, Stats.health);
            configured = true;
            fireCycle.Reset();
            flightVelocity = flightVelocitySmoothing = Vector3.zero;
            enteringView = false;
            IsRetreating = false;
            hitEndsAt = 0;
            guns = GetComponentsInChildren<ShipGun>(false);
            engineEffects = GetComponentsInChildren<ShipBoostEffect>(true);
            if (animator == null) animator = GetComponentInChildren<Animator>();
            surfaces.Clear();
            foreach (var renderer in GetComponentsInChildren<Renderer>(false))
            {
                if (!(renderer is MeshRenderer) && !(renderer is SkinnedMeshRenderer)) continue;
                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    var original = new MaterialPropertyBlock(); var flash = new MaterialPropertyBlock();
                    renderer.GetPropertyBlock(original, i); renderer.GetPropertyBlock(flash, i);
                    flash.SetColor("_BaseColor", Color.white); flash.SetColor("_Color", Color.white);
                    flash.SetColor("_1st_ShadeColor", Color.white); flash.SetColor("_2nd_ShadeColor", Color.white);
                    surfaces.Add(new Surface { renderer = renderer, index = i, original = original, flash = flash });
                }
            }
            SetTarget(player);
            barrier.Configure(armor, transform);
            foreach (var effect in engineEffects) if (effect != null) effect.SetFlightState(true, false);
            return true;
        }

        public static bool Valid(ShipPartStats part, ShipPartSlot slot) =>
            part != null && part.slot == slot && part.partPrefab != null &&
            part.partPrefab.GetComponentInChildren<ShipController>(true) == null &&
            part.partPrefab.GetComponentInChildren<Enemy>(true) == null;

        private void CreatePart(ShipPartStats data, Transform mount, ShipRandomColors.Palette palette)
        {
            if (mount != null && mount != transform && !mount.IsChildOf(transform)) mount = null;
            var instance = Instantiate(data.partPrefab, mount != null ? mount : transform, false);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.name = data.slot + "_" + data.partPrefab.name;
            ShipRandomColors.Apply(instance, palette, data.slot);
            parts[(int)data.slot] = instance;
        }

        private void SetTarget(Transform player)
        {
            target = player;
            targetShip = player != null ? player.GetComponentInParent<ShipController>() : null;
        }

        private void Update()
        {
            if (!IsAlive) return;
            if ((target == null || !target.gameObject.activeInHierarchy) && Time.time >= nextTargetSearch)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                SetTarget(player != null ? player.transform : null);
                nextTargetSearch = Time.time + 0.5f;
            }
            if (target == null || (targetShip != null && !targetShip.IsAlive) || Time.time < hitEndsAt)
            { StopFiring(); return; }
            Vector3 offset = target.position - body.position;
            UpdateScreenEntry(offset.sqrMagnitude);
            bool canFire = offset.sqrMagnitude <= fireRange * fireRange && offset.sqrMagnitude > 0.001f &&
                Vector3.Angle(transform.forward, offset) <= fireAngle;
            if (!canFire) { StopFiring(); return; }
            EnemyFirePhase phase = fireCycle.Tick(Time.timeAsDouble, true, attackWarningTime, fireBurstDuration, firePauseDuration);
            if (phase == EnemyFirePhase.Warning || phase == EnemyFirePhase.Firing)
                telegraph.Show(gunMount != null ? gunMount.position : transform.position, target.position);
            else telegraph.Hide();
            bool ready = phase == EnemyFirePhase.Firing;
            if (ready && !firing && animator != null)
                foreach (var parameter in animator.parameters)
                    if (parameter.name == "Attack" && parameter.type == AnimatorControllerParameterType.Trigger)
                    { animator.SetTrigger("Attack"); break; }
            firing = ready;
            foreach (var gun in guns) if (gun != null) gun.SetAITarget(ready, Mathf.Max(0, Stats.attack), transform, target);
        }

        private void FixedUpdate()
        {
            barrier.Tick(IsAlive, Time.fixedDeltaTime);
            if (!IsAlive) return;
            body.angularVelocity = Vector3.zero;
            if (Time.time < hitEndsAt)
            {
                IsRetreating = false;
                body.MovePosition(body.position + knockback * Time.fixedDeltaTime);
                knockback = Vector3.MoveTowards(knockback, Vector3.zero, hitKnockbackSpeed / Mathf.Max(0.01f, hitReactionDuration) * Time.fixedDeltaTime);
                return;
            }
            if (target == null || !target.gameObject.activeInHierarchy || (targetShip != null && !targetShip.IsAlive))
            { body.linearVelocity = Vector3.zero; flightVelocity = flightVelocitySmoothing = Vector3.zero; IsRetreating = false; return; }
            Vector3 offset = target.position - body.position;
            if (offset.sqrMagnitude < 0.001f) return;
            UpdateScreenEntry(offset.sqrMagnitude);
            Vector3 flightOffset = enteringView ? ScreenEntryPosition() - body.position : offset;
            if (flightOffset.sqrMagnitude < 0.001f) return;
            // Aim at the player independently of the path used to enter the screen or retreat.
            Quaternion rotation = Quaternion.RotateTowards(body.rotation, Quaternion.LookRotation(offset.normalized, Vector3.up),
                Mathf.Max(0, Stats.handling) * Time.fixedDeltaTime);
            float distance = offset.magnitude;
            bool inFront = Vector3.Dot(-offset / distance, target.forward) > 0.1f;
            float factor = 1;
            Vector3 travelDirection = rotation * Vector3.forward;
            if (enteringView)
                travelDirection = flightOffset.normalized;
            else if (inFront && distance <= fireRange)
            {
                float desiredDistance = Mathf.Min(frontKeepDistance, fireRange * 0.8f);
                factor = EnemyCombatMotion.FrontSpeedFactor(distance, desiredDistance, frontDistanceResponse);
                travelDirection = offset / distance;
            }
            else if (distance <= fireRange)
            {
                // Keep pursuing inside firing range instead of falling behind at a fixed quarter-speed.
                factor = Mathf.Lerp(closeRangeSpeedMultiplier, 1, Mathf.Clamp01(distance / Mathf.Max(0.01f, fireRange)));
            }
            IsRetreating = !enteringView && inFront && factor < 0;
            Vector3 desiredVelocity = travelDirection * (Mathf.Max(0, Stats.speed) * factor);
            flightVelocity = Vector3.SmoothDamp(flightVelocity, desiredVelocity, ref flightVelocitySmoothing,
                speedResponseTime, Mathf.Infinity, Time.fixedDeltaTime);
            body.MoveRotation(rotation);
            body.MovePosition(body.position + flightVelocity * Time.fixedDeltaTime);
        }

        private void UpdateScreenEntry(float distanceSquared)
        {
            if (playerCamera == null) playerCamera = Camera.main;
            if (playerCamera == null || !playerCamera.isActiveAndEnabled ||
                distanceSquared > screenEntryRange * screenEntryRange)
            { enteringView = false; return; }
            Vector3 viewport = playerCamera.WorldToViewportPoint(body.position);
            float returnMargin = Mathf.Max(screenEdgeMargin, screenReturnMargin);
            if (enteringView)
            {
                if (InsideViewport(viewport, returnMargin)) enteringView = false;
                return;
            }
            if (InsideViewport(viewport, screenEdgeMargin)) return;
            enteringView = true;
            float goalMargin = Mathf.Clamp(Mathf.Max(0.25f, returnMargin + 0.05f), 0.25f, 0.49f);
            if (viewport.z <= playerCamera.nearClipPlane)
            {
                float side = Vector3.Dot(body.position - target.position, playerCamera.transform.right);
                entryViewport = new Vector2(Mathf.Clamp(side < 0 ? 0.35f : 0.65f, goalMargin, 1 - goalMargin), 0.5f);
            }
            else
                entryViewport = new Vector2(Mathf.Clamp(viewport.x, goalMargin, 1 - goalMargin), Mathf.Clamp(viewport.y, goalMargin, 1 - goalMargin));
        }

        private bool InsideViewport(Vector3 point, float margin) => point.z > playerCamera.nearClipPlane && point.z < playerCamera.farClipPlane &&
            point.x >= margin && point.x <= 1 - margin && point.y >= margin && point.y <= 1 - margin;

        private Vector3 ScreenEntryPosition()
        {
            float playerDepth = playerCamera.WorldToViewportPoint(target.position).z;
            float depth = Mathf.Max(playerCamera.nearClipPlane + 2, playerDepth + screenEntryForwardDistance);
            return playerCamera.ViewportToWorldPoint(new Vector3(entryViewport.x, entryViewport.y, depth));
        }

        private void LateUpdate() { if (flashing && Time.time >= hitEndsAt) RestoreFlash(); }

        public void TakeDamage(float amount)
            => TakeHit(amount, ShipHitSource.Unknown, transform.position, -transform.forward);

        public void TakeHit(float amount, ShipHitSource source, Vector3 point, Vector3 normal)
        {
            if (!IsAlive || amount <= 0) return;
            float damage = Mathf.Max(0, amount - Mathf.Max(0, Stats.defense));
            damage = barrier.Absorb(damage);
            if (damage <= 0) return;
            float previousHealth = Health;
            Health = Mathf.Max(0, Health - damage);
            ShipCombatFeedback.Report(source, false, previousHealth - Health, point, normal);
            StopFiring();
            hitEndsAt = Time.time + hitReactionDuration;
            knockback = (target != null ? transform.position - target.position : -transform.forward).normalized * hitKnockbackSpeed *
                (wingPart != null ? wingPart.knockbackMultiplier : 1f);
            foreach (var surface in surfaces)
                if (surface.renderer != null) surface.renderer.SetPropertyBlock(surface.flash, surface.index);
            flashing = true;
            if (Health > 0) return;
            AwardMaterialDrop();
            Defeated?.Invoke(this, source);
            body.linearVelocity = body.angularVelocity = Vector3.zero;
            foreach (var effect in engineEffects) if (effect != null) effect.SetFlightState(false, false);
            died.Invoke();
            gameObject.SetActive(false);
            Destroy(gameObject);
        }

        private void AwardMaterialDrop()
        {
            var resources = ResourceManager.Instance;
            var storage = targetShip != null ? targetShip.Inventory : resources != null ? resources.Inventory : null;
            if (storage == null)
            { Debug.LogWarning("Enemy material drop needs the player's inventory or ResourceManager.", this); return; }
            var options = new List<ShipMaterialData>();
            var ids = new HashSet<string>();
            foreach (var pool in dropMaterialPools)
            {
                if (pool == null) continue;
                foreach (var option in pool.ValidOptions())
                    if (ids.Add(option.material.itemId)) options.Add(option.material);
            }
            if (options.Count == 0)
            { Debug.LogWarning("Enemy Drop Material Pools need valid workshop materials.", this); return; }
            var material = options[Random.Range(0, options.Count)];
            if (resources != null) resources.RegisterMaterial(material);
            if (storage.Add(material.itemId, 1))
                storage.NotifyPickupCollected(material, material.itemId, 1);
        }

        private void StopFiring()
        {
            fireCycle.Interrupt(Time.timeAsDouble, firePauseDuration);
            firing = false;
            if (telegraph != null) telegraph.Hide();
            foreach (var gun in guns) if (gun != null) gun.SetAITarget(false, 0, transform, null);
        }

        private void RestoreFlash()
        {
            if (!flashing) return;
            foreach (var surface in surfaces)
                if (surface.renderer != null) surface.renderer.SetPropertyBlock(surface.original, surface.index);
            flashing = false;
        }

        private void OnDisable()
        {
            barrier.SetVisible(false);
            StopFiring(); RestoreFlash();
            foreach (var effect in engineEffects) if (effect != null) effect.SetFlightState(false, false);
        }
        private void OnDestroy() => barrier.Dispose();
    }
}
