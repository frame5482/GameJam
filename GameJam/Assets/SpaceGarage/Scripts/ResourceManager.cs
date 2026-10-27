using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using System.IO;

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
        [SerializeField] private ShipCustomizationCatalog customizationCatalog = null;
        [SerializeField] private bool enableJsonSave = true;
        [SerializeField] private string saveFileName = "space-garage.json";
        private readonly HashSet<string> unlockedParts = new HashSet<string>();
        private string[] equippedParts = new string[4];
        private bool ready, dirty, protectUnreadableSave;
        public bool HasStartedGame { get; private set; }
        private float saveAt;
        public ShipCustomizationCatalog CustomizationCatalog => customizationCatalog;
        public string SavePath => Path.Combine(Application.persistentDataPath,
            string.IsNullOrWhiteSpace(saveFileName) ? "space-garage.json" : Path.GetFileName(saveFileName));
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
            bool loaded = enableJsonSave && LoadProgress();
            HasStartedGame = loaded;
            if (!loaded) foreach (var stack in startingMaterials)
                if (stack != null && stack.material != null && stack.amount > 0)
                { RegisterMaterial(stack.material); Inventory.Add(stack.material.itemId, stack.amount); }
            EnsureDefaultLoadout();
            ready = true;
            dirty = !protectUnreadableSave;
            saveAt = Time.unscaledTime + 0.5f;
            if (persistAcrossScenes) DontDestroyOnLoad(gameObject);
        }

        public bool AddMoney(long amount)
        {
            if (amount <= 0 || money > long.MaxValue - amount) return false;
            money += amount; NotifyChanged(); return true;
        }
        public bool TrySpendMoney(long amount)
        {
            if (amount < 0 || amount > money) return false;
            money -= amount; NotifyChanged(); return true;
        }
        public bool RegisterMaterial(ShipMaterialData material)
        {
            if (material == null || string.IsNullOrWhiteSpace(material.itemId)) return false;
            foreach (var existing in materialCatalog)
                if (existing != null && existing.itemId == material.itemId) return existing == material;
            materialCatalog.Add(material); NotifyChanged(); return true;
        }
        public bool IsUnlocked(ShipPartStats part) => part != null && customizationCatalog != null &&
            customizationCatalog.Find(part.partId) == part &&
            (customizationCatalog.IsInitiallyUnlocked(part.partId) || unlockedParts.Contains(part.partId));

        public ShipPartStats Equipped(ShipPartSlot slot) => customizationCatalog != null && (int)slot >= 0 && (int)slot < 4
            ? customizationCatalog.Find(equippedParts[(int)slot]) : null;

        public bool TryUnlock(ShipPartStats part)
        {
            if (part == null || customizationCatalog == null || customizationCatalog.Find(part.partId) != part || IsUnlocked(part) ||
                !customizationCatalog.TryGetPrice(part.partId, out int price) || money < price) return false;
            money -= price; unlockedParts.Add(part.partId); NotifyChanged(); return true;
        }
        public bool TryEquip(ShipPartStats part)
        {
            if (!IsUnlocked(part) || (int)part.slot < 0 || (int)part.slot >= 4) return false;
            equippedParts[(int)part.slot] = part.partId; NotifyChanged(); return true;
        }
        private void EnsureDefaultLoadout()
        {
            if (customizationCatalog == null) return;
            for (int i = 0; i < 4; i++)
            {
                var current = Equipped((ShipPartSlot)i);
                if (current != null && current.slot == (ShipPartSlot)i && IsUnlocked(current)) continue;
                equippedParts[i] = null;
                foreach (var part in customizationCatalog.Parts)
                    if (part.slot == (ShipPartSlot)i && IsUnlocked(part)) { equippedParts[i] = part.partId; break; }
            }
        }
        private void NotifyChanged()
        {
            if (ready) { dirty = true; saveAt = Time.unscaledTime + 0.5f; }
            changed.Invoke();
        }
        private void Update()
        {
            if (HasStartedGame && dirty && enableJsonSave && !protectUnreadableSave && Time.unscaledTime >= saveAt) SaveProgress();
        }
        private GarageSaveData Snapshot()
        {
            var data = new GarageSaveData { version = 1, money = money, equippedParts = (string[])equippedParts.Clone(),
                materials = new List<ShipInventory.ItemStack>(), unlockedParts = new List<string>() };
            foreach (var stack in Inventory.Items)
                if (stack != null && stack.amount > 0) data.materials.Add(new ShipInventory.ItemStack { itemId = stack.itemId, amount = stack.amount });
            data.unlockedParts.AddRange(unlockedParts); data.unlockedParts.Sort(StringComparer.Ordinal);
            return data;
        }
        public bool BeginGame() => HasStartedGame || ResetForNewGame();

        public bool ResetForNewGame()
        {
            if (!ready || Inventory == null || protectUnreadableSave)
            {
                Debug.LogWarning("Cannot start a new game: resources are not ready or the save is protected because it could not be read.", this);
                return false;
            }
            var data = new GarageSaveData
            {
                version = 1, money = 3000,
                materials = new List<ShipInventory.ItemStack>(),
                unlockedParts = new List<string>(), equippedParts = new string[4]
            };
            foreach (var stack in startingMaterials)
            {
                if (stack == null || stack.material == null || stack.amount <= 0 ||
                    string.IsNullOrWhiteSpace(stack.material.itemId)) continue;
                var existing = data.materials.Find(item => item.itemId == stack.material.itemId);
                if (existing == null)
                    data.materials.Add(new ShipInventory.ItemStack { itemId = stack.material.itemId, amount = stack.amount });
                else if (existing.amount <= int.MaxValue - stack.amount) existing.amount += stack.amount;
                else
                {
                    Debug.LogWarning("Cannot start a new game: Starting Materials exceed the inventory limit.", this);
                    return false;
                }
            }
            if (customizationCatalog != null)
                foreach (var part in customizationCatalog.Parts)
                    if (data.equippedParts[(int)part.slot] == null &&
                        customizationCatalog.Find(part.partId) == part && customizationCatalog.IsInitiallyUnlocked(part.partId))
                        data.equippedParts[(int)part.slot] = part.partId;
            if (!data.IsValid()) return false;
            if (enableJsonSave)
            {
                try
                {
                    GarageJsonStorage.Write(SavePath, JsonUtility.ToJson(data, true));
                }
                catch (Exception error)
                {
                    Debug.LogWarning("Cannot save new game: " + error.Message, this);
                    return false;
                }
                // Recovery must restore the new game, rather than progress from before the reset.
                try { File.Copy(SavePath, SavePath + ".bak", true); }
                catch (Exception error) { Debug.LogWarning("Unable to update new game backup: " + error.Message, this); }
            }
            money = data.money;
            HasStartedGame = true;
            unlockedParts.Clear();
            equippedParts = data.equippedParts;
            foreach (var stack in startingMaterials)
                if (stack != null && stack.material != null && stack.amount > 0) RegisterMaterial(stack.material);
            Inventory.Restore(data.materials);
            dirty = false;
            return true;
        }
        [ContextMenu("Save Progress")]
        public bool SaveProgress()
        {
            if (!ready || !HasStartedGame || !enableJsonSave || protectUnreadableSave || Inventory == null) return false;
            try
            {
                string path = SavePath;
                var data = Snapshot();
                if (!data.IsValid()) throw new InvalidDataException("Cannot save invalid resource data");
                GarageJsonStorage.Write(path, JsonUtility.ToJson(data, true));
                dirty = false; return true;
            }
            catch (Exception error)
            {
                dirty = false;
                Debug.LogWarning("Unable to save progress: " + error.Message, this); return false;
            }
        }
        private bool LoadProgress()
        {
            if (!GarageJsonStorage.TryRead(SavePath, JsonUtility.FromJson<GarageSaveData>,
                message => Debug.LogWarning(message, this), out var data, out protectUnreadableSave)) return false;
            money = data.money; equippedParts = data.equippedParts;
            unlockedParts.Clear(); foreach (var id in data.unlockedParts) unlockedParts.Add(id);
            Inventory.Restore(data.materials); return true;
        }
        private void OnApplicationPause(bool paused) { if (paused && dirty) SaveProgress(); }
        private void OnApplicationQuit() { if (dirty) SaveProgress(); }
        private void OnDestroy()
        {
            if (Instance == this && dirty) SaveProgress();
            if (Inventory != null) Inventory.changed.RemoveListener(NotifyChanged);
            if (Instance == this) Instance = null;
        }
    }
}
