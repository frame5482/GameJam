using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SpaceGarage
{
    public sealed class ShipInventory : MonoBehaviour
    {
        [Serializable]
        public sealed class ItemStack
        {
            public string itemId;
            [Min(1)] public int amount = 1;
        }
        [SerializeField] private List<ItemStack> items = new List<ItemStack>();
        public UnityEvent changed = new UnityEvent();
        // Only successful world pickups raise this; loading, purchases and repair spending do not.
        public event Action<ShipMaterialData, string, int> pickupCollected;
        internal void NotifyPickupCollected(ShipMaterialData material, string itemId, int amount)
            => pickupCollected?.Invoke(material, itemId, amount);
        public IReadOnlyList<ItemStack> Items => items;
        public void Restore(IReadOnlyList<ItemStack> saved)
        {
            items.Clear();
            foreach (var item in saved) items.Add(new ItemStack { itemId = item.itemId, amount = item.amount });
            changed.Invoke();
        }
        public int Count(string itemId)
        {
            int count = 0;
            foreach (var item in items) if (item != null && item.itemId == itemId) count += item.amount;
            return count;
        }
        public bool Add(string itemId, int amount)
        {
            if (string.IsNullOrWhiteSpace(itemId) || amount <= 0) return false;
            foreach (var item in items)
                if (item != null && item.itemId == itemId)
                { if (item.amount > int.MaxValue - amount) return false; item.amount += amount; changed.Invoke(); return true; }
            items.Add(new ItemStack { itemId = itemId, amount = amount });
            changed.Invoke(); return true;
        }

        public bool TryRemove(string itemId, int amount)
        {
            if (string.IsNullOrWhiteSpace(itemId) || amount <= 0 || Count(itemId) < amount) return false;
            int remaining = amount;
            for (int i = items.Count - 1; i >= 0 && remaining > 0; i--)
            {
                var item = items[i];
                if (item == null || item.itemId != itemId) continue;
                int take = Math.Min(remaining, item.amount);
                item.amount -= take;
                remaining -= take;
                if (item.amount <= 0) items.RemoveAt(i);
            }
            changed.Invoke();
            return true;
        }
    }
}
