using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace SpaceGarage
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ShipController : MonoBehaviour, IShipDamageable
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
        public ShipInventory Inventory => inventory;

        private Rigidbody body;
        private Vector3 movement;
        private bool boostHeld;
        private bool fireHeld;
        private bool boostExhausted;
        private float recoveryAt;
        // Saved preview instances are replaced on Awake so edit-mode assembly never duplicates parts in play.
        [SerializeField, HideInInspector] private GameObject[] assembledParts = new GameObject[4];

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.useGravity = false;
            body.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            if (inventory == null) inventory = GetComponent<ShipInventory>();
            AssembleParts();
            RebuildStats(true);
        }

        private void Update()
        {
            movement = Vector3.zero;
            boostHeld = false;
            fireHeld = false;
            var keyboard = Keyboard.current;
            if (IsAlive && keyboard != null && Application.isFocused)
            {
                movement = new Vector3((keyboard.dKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed ? 1 : 0),
                    0, (keyboard.sKey.isPressed ? 1 : 0) - (keyboard.wKey.isPressed ? 1 : 0)).normalized;
                boostHeld = keyboard.spaceKey.isPressed;
                if (keyboard.eKey.wasPressedThisFrame) CollectNearby();
            }
            fireHeld = IsAlive && Application.isFocused && Mouse.current != null && Mouse.current.leftButton.isPressed;
            foreach (var gun in guns)
                if (gun != null) gun.SetTrigger(fireHeld, Stats.attack, transform);
        }

        private void FixedUpdate()
        {
            if (!boostHeld) boostExhausted = false;
            IsBoosting = IsAlive && boostHeld && !boostExhausted && movement.sqrMagnitude > 0 && Boost > 0;
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

            float distance = Stats.speed * (Time.fixedDeltaTime + boostedSeconds * (Mathf.Max(1, multiplier) - 1));
            if (IsAlive)
            {
                body.MovePosition(body.position + movement * distance);
                if (movement.sqrMagnitude > 0)
                    body.MoveRotation(Quaternion.RotateTowards(body.rotation, Quaternion.LookRotation(movement, Vector3.up),
                        Stats.handling * Time.fixedDeltaTime));
            }
            boostChanged.Invoke(Boost, Stats.boosterCapacity);
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
            if (Application.isPlaying && body != null) AssembleParts();
            RebuildStats(false);
            return true;
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
        {
            if (!IsAlive || amount <= 0) return;
            Health = Mathf.Max(0, Health - Mathf.Max(0, amount - Stats.defense));
            healthChanged.Invoke(Health, Stats.health);
            if (IsAlive) return;
            movement = Vector3.zero;
            boostHeld = false;
            IsBoosting = false;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            foreach (var gun in guns) if (gun != null) gun.SetTrigger(false, Stats.attack, transform);
            died.Invoke();
        }

        public void CollectNearby()
        {
            if (!IsAlive || inventory == null) return;
            foreach (var hit in Physics.OverlapSphere(transform.position, pickupRadius, pickupLayers, QueryTriggerInteraction.Collide))
            {
                var pickup = hit.GetComponentInParent<ShipPickup>();
                if (pickup != null) pickup.TryCollect(inventory);
            }
        }

        private void OnDisable()
        {
            movement = Vector3.zero; boostHeld = false; fireHeld = false; IsBoosting = false;
            if (guns != null) foreach (var gun in guns) if (gun != null) gun.SetTrigger(false, 0, transform);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, pickupRadius);
        }
    }
}
