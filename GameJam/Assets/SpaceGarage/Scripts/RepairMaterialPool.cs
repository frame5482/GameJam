using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceGarage
{
    [Serializable]
    public sealed class RepairMaterialOption
    {
        public ShipMaterialData material;
        [Min(1)] public int unitsAtFullDamage = 10;
        [Min(0)] public int paymentPerUnit = 5;
        [Tooltip("Empty: any part in this slot. Otherwise this material is used only by the listed parts.")]
        public List<ShipPartStats> allowedParts = new List<ShipPartStats>();
    }

    [CreateAssetMenu(menuName = "Space Garage/Repair Material Pool")]
    public sealed class RepairMaterialPool : ScriptableObject
    {
        public ShipPartSlot slot;
        [Min(1)] public int minimumMaterialTypes = 1;
        [Min(1)] public int maximumMaterialTypes = 2;
        public List<RepairMaterialOption> materials = new List<RepairMaterialOption>();

        public List<RepairMaterialOption> ValidOptions(ShipPartStats part = null)
        {
            var result = new List<RepairMaterialOption>();
            var ids = new HashSet<string>();
            foreach (var option in materials)
                if (option != null && option.material != null && !string.IsNullOrWhiteSpace(option.material.itemId) &&
                    option.unitsAtFullDamage > 0 &&
                    (part == null || option.allowedParts == null || option.allowedParts.Count == 0 || option.allowedParts.Contains(part)) &&
                    ids.Add(option.material.itemId)) result.Add(option);
            return result;
        }
    }
}
