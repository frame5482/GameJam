using System;
using UnityEngine;

namespace SpaceGarage
{
    // Shared data contract for both the original UGUI HUD and the replaceable Toolkit HUD.
    [Serializable]
    public sealed class ShipMaterialRequirement
    {
        public ShipMaterialData material;
        [Min(1)] public int requiredAmount = 1;
    }
}
