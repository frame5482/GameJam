using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceGarage
{
    // Attach to a row made by the user. Does not create, move, or activate UI objects.
    public sealed class ShipTrackedMaterialRow : MonoBehaviour
    {
        [SerializeField] private Image icon = null;
        [SerializeField] private TMP_Text materialName = null;
        [SerializeField] private TMP_Text amountText = null;

        public void Display(ShipMaterialData material, int owned, int required)
        {
            if (icon != null)
            {
                icon.sprite = material != null ? material.icon : null;
                icon.enabled = icon.sprite != null;
            }
            if (materialName != null)
                materialName.text = material == null ? string.Empty :
                    string.IsNullOrWhiteSpace(material.displayName) ? material.itemId : material.displayName;
            if (amountText != null)
                amountText.text = material != null ? $"{owned}/{required}" : string.Empty;
        }
    }
}
