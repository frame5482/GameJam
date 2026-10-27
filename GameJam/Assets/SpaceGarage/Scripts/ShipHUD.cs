using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceGarage
{
    [AddComponentMenu("Space Garage/UI/Canvas Combat HUD")]
    [DisallowMultipleComponent]
    public sealed class ShipHUD : MonoBehaviour
    {
        [Tooltip("Canvas/UGUI HUD. Drag the player's ShipController here, or enable Auto Find Player.")]
        [SerializeField] private ShipController ship = null;
        [SerializeField] private bool autoFindPlayer = true;
        [Header("Existing health UI (all optional)")]
        [SerializeField] private TMP_Text healthText = null;
        [SerializeField] private Slider healthSlider = null;
        [SerializeField] private Image healthFill = null;
        [Header("Existing booster UI (all optional)")]
        [SerializeField] private TMP_Text boostText = null;
        [SerializeField] private Slider boostSlider = null;
        [SerializeField] private Image boostFill = null;
        [Header("Existing dodge cooldown UI (optional)")]
        [SerializeField] private TMP_Text dodgeCooldownText = null;
        [SerializeField] private Slider dodgeReadySlider = null;
        [SerializeField] private Image dodgeReadyFill = null;
        [Header("Existing speed UI")]
        [SerializeField] private TMP_Text speedText = null;
        [Header("Spawn marked material rows into your Canvas container")]
        [SerializeField] private RectTransform materialContainer = null;
        [SerializeField] private ShipTrackedMaterialRow materialRowPrefab = null;
        [Header("Successful pickups: reuse your repair material prefab")]
        [SerializeField] private RectTransform pickupContainer = null;
        [SerializeField] private RepairMaterialDragItem pickupRowPrefab = null;
        [SerializeField, Min(0.1f)] private float pickupNotificationDuration = 3f;
        [Header("Rows made by the user, in display order")]
        [SerializeField] private ShipTrackedMaterialRow[] materialRows = new ShipTrackedMaterialRow[0];
        private readonly List<ShipMaterialRequirement> tracked = new List<ShipMaterialRequirement>();
        private ShipInventory subscribedInventory;
        private int lastHealth = -1, lastMaxHealth = -1, lastBoost = -1, lastMaxBoost = -1, lastSpeed = -1;
        private int lastDodge = -1;
        private float nextPlayerSearch;
        private readonly List<ShipTrackedMaterialRow> spawnedMaterialRows = new List<ShipTrackedMaterialRow>();
        private RectTransform spawnedContainer;
        private ShipTrackedMaterialRow spawnedRowPrefab;
        private bool materialSetupWarning;
        private sealed class PickupNotification
        {
            public RepairMaterialDragItem row;
            public float expiresAt;
        }
        private readonly List<PickupNotification> pickupNotifications = new List<PickupNotification>();
        private bool pickupSetupWarning;

        public void SetShip(ShipController target)
        {
            Unsubscribe();
            ClearPickupNotifications();
            ship = target;
            ResetCache();
            BindInventory();
            RefreshMaterials();
        }

        // Called by the future marking/customization system. HUD does not choose materials itself.
        public void SetTrackedMaterials(IEnumerable<ShipMaterialRequirement> requirements)
        {
            tracked.Clear();
            if (requirements != null)
                foreach (var entry in requirements)
                    if (entry != null && entry.material != null && !string.IsNullOrWhiteSpace(entry.material.itemId))
                        tracked.Add(new ShipMaterialRequirement
                        { material = entry.material, requiredAmount = Mathf.Max(1, entry.requiredAmount) });
            if (materialContainer == null && tracked.Count > materialRows.Length)
                Debug.LogWarning($"HUD has {materialRows.Length} material rows for {tracked.Count} marked materials. Add and assign more rows to display the remaining entries.", this);
            RefreshMaterials();
        }

        private void OnEnable() { ResetCache(); BindInventory(); RefreshMaterials(); }
        private void OnDisable() { Unsubscribe(); ClearSpawnedMaterialRows(); ClearPickupNotifications(); }
        private void ResetCache() => lastHealth = lastMaxHealth = lastBoost = lastMaxBoost = lastSpeed = lastDodge = -1;

        private void LateUpdate()
        {
            for (int i = pickupNotifications.Count - 1; i >= 0; i--)
                if (pickupNotifications[i].row == null || Time.unscaledTime >= pickupNotifications[i].expiresAt)
                {
                    RemovePickupRow(pickupNotifications[i].row);
                    pickupNotifications.RemoveAt(i);
                }
            if (ship == null && autoFindPlayer && Time.unscaledTime >= nextPlayerSearch)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                ship = player != null ? player.GetComponent<ShipController>() : null;
                nextPlayerSearch = Time.unscaledTime + 0.5f;
                ResetCache();
            }
            BindInventory();
            float health = ship != null ? ship.Health : 0;
            float maxHealth = ship != null ? ship.Stats.health : 0;
            float boost = ship != null ? ship.Boost : 0;
            float maxBoost = ship != null ? ship.Stats.boosterCapacity : 0;
            int hp = Mathf.CeilToInt(health), hpMax = Mathf.CeilToInt(maxHealth);
            int fuel = Mathf.CeilToInt(boost), fuelMax = Mathf.CeilToInt(maxBoost);
            int speed = Mathf.RoundToInt((ship != null ? ship.CurrentSpeed : 0) * 10);
            if (healthText != null && (hp != lastHealth || hpMax != lastMaxHealth)) healthText.text = $"{hp}/{hpMax}";
            if (boostText != null && (fuel != lastBoost || fuelMax != lastMaxBoost)) boostText.text = $"{fuel}/{fuelMax}";
            if (speedText != null && speed != lastSpeed) speedText.text = (speed / 10f).ToString("0.0");
            lastHealth = hp; lastMaxHealth = hpMax; lastBoost = fuel; lastMaxBoost = fuelMax; lastSpeed = speed;
            SetGauge(healthSlider, healthFill, health, maxHealth);
            SetGauge(boostSlider, boostFill, boost, maxBoost);
            int dodgeTime = Mathf.CeilToInt((ship != null ? ship.DodgeCooldownRemaining : 0) * 10);
            if (dodgeCooldownText != null && dodgeTime != lastDodge)
                dodgeCooldownText.text = (dodgeTime / 10f).ToString("0.0");
            lastDodge = dodgeTime;
            SetGauge(dodgeReadySlider, dodgeReadyFill, ship != null ? ship.DodgeReadyProgress : 0, 1);
        }

        private static void SetGauge(Slider slider, Image fill, float current, float maximum)
        {
            float ratio = maximum > 0 ? Mathf.Clamp01(current / maximum) : 0;
            if (slider != null)
            {
                float value = Mathf.Lerp(slider.minValue, slider.maxValue, ratio);
                slider.SetValueWithoutNotify(value);
            }
            if (fill != null) fill.fillAmount = ratio;
        }

        private void BindInventory()
        {
            var next = isActiveAndEnabled && ship != null ? ship.Inventory : null;
            if (ReferenceEquals(next, subscribedInventory)) return;
            Unsubscribe();
            subscribedInventory = next;
            if (isActiveAndEnabled && subscribedInventory != null)
            {
                subscribedInventory.changed.AddListener(RefreshMaterials);
                subscribedInventory.pickupCollected += ShowPickupNotification;
            }
            RefreshMaterials();
        }

        private void Unsubscribe()
        {
            if (subscribedInventory != null)
            {
                subscribedInventory.changed.RemoveListener(RefreshMaterials);
                subscribedInventory.pickupCollected -= ShowPickupNotification;
            }
            subscribedInventory = null;
        }

        private void ShowPickupNotification(ShipMaterialData material, string itemId, int amount)
        {
            if (!isActiveAndEnabled || amount <= 0) return;
            if (pickupContainer == null || pickupRowPrefab == null ||
                pickupRowPrefab.transform == pickupContainer || pickupContainer.IsChildOf(pickupRowPrefab.transform) ||
                pickupRowPrefab.GetComponentInChildren<ShipHUD>(true) != null)
            {
                if (!pickupSetupWarning)
                {
                    Debug.LogWarning("Assign Pickup Container and a separate repair material prefab on ShipHUD to show collected items.", this);
                    pickupSetupWarning = true;
                }
                return;
            }
            pickupSetupWarning = false;
            if (material == null && ResourceManager.Instance != null)
                foreach (var data in ResourceManager.Instance.MaterialCatalog)
                    if (data != null && data.itemId == itemId) { material = data; break; }
            var row = Instantiate(pickupRowPrefab, pickupContainer, false);
            row.name = "Pickup_" + itemId;
            row.DisplayPickup(material, itemId, amount);
            row.gameObject.SetActive(true);
            pickupNotifications.Add(new PickupNotification
            { row = row, expiresAt = Time.unscaledTime + Mathf.Max(0.1f, pickupNotificationDuration) });
        }

        private static void RemovePickupRow(RepairMaterialDragItem row)
        {
            if (row == null) return;
            row.gameObject.SetActive(false);
            if (Application.isPlaying) Destroy(row.gameObject);
            else DestroyImmediate(row.gameObject);
        }

        private void ClearPickupNotifications()
        {
            foreach (var notification in pickupNotifications) RemovePickupRow(notification.row);
            pickupNotifications.Clear();
        }

        public void RefreshMaterials()
        {
            var inventory = ship != null ? ship.Inventory : null;
            if (materialContainer != null)
            {
                PrepareSpawnedMaterialRows();
                for (int i = 0; i < spawnedMaterialRows.Count; i++)
                {
                    var row = spawnedMaterialRows[i];
                    if (row == null) continue;
                    bool active = i < tracked.Count;
                    row.gameObject.SetActive(active);
                    if (active) DisplayMaterial(row, tracked[i], inventory);
                }
                return;
            }
            if (spawnedMaterialRows.Count > 0) ClearSpawnedMaterialRows();
            for (int i = 0; i < materialRows.Length; i++)
            {
                var entry = i < tracked.Count ? tracked[i] : null;
                if (materialRows[i] != null) DisplayMaterial(materialRows[i], entry, inventory);
            }
        }

        private static void DisplayMaterial(ShipTrackedMaterialRow row, ShipMaterialRequirement entry, ShipInventory inventory)
        {
            var material = entry != null ? entry.material : null;
            int amount = inventory != null && material != null ? inventory.Count(material.itemId) : 0;
            row.Display(material, amount, entry != null ? entry.requiredAmount : 0);
        }

        private void PrepareSpawnedMaterialRows()
        {
            if (spawnedContainer != materialContainer || spawnedRowPrefab != materialRowPrefab)
            {
                ClearSpawnedMaterialRows();
                spawnedContainer = materialContainer;
                spawnedRowPrefab = materialRowPrefab;
                materialSetupWarning = false;
            }
            if (materialRowPrefab == null || materialRowPrefab.transform == materialContainer ||
                materialContainer.IsChildOf(materialRowPrefab.transform) || materialRowPrefab.GetComponentInChildren<ShipHUD>(true) != null)
            {
                if (!materialSetupWarning && tracked.Count > 0)
                {
                    Debug.LogWarning("Material Container needs a separate Material Row Prefab with ShipTrackedMaterialRow. Do not use the container or the entire HUD as the row prefab.", this);
                    materialSetupWarning = true;
                }
                return;
            }
            if (!isActiveAndEnabled) return;
            for (int i = 0; i < tracked.Count; i++)
            {
                if (i < spawnedMaterialRows.Count && spawnedMaterialRows[i] != null) continue;
                var row = Instantiate(materialRowPrefab, materialContainer, false);
                row.name = "MarkedMaterial_" + i;
                if (i < spawnedMaterialRows.Count) spawnedMaterialRows[i] = row;
                else spawnedMaterialRows.Add(row);
            }
        }

        private void ClearSpawnedMaterialRows()
        {
            foreach (var row in spawnedMaterialRows)
                if (row != null)
                {
                    row.gameObject.SetActive(false);
                    if (Application.isPlaying) Destroy(row.gameObject);
                    else DestroyImmediate(row.gameObject);
                }
            spawnedMaterialRows.Clear();
            spawnedContainer = null;
            spawnedRowPrefab = null;
        }
    }
}
