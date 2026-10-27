using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace SpaceGarage
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ShipController : MonoBehaviour, IShipHitReceiver
    {
        [Header("Base stats + four equipped parts")]
        [SerializeField] private ShipStats baseStats = new ShipStats
        { attack = 10, defense = 0, speed = 8, handling = 360, health = 100, boosterCapacity = 100 };
        [SerializeField] private ShipPartStats gunPart;
        [SerializeField] private ShipPartStats armorPlatePart;
        [SerializeField] private ShipPartStats wingPart;
        [SerializeField] private ShipPartStats jetPart;
        [Header("Assembly mounts (local +Z is forward; empty mounts use ship root)")]
        [SerializeField] private Transform gunMount = null;
        [SerializeField] private Transform armorPlateMount = null;
        [SerializeField] private Transform wingMount = null;
        [SerializeField] private Transform jetMount = null;
        [Header("3D aircraft banking")]
        [SerializeField, Range(0, 70)] private float bankAngle = 25f;
        [SerializeField, Min(0)] private float bankDegreesPerSecond = 75f;
        [Header("Weighted flight response")]
        [SerializeField, Range(1, 80)] private float maxPitchAngle = 30f;
        [SerializeField, Min(0)] private float pitchDegreesPerSecond = 36f;
        [SerializeField, Min(0)] private float turnDegreesPerSecond = 54f;
        [SerializeField, Min(0.01f)] private float steeringResponseTime = 0.32f;
        [SerializeField, Min(0.01f)] private float speedResponseTime = 0.6f;
        [Header("Boost steering assistance")]
        [SerializeField, Min(1)] private float boostSteeringMultiplier = 2f;
        [SerializeField, Range(0.1f, 1)] private float boostSteeringResponseMultiplier = 0.5f;
        [Header("Dodge roll (Space + A/D)")]
        [SerializeField, Min(0.01f)] private float dodgeDuration = 1.2f;
        [SerializeField, Min(0.01f)] private float dodgeDashDuration = 0.6f;
        [SerializeField, Min(0)] private float dodgeCooldown = 1f;
        [SerializeField, Min(0)] private float dodgeDistance = 5f;
        [Header("References")]
        [SerializeField] private ShipGun[] guns;
        [SerializeField] private ShipInventory inventory;
        [Header("Pickup")]
        [SerializeField, Min(0)] private float pickupRadius = 3f;
        [SerializeField] private LayerMask pickupLayers = ~0;
        [Header("Events (optional existing UI / game flow)")]
        public UnityEvent<float, float> healthChanged = new UnityEvent<float, float>();
        public UnityEvent<float, float> boostChanged = new UnityEvent<float, float>();
        public UnityEvent died = new UnityEvent();

        public ShipStats Stats { get; private set; }
        public float Health { get; private set; }
        public float Boost { get; private set; }
        public bool IsAlive => Health > 0;
        public bool IsBoosting { get; private set; }
        public ShipInventory Inventory => ResourceManager.Instance != null ? ResourceManager.Instance.Inventory : inventory;
        public float CurrentSpeed => flightSpeed;
        public bool IsRolling => dodge.IsInvulnerable(Time.timeAsDouble);
        public bool IsInvulnerable => IsAlive && IsRolling;
        public float DodgeCooldown => Mathf.Max(0, dodgeCooldown);
        public float DodgeCooldownRemaining => Mathf.Min(DodgeCooldown, (float)dodge.CooldownRemaining(Time.timeAsDouble));
        public bool CanDodge => isActiveAndEnabled && IsAlive && !IsRolling && dodge.CooldownRemaining(Time.timeAsDouble) <= 0;
        public float DodgeReadyProgress => !isActiveAndEnabled || !IsAlive || IsRolling ? 0 :
            DodgeCooldown > 0 ? Mathf.Clamp01(1 - DodgeCooldownRemaining / DodgeCooldown) : 1;

        private Rigidbody body;
        private readonly ShipBarrierState barrier = new ShipBarrierState();
        public float Barrier => barrier.Current;
        public float BarrierCapacity => barrier.Capacity;
        public bool AlternateMode { get; private set; }
        private int dodgeChargesUsed;
        private double dodgeRechargeAt;
        private bool wasBoosting;
        private float boostSlowUntil;
        private Vector2 flightInput;
        private float yaw;
        private float pitch;
        private float pitchRate;
        private float turnRate;
        private float pitchRateVelocity;
        private float turnRateVelocity;
        private float flightSpeed;
        private float speedVelocity;
        private bool boostHeld;
        private bool fireHeld;
        private bool boostExhausted;
        private float recoveryAt;
        private float currentBank;
        private readonly ShipDodgeState dodge = new ShipDodgeState();
        private Vector3 dodgeDirectionWorld;
        private float dodgeDistanceMoved;
        private ShipBoostEffect[] boostEffects = new ShipBoostEffect[0];
        // Saved preview instances are replaced on Awake so edit-mode assembly never duplicates parts in play.
        [SerializeField, HideInInspector] private GameObject[] assembledParts = new GameObject[4];

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.useGravity = false;
            body.constraints = RigidbodyConstraints.None;
            Vector3 forward = body.rotation * Vector3.forward;
            yaw = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
            pitch = Mathf.Clamp(-Mathf.Asin(Mathf.Clamp(forward.y, -1, 1)) * Mathf.Rad2Deg,
                -maxPitchAngle, maxPitchAngle);
            if (inventory == null) inventory = GetComponent<ShipInventory>();
            var savedResources = ResourceManager.Instance;
            if (savedResources != null && savedResources.CustomizationCatalog != null)
            {
                gunPart = savedResources.Equipped(ShipPartSlot.Gun) ?? gunPart;
                armorPlatePart = savedResources.Equipped(ShipPartSlot.ArmorPlate) ?? armorPlatePart;
                wingPart = savedResources.Equipped(ShipPartSlot.Wing) ?? wingPart;
                jetPart = savedResources.Equipped(ShipPartSlot.Jet) ?? jetPart;
            }
            AssembleParts();
            RebuildStats(true);
            barrier.Configure(armorPlatePart, transform);
        }

        private void Update()
        {
            flightInput = Vector2.zero;
            boostHeld = false;
            fireHeld = false;
            var keyboard = Keyboard.current;
            if (IsAlive && keyboard != null && Application.isFocused)
            {
                flightInput = new Vector2((keyboard.dKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed ? 1 : 0),
                    (keyboard.sKey.isPressed ? 1 : 0) - (keyboard.wKey.isPressed ? 1 : 0));
                boostHeld = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
                if (keyboard.spaceKey.wasPressedThisFrame) TryDodge(Mathf.RoundToInt(flightInput.x));
                if (keyboard.eKey.wasPressedThisFrame) TogglePartMode();
            }
            fireHeld = IsAlive && Application.isFocused && Mouse.current != null && Mouse.current.leftButton.isPressed;
            foreach (var gun in guns)
                if (gun != null) gun.SetTrigger(fireHeld, Stats.attack, transform);
        }

        private void FixedUpdate()
        {
            barrier.Tick(IsAlive, Time.fixedDeltaTime);
            if (IsAlive) CollectNearby();
            if (!boostHeld) boostExhausted = false;
            IsBoosting = IsAlive && boostHeld && !boostExhausted && Boost > 0;
            if (wasBoosting && !IsBoosting && jetPart != null)
                boostSlowUntil = Time.time + jetPart.postBoostSlowDuration;
            wasBoosting = IsBoosting;
            float multiplier = jetPart != null ? jetPart.boostSpeedMultiplier : 2f;
            float drain = jetPart != null ? jetPart.boostDrainPerSecond : 25f;
            float recovery = jetPart != null ? jetPart.boostRecoveryPerSecond : 15f;
            float delay = jetPart != null ? jetPart.boostRecoveryDelay : 1f;
            float boostedSeconds = IsBoosting ? Mathf.Min(Time.fixedDeltaTime, Boost / Mathf.Max(0.01f, drain)) : 0;
            if (IsBoosting)
            {
                Boost = Mathf.Max(0, Boost - drain * boostedSeconds);
                recoveryAt = Time.time + delay;
                if (Boost <= 0) boostExhausted = true;
            }
            else if (IsAlive && !boostHeld && Time.time >= recoveryAt)
                Boost = Mathf.Min(Stats.boosterCapacity, Boost + recovery * Time.fixedDeltaTime);

            if (IsAlive)
            {
                float step = Time.fixedDeltaTime;
                bool rollingThisStep = dodge.IsRolling;
                dodge.Tick(Time.timeAsDouble);
                float rollAngle = 0;
                Vector3 dodgeStep = Vector3.zero;
                if (rollingThisStep)
                {
                    rollAngle = -dodge.Direction * 360f * Mathf.SmoothStep(0, 1, dodge.Progress(Time.timeAsDouble));
                    float traveled = dodgeDistance * dodge.TravelProgress(Time.timeAsDouble, dodgeDashDuration);
                    dodgeStep = dodgeDirectionWorld * (traveled - dodgeDistanceMoved);
                    dodgeDistanceMoved = traveled;
                }
                else
                {
                    float responseFactor = wingPart != null ? (AlternateMode && wingPart.switchableMode ? wingPart.alternateSteeringResponseMultiplier : wingPart.steeringResponseMultiplier) : 1f;
                    float boostSteering = IsBoosting ? Mathf.Max(1, boostSteeringMultiplier) : 1f;
                    float responseTime = steeringResponseTime * responseFactor * (IsBoosting ? boostSteeringResponseMultiplier : 1f);
                    currentBank = Mathf.MoveTowards(currentBank, -flightInput.x * bankAngle,
                        bankDegreesPerSecond * step * boostSteering / Mathf.Max(0.01f, responseFactor));
                    float turn = bankAngle > 0 ? -currentBank / bankAngle : 0;
                    float handlingFactor = wingPart != null ? (AlternateMode && wingPart.switchableMode ? wingPart.alternateHandlingMultiplier : wingPart.handlingMultiplier) : 1f;
                    pitchRate = Mathf.SmoothDamp(pitchRate, -flightInput.y * Mathf.Min(Stats.handling, pitchDegreesPerSecond * Mathf.Min(1, handlingFactor)) * boostSteering,
                        ref pitchRateVelocity, responseTime, Mathf.Infinity, step);
                    turnRate = Mathf.SmoothDamp(turnRate, turn * Mathf.Min(Stats.handling, turnDegreesPerSecond * Mathf.Min(1, handlingFactor)) * boostSteering,
                        ref turnRateVelocity, responseTime, Mathf.Infinity, step);
                    pitch = Mathf.Clamp(pitch + pitchRate * step, -maxPitchAngle, maxPitchAngle);
                    // Discard outward angular momentum at the pitch stop so reversing responds smoothly.
                    if ((pitch <= -maxPitchAngle && pitchRate < 0) || (pitch >= maxPitchAngle && pitchRate > 0))
                    { pitchRate = 0; pitchRateVelocity = 0; }
                    yaw = Mathf.Repeat(yaw + turnRate * step, 360f);
                }
                // Roll is around the forward axis only: flight direction and No-Roll camera remain stable.
                Quaternion heading = Quaternion.Euler(pitch, yaw, currentBank + rollAngle);
                float speedTarget = Stats.speed * (1 + boostedSeconds / step * (Mathf.Max(1, multiplier) - 1));
                if (Time.time < boostSlowUntil && jetPart != null) speedTarget *= jetPart.postBoostSpeedMultiplier;
                flightSpeed = Mathf.SmoothDamp(flightSpeed, speedTarget, ref speedVelocity,
                    speedResponseTime, Mathf.Infinity, step);
                body.angularVelocity = Vector3.zero;
                if (wingPart != null && wingPart.knockbackMultiplier < 1f)
                    body.linearVelocity *= wingPart.knockbackMultiplier;
                body.MoveRotation(heading);
                body.MovePosition(body.position + (heading * Vector3.forward) * (flightSpeed * step) + dodgeStep);
            }
            UpdateBoostEffects(IsBoosting);
            boostChanged.Invoke(Boost, Stats.boosterCapacity);
        }

        public bool TryDodge(int direction)
        {
            if (dodgeRechargeAt > 0 && Time.timeAsDouble >= dodgeRechargeAt)
            { dodgeChargesUsed = 0; dodgeRechargeAt = 0; }
            int charges = jetPart != null ? Mathf.Max(1, jetPart.dodgeCharges) : 1;
            bool lastCharge = dodgeChargesUsed + 1 >= charges;
            if (!isActiveAndEnabled || !IsAlive || !dodge.TryBegin(direction, Time.timeAsDouble, dodgeDuration, lastCharge ? DodgeCooldown : 0))
                return false;
            dodgeChargesUsed++;
            if (lastCharge) dodgeRechargeAt = dodge.ReadyAt;
            pitchRate = turnRate = pitchRateVelocity = turnRateVelocity = 0;
            dodgeDirectionWorld = Quaternion.Euler(0, yaw, 0) * Vector3.right * dodge.Direction;
            dodgeDistanceMoved = 0;
            return true;
        }

        // Customization can call this before spawning or while docked. Invalid loadouts leave the current one intact.
        public bool SetLoadout(ShipPartStats gun, ShipPartStats armor, ShipPartStats wing, ShipPartStats jet)
        {
            if (!Fits(gun, ShipPartSlot.Gun) || !Fits(armor, ShipPartSlot.ArmorPlate) ||
                !Fits(wing, ShipPartSlot.Wing) || !Fits(jet, ShipPartSlot.Jet))
            {
                Debug.LogWarning("Loadout rejected: a part is assigned to the wrong slot.", this);
                return false;
            }
            gunPart = gun; armorPlatePart = armor; wingPart = wing; jetPart = jet;
            AlternateMode = false;
            dodgeChargesUsed = 0; dodgeRechargeAt = 0; dodge.Cancel();
            boostSlowUntil = 0; wasBoosting = false;
            if (Application.isPlaying && body != null) AssembleParts();
            RebuildStats(false);
            if (Application.isPlaying && body != null) barrier.Configure(armorPlatePart, transform);
            return true;
        }

        public void TogglePartMode()
        {
            if (!IsAlive || !((armorPlatePart != null && armorPlatePart.switchableMode) ||
                (wingPart != null && wingPart.switchableMode))) return;
            AlternateMode = !AlternateMode;
            RebuildStats(false);
        }

        private static bool Fits(ShipPartStats part, ShipPartSlot slot) => part == null || part.slot == slot;

        private void AssembleParts()
        {
            if (guns != null)
                foreach (var gun in guns) if (gun != null) gun.SetTrigger(false, 0, transform);
            for (int i = 0; i < assembledParts.Length; i++)
            {
                if (assembledParts[i] == null) continue;
                assembledParts[i].SetActive(false);
                Destroy(assembledParts[i]);
                assembledParts[i] = null;
            }
            AssemblePart(gunPart, ShipPartSlot.Gun, gunMount);
            AssemblePart(armorPlatePart, ShipPartSlot.ArmorPlate, armorPlateMount);
            AssemblePart(wingPart, ShipPartSlot.Wing, wingMount);
            AssemblePart(jetPart, ShipPartSlot.Jet, jetMount);
            guns = GetComponentsInChildren<ShipGun>(false);
            boostEffects = GetComponentsInChildren<ShipBoostEffect>(true);
            UpdateBoostEffects(IsBoosting && IsAlive);
        }

        private void UpdateBoostEffects(bool active)
        {
            UISfxManager.SetJetLoop(this, IsAlive && isActiveAndEnabled, active);
            foreach (var effect in boostEffects)
                if (effect != null) effect.SetFlightState(IsAlive && isActiveAndEnabled, active);
        }

        private void AssemblePart(ShipPartStats part, ShipPartSlot slot, Transform mount)
        {
            if (part == null || part.slot != slot || part.partPrefab == null) return;
            if (mount != null && mount != transform && !mount.IsChildOf(transform))
            { Debug.LogWarning($"Cannot assemble {slot}: mount must belong to this ship.", this); return; }
            var instance = Instantiate(part.partPrefab, mount != null ? mount : transform, false);
            instance.name = slot + "_" + part.partPrefab.name;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            assembledParts[(int)slot] = instance;
        }

        public void RebuildStats(bool refill = false)
        {
            var total = baseStats;
            AddPart(ref total, gunPart, ShipPartSlot.Gun);
            AddPart(ref total, armorPlatePart, ShipPartSlot.ArmorPlate);
            AddPart(ref total, wingPart, ShipPartSlot.Wing);
            AddPart(ref total, jetPart, ShipPartSlot.Jet);
            total = ShipPartTraits.Apply(total, armorPlatePart, wingPart, jetPart, AlternateMode);
            total.attack = Mathf.Max(0, total.attack); total.defense = Mathf.Max(0, total.defense);
            total.speed = Mathf.Max(0, total.speed); total.handling = Mathf.Max(0, total.handling);
            total.health = Mathf.Max(1, total.health); total.boosterCapacity = Mathf.Max(0, total.boosterCapacity);
            Stats = total;
            Health = refill ? total.health : Mathf.Min(Health, total.health);
            Boost = refill ? total.boosterCapacity : Mathf.Min(Boost, total.boosterCapacity);
            healthChanged.Invoke(Health, Stats.health);
            boostChanged.Invoke(Boost, Stats.boosterCapacity);
        }

        private void AddPart(ref ShipStats total, ShipPartStats part, ShipPartSlot slot)
        {
            if (part == null) return;
            if (part.slot != slot) { Debug.LogWarning($"Ignoring {part.name}: expected slot {slot}.", this); return; }
            total += part.stats;
        }

        public void TakeDamage(float amount)
            => TakeHit(amount, ShipHitSource.Unknown, transform.position, -transform.forward);

        public void TakeHit(float amount, ShipHitSource source, Vector3 point, Vector3 normal)
        {
            if (!IsAlive || IsInvulnerable || amount <= 0) return;
            float previousHealth = Health;
            Health = Mathf.Max(0, Health - barrier.Absorb(Mathf.Max(0, amount - Stats.defense)));
            ShipCombatFeedback.Report(source, true, previousHealth - Health, point, normal);
            healthChanged.Invoke(Health, Stats.health);
            if (IsAlive) return;
            dodge.Cancel();
            flightInput = Vector2.zero;
            boostHeld = false;
            IsBoosting = false;
            barrier.SetVisible(false);
            UpdateBoostEffects(false);
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            flightSpeed = 0;
            speedVelocity = 0;
            pitchRate = turnRate = pitchRateVelocity = turnRateVelocity = 0;
            foreach (var gun in guns) if (gun != null) gun.SetTrigger(false, Stats.attack, transform);
            died.Invoke();
        }

        public void CollectNearby()
        {
            var storage = Inventory;
            if (!IsAlive || storage == null) return;
            foreach (var hit in Physics.OverlapSphere(transform.position, pickupRadius, pickupLayers, QueryTriggerInteraction.Collide))
            {
                var pickup = hit.GetComponentInParent<ShipPickup>();
                if (pickup != null) pickup.Attract(this, storage);
            }
        }

        private void OnDisable()
        {
            barrier.SetVisible(false);
            bool wasRolling = dodge.IsRolling;
            dodge.Cancel();
            if (wasRolling && body != null) body.rotation = Quaternion.Euler(pitch, yaw, currentBank);
            flightInput = Vector2.zero; boostHeld = false; fireHeld = false; IsBoosting = false;
            UpdateBoostEffects(false);
            if (guns != null) foreach (var gun in guns) if (gun != null) gun.SetTrigger(false, 0, transform);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, pickupRadius);
        }

        private void OnDestroy() => barrier.Dispose();
    }
}
