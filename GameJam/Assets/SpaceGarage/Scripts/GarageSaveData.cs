using System;
using System.Collections.Generic;

namespace SpaceGarage
{
    [Serializable]
    public sealed class GarageSaveData
    {
        public int version;
        public long money;
        public List<ShipInventory.ItemStack> materials;
        public List<string> unlockedParts;
        public string[] equippedParts;

        public bool IsValid()
        {
            if (version != 1 || money < 0 || materials == null || unlockedParts == null || equippedParts == null || equippedParts.Length != 4) return false;
            var ids = new HashSet<string>();
            foreach (var stack in materials)
                if (stack == null || string.IsNullOrWhiteSpace(stack.itemId) || stack.amount <= 0 || !ids.Add(stack.itemId)) return false;
            ids.Clear();
            foreach (var id in unlockedParts) if (string.IsNullOrWhiteSpace(id) || !ids.Add(id)) return false;
            return true;
        }
    }
}
