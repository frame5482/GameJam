using UnityEngine;

namespace SpaceGarage
{
    [RequireComponent(typeof(SphereCollider))]
    public sealed class ShipPickup : MonoBehaviour
    {
        [SerializeField] private string itemId = "Scrap";
        [SerializeField] private ShipMaterialData material = null;
        [SerializeField, Min(1)] private int amount = 1;
        private bool collected;
        public bool TryCollect(ShipInventory inventory)
        {
            if (collected || inventory == null) return false;
            // Lock before invoking inventory events to prevent reentrant or multi-collider collection.
            collected = true;
            if (material != null && ResourceManager.Instance != null) ResourceManager.Instance.RegisterMaterial(material);
            if (!inventory.Add(material != null ? material.itemId : itemId, amount)) { collected = false; return false; }
            gameObject.SetActive(false);
            Destroy(gameObject);
            return true;
        }
        private void Reset() => GetComponent<SphereCollider>().isTrigger = true;
    }
}
