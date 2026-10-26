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
                { item.amount += amount; changed.Invoke(); return true; }
            items.Add(new ItemStack { itemId = itemId, amount = amount });
            changed.Invoke(); return true;
        }
    }
}
