using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceGarage
{
    public sealed class CustomizationPartRow : MonoBehaviour
    {
        [SerializeField] private Button selectButton = null;
        [SerializeField] private Image icon = null;
        [SerializeField] private GameObject lockIcon = null;
        [SerializeField] private GameObject selectedIndicator = null;
        [SerializeField] private GameObject equippedIndicator = null;
        [SerializeField] private TMP_Text nameText = null;
        [SerializeField] private TMP_Text priceText = null;
        private ShipPartStats part;
        private ShipCustomization customization;
        public void Bind(ShipPartStats data, ShipCustomization owner, ResourceManager resources)
        {
            part = data; customization = owner;
            bool unlocked = resources.IsUnlocked(data);
            if (selectButton != null) { selectButton.onClick.RemoveListener(Select); selectButton.onClick.AddListener(Select); }
            if (icon != null) { icon.sprite = data.icon; icon.enabled = data.icon != null; }
            if (lockIcon != null) lockIcon.SetActive(!unlocked);
            if (selectedIndicator != null) selectedIndicator.SetActive(owner.SelectedPart == data);
            if (equippedIndicator != null) equippedIndicator.SetActive(resources.Equipped(data.slot) == data);
            if (nameText != null) nameText.text = string.IsNullOrWhiteSpace(data.displayName) ? data.name : data.displayName;
            if (priceText != null)
            {
                resources.CustomizationCatalog.TryGetPrice(data.partId, out int price);
                priceText.text = unlocked ? string.Empty : price.ToString("N0");
            }
        }
        public void Select() { if (customization != null) customization.SelectPart(part); }
        private void OnDestroy() { if (selectButton != null) selectButton.onClick.RemoveListener(Select); }
    }
}
