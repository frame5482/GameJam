using UnityEngine;

namespace SpaceGarage
{
    [CreateAssetMenu(menuName = "Space Garage/Material Data")]
    public sealed class ShipMaterialData : ScriptableObject
    {
        [Tooltip("Stable inventory key. Must be unique per material.")]
        public string itemId = "Scrap";
        public string displayName = "Scrap";
        public Sprite icon;
    }
}
