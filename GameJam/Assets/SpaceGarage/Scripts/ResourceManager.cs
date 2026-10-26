using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace SpaceGarage
{
    [Serializable]
    public sealed class StartingMaterial
    {
        public ShipMaterialData material;
        [Min(0)] public int amount;
    }

    [DefaultExecutionOrder(-500)]
    [RequireComponent(typeof(ShipInventory))]
    [DisallowMultipleComponent]
    public sealed class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }
        [SerializeField, Min(0)] private long money;
        [SerializeField] private bool persistAcrossScenes = true;
        [SerializeField] private List<ShipMaterialData> materialCatalog = new List<ShipMaterialData>();
        [SerializeField] private List<StartingMaterial> startingMaterials = new List<StartingMaterial>();
        public UnityEvent changed = new UnityEvent();
        public ShipInventory Inventory { get; private set; }
        public long Money => money;
        public IReadOnlyList<ShipMaterialData> MaterialCatalog => materialCatalog;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Inventory = GetComponent<ShipInventory>();
            Inventory.changed.AddListener(NotifyChanged);
            foreach (var stack in startingMaterials)
                if (stack != null && stack.material != null && stack.amount > 0)
                { RegisterMaterial(stack.material); Inventory.Add(stack.material.itemId, stack.amount); }
            if (persistAcrossScenes) DontDestroyOnLoad(gameObject);
        }

        public bool AddMoney(long amount)
        {
            if (amount <= 0 || money > long.MaxValue - amount) return false;
            money += amount; changed.Invoke(); return true;
        }
        public bool TrySpendMoney(long amount)
        {
            if (amount < 0 || amount > money) return false;
            money -= amount; changed.Invoke(); return true;
        }
        public bool RegisterMaterial(ShipMaterialData material)
        {
            if (material == null || string.IsNullOrWhiteSpace(material.itemId)) return false;
            foreach (var existing in materialCatalog)
                if (existing != null && existing.itemId == material.itemId) return existing == material;
            materialCatalog.Add(material); changed.Invoke(); return true;
        }
        private void NotifyChanged() => changed.Invoke();
        private void OnDestroy()
        {
            if (Inventory != null) Inventory.changed.RemoveListener(NotifyChanged);
            if (Instance == this) Instance = null;
        }
    }
}
