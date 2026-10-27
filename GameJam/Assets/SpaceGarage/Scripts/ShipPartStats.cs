using System;
using UnityEngine;

namespace SpaceGarage
{
    public enum ShipPartSlot { Gun, ArmorPlate, Wing, Jet }

    [Serializable]
    public struct ShipStats
    {
        [Min(0)] public float attack;
        [Min(0)] public float defense;
        [Min(0)] public float speed;
        [Min(0)] public float handling;
        [Min(0)] public float health;
        [Min(0)] public float boosterCapacity;

        public static ShipStats operator +(ShipStats a, ShipStats b)
        {
            return new ShipStats
            {
                attack = a.attack + b.attack,
                defense = a.defense + b.defense,
                speed = a.speed + b.speed,
                handling = a.handling + b.handling,
                health = a.health + b.health,
                boosterCapacity = a.boosterCapacity + b.boosterCapacity
            };
        }
    }

    [CreateAssetMenu(menuName = "Space Garage/Ship Part Stats")]
    public sealed class ShipPartStats : ScriptableObject
    {
        [Tooltip("Stable unique ID used by JSON saves. Do not change after shipping.")]
        public string partId;
        public string displayName;
        public Sprite icon;
        public ShipPartSlot slot;
        [Tooltip("Part instantiated at its matching ship mount. Gun prefabs should contain ShipGun.")]
        public GameObject partPrefab;
        public ShipStats stats;
        [Header("Jet settings (used only in the Jet slot)")]
        [Min(1)] public float boostSpeedMultiplier = 2f;
        [Min(0.01f)] public float boostDrainPerSecond = 25f;
        [Min(0)] public float boostRecoveryPerSecond = 15f;
        [Min(0)] public float boostRecoveryDelay = 1f;

        [Header("Part traits (multipliers apply to the combined ship)")]
        [Min(0.01f)] public float speedMultiplier = 1f;
        [Min(0.01f)] public float healthMultiplier = 1f;
        [Min(0.01f)] public float handlingMultiplier = 1f;
        [Min(0.01f)] public float steeringResponseMultiplier = 1f;
        [Range(0, 1)] public float knockbackMultiplier = 1f;
        [Header("Unity mode: starts in speed mode; E toggles both equipped Unity parts")]
        public bool switchableMode;
        [Min(0.01f)] public float alternateSpeedMultiplier = 1f;
        [Min(0.01f)] public float alternateHandlingMultiplier = 1f;
        [Min(0.01f)] public float alternateSteeringResponseMultiplier = 1f;
        [Min(0)] public float alternateDefense;
        [Header("Automatic barrier (Armor Plate only)")]
        [Min(0)] public float barrierCapacity;
        [Min(0)] public float barrierRecoveryDelay = 4f;
        [Min(0)] public float barrierRecoveryPerSecond = 15f;
        public GameObject barrierPrefab;
        [Min(0.01f)] public float barrierVisualScale = 1f;
        [Header("Jet special settings")]
        [Min(1)] public int dodgeCharges = 1;
        [Range(0.01f, 1)] public float postBoostSpeedMultiplier = 1f;
        [Min(0)] public float postBoostSlowDuration;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(partId)) partId = Guid.NewGuid().ToString("N");
        }
    }
}
