using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceGarage
{
    public sealed class ShipHUD : MonoBehaviour
    {
        [SerializeField] private ShipController ship = null;
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
        [Header("Rows made by the user, in display order")]
        [SerializeField] private ShipTrackedMaterialRow[] materialRows = new ShipTrackedMaterialRow[0];
        private readonly List<ShipMaterialRequirement> tracked = new List<ShipMaterialRequirement>();
        private ShipInventory subscribedInventory;
        private int lastHealth = -1, lastMaxHealth = -1, lastBoost = -1, lastMaxBoost = -1, lastSpeed = -1;
        private int lastDodge = -1;

        public void SetShip(ShipController target)
        {
            Unsubscribe();
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
            if (tracked.Count > materialRows.Length)
                Debug.LogWarning($"HUD has {materialRows.Length} material rows for {tracked.Count} marked materials. Add and assign more rows to display the remaining entries.", this);
            RefreshMaterials();
        }

        private void OnEnable() { ResetCache(); BindInventory(); RefreshMaterials(); }
        private void OnDisable() => Unsubscribe();
        private void ResetCache() => lastHealth = lastMaxHealth = lastBoost = lastMaxBoost = lastSpeed = lastDodge = -1;

        private void LateUpdate()
        {
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
            if (isActiveAndEnabled && subscribedInventory != null) subscribedInventory.changed.AddListener(RefreshMaterials);
            RefreshMaterials();
        }

        private void Unsubscribe()
        {
            if (subscribedInventory != null) subscribedInventory.changed.RemoveListener(RefreshMaterials);
            subscribedInventory = null;
        }

        public void RefreshMaterials()
        {
            var inventory = ship != null ? ship.Inventory : null;
            for (int i = 0; i < materialRows.Length; i++)
            {
                if (materialRows[i] == null) continue;
                var entry = i < tracked.Count ? tracked[i] : null;
                var material = entry != null ? entry.material : null;
                int amount = inventory != null && material != null ? inventory.Count(material.itemId) : 0;
                materialRows[i].Display(material, amount, entry != null ? entry.requiredAmount : 0);
            }
        }
    }
}
