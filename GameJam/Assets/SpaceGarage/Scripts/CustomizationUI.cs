using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceGarage
{
    public sealed class CustomizationUI : MonoBehaviour
    {
        [SerializeField] private ShipCustomization customization = null;
        [SerializeField] private GameObject customizationPanel = null;
        [SerializeField] private Button openButton = null;
        [SerializeField] private Button backButton = null;
        [SerializeField] private Button gunCategoryButton = null;
        [SerializeField] private Button armorCategoryButton = null;
        [SerializeField] private Button wingCategoryButton = null;
        [SerializeField] private Button jetCategoryButton = null;
        [SerializeField] private Button equipButton = null;
        [SerializeField] private Button unlockButton = null;
        [SerializeField] private TMP_Text moneyText = null;
        [SerializeField] private TMP_Text selectedNameText = null;
        [SerializeField] private TMP_Text unlockPriceText = null;
        [SerializeField] private RectTransform partContainer = null;
        [SerializeField] private CustomizationPartRow partRowPrefab = null;
        private ResourceManager resources;
        private readonly List<CustomizationPartRow> rows = new List<CustomizationPartRow>();
        private void OnEnable()
        {
            resources = ResourceManager.Instance;
            if (customization != null) customization.Changed += Refresh;
            if (resources != null) resources.changed.AddListener(Refresh);
            Bind(true); Refresh();
        }
        private void Bind(bool add)
        {
            Hook(openButton, Open, add); Hook(backButton, Back, add);
            Hook(gunCategoryButton, Gun, add); Hook(armorCategoryButton, Armor, add);
            Hook(wingCategoryButton, Wing, add); Hook(jetCategoryButton, Jet, add);
            Hook(equipButton, Equip, add); Hook(unlockButton, Unlock, add);
        }
        private static void Hook(Button button, UnityEngine.Events.UnityAction action, bool add)
        { if (button == null) return; if (add) button.onClick.AddListener(action); else button.onClick.RemoveListener(action); }
        private void Open() { if (customization != null) customization.OpenCustomization(); }
        private void Back() { if (customization != null) customization.CloseCustomization(); }
        private void Gun() => customization.SelectCategory(0);
        private void Armor() => customization.SelectCategory(1);
        private void Wing() => customization.SelectCategory(2);
        private void Jet() => customization.SelectCategory(3);
        private void Equip() => customization.EquipSelected();
        private void Unlock() => customization.UnlockSelected();
        private void Refresh()
        {
            bool open = customization != null && customization.IsOpen;
            if (customizationPanel != null && customizationPanel != gameObject) customizationPanel.SetActive(open);
            if (openButton != null) openButton.interactable = !open;
            if (moneyText != null) moneyText.text = resources != null ? resources.Money.ToString("N0") : "0";
            if (!open || resources == null || resources.CustomizationCatalog == null) return;
            var selected = customization.SelectedPart;
            bool unlocked = resources.IsUnlocked(selected);
            bool priced = resources.CustomizationCatalog.TryGetPrice(selected != null ? selected.partId : null, out int price);
            if (equipButton != null) equipButton.interactable = unlocked && resources.Equipped(selected.slot) != selected;
            if (unlockButton != null) unlockButton.interactable = selected != null && !unlocked && priced && resources.Money >= price;
            if (selectedNameText != null) selectedNameText.text = selected != null ? (string.IsNullOrWhiteSpace(selected.displayName) ? selected.name : selected.displayName) : string.Empty;
            if (unlockPriceText != null) unlockPriceText.text = selected != null && !unlocked && priced ? price.ToString("N0") : string.Empty;
            if (partContainer == null || partRowPrefab == null) return;
            var seen = new HashSet<string>();
            var available = new List<ShipPartStats>(); var locked = new List<ShipPartStats>();
            foreach (var part in resources.CustomizationCatalog.Parts)
                if (part.slot == customization.SelectedSlot && seen.Add(part.partId))
                    (resources.IsUnlocked(part) ? available : locked).Add(part);
            available.AddRange(locked);
            for (int i = 0; i < available.Count; i++)
            {
                if (rows.Count <= i) rows.Add(Instantiate(partRowPrefab, partContainer, false));
                rows[i].Bind(available[i], customization, resources); rows[i].gameObject.SetActive(true);
                rows[i].transform.SetAsLastSibling();
            }
            for (int i = available.Count; i < rows.Count; i++) rows[i].gameObject.SetActive(false);
        }
        private void OnDisable()
        {
            Bind(false);
            if (customization != null) customization.Changed -= Refresh;
            if (resources != null) resources.changed.RemoveListener(Refresh);
        }
        private void OnDestroy() { foreach (var row in rows) if (row != null) Destroy(row.gameObject); }
    }
}
