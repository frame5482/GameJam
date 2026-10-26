using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceGarage
{
    [Serializable]
    public sealed class PricedShipPart
    {
        public ShipPartStats part;
        [Min(0)] public int price = 100;
    }
    [Serializable]
    public sealed class CustomizationPartPool
    {
        public ShipPartSlot slot;
        public List<ShipPartStats> initiallyUnlocked = new List<ShipPartStats>();
        public List<PricedShipPart> locked = new List<PricedShipPart>();
    }
    [CreateAssetMenu(menuName = "Space Garage/Customization Catalog")]
    public sealed class ShipCustomizationCatalog : ScriptableObject
    {
        public CustomizationPartPool[] pools = {
            new CustomizationPartPool { slot = ShipPartSlot.Gun },
            new CustomizationPartPool { slot = ShipPartSlot.ArmorPlate },
            new CustomizationPartPool { slot = ShipPartSlot.Wing },
            new CustomizationPartPool { slot = ShipPartSlot.Jet }
        };
        public IEnumerable<ShipPartStats> Parts
        {
            get
            {
                if (pools == null) yield break;
                foreach (var pool in pools)
                {
                    if (pool == null) continue;
                    if (pool.initiallyUnlocked != null)
                        foreach (var part in pool.initiallyUnlocked) if (Valid(part, pool.slot)) yield return part;
                    if (pool.locked != null)
                        foreach (var entry in pool.locked) if (entry != null && Valid(entry.part, pool.slot)) yield return entry.part;
                }
            }
        }
        public static bool Valid(ShipPartStats part, ShipPartSlot slot) =>
            part != null && !string.IsNullOrWhiteSpace(part.partId) && Enemy.Valid(part, slot);
        public ShipPartStats Find(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            ShipPartStats result = null;
            foreach (var part in Parts)
                if (part.partId == id)
                {
                    if (result != null && result != part) return null;
                    result = part;
                }
            return result;
        }
        private void OnValidate()
        {
            var ids = new Dictionary<string, ShipPartStats>();
            foreach (var part in Parts)
            {
                if (ids.TryGetValue(part.partId, out var other) && other != part)
                    Debug.LogWarning("Duplicate Part ID in customization catalog: " + part.partId + ". Assign a unique Part ID to " + part.name, this);
                else ids[part.partId] = part;
            }
        }
        public bool IsInitiallyUnlocked(string id)
        {
            if (pools == null) return false;
            foreach (var pool in pools)
                if (pool != null && pool.initiallyUnlocked != null)
                    foreach (var part in pool.initiallyUnlocked)
                        if (Valid(part, pool.slot) && part.partId == id) return true;
            return false;
        }
        public bool TryGetPrice(string id, out int price)
        {
            price = 0;
            if (pools == null) return false;
            foreach (var pool in pools)
                if (pool != null && pool.locked != null)
                    foreach (var entry in pool.locked)
                        if (entry != null && Valid(entry.part, pool.slot) && entry.part.partId == id)
                        { price = Mathf.Max(0, entry.price); return true; }
            return false;
        }
    }
}
