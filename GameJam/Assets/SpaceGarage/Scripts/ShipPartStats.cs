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

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(partId)) partId = Guid.NewGuid().ToString("N");
        }
    }
}
