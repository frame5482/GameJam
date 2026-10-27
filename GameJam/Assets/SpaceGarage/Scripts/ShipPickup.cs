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
        [SerializeField, Min(0.1f)] private float attractionSpeed = 25f;
        private ShipController collector;
        private ShipInventory destination;
        public void Attract(ShipController ship, ShipInventory inventory)
        {
            if (collected || ship == null || inventory == null) return;
            if (collector != null && collector != ship && collector.IsAlive) return;
            collector = ship; destination = inventory;
        }
        private void Update()
        {
            if (collected || collector == null || !collector.isActiveAndEnabled || !collector.IsAlive) return;
            transform.position = Vector3.MoveTowards(transform.position, collector.transform.position,
                (attractionSpeed + collector.CurrentSpeed) * Time.deltaTime);
            if ((transform.position - collector.transform.position).sqrMagnitude <= 0.25f)
                TryCollect(destination);
        }
        public bool TryCollect(ShipInventory inventory)
        {
            if (collected || inventory == null) return false;
            // Lock before invoking inventory events to prevent reentrant or multi-collider collection.
            collected = true;
            if (material != null && ResourceManager.Instance != null) ResourceManager.Instance.RegisterMaterial(material);
            if (!inventory.Add(material != null ? material.itemId : itemId, amount)) { collected = false; return false; }
            inventory.NotifyPickupCollected(material, material != null ? material.itemId : itemId, amount);
            gameObject.SetActive(false);
            Destroy(gameObject);
            return true;
        }
        private void Reset() => GetComponent<SphereCollider>().isTrigger = true;
    }
}
