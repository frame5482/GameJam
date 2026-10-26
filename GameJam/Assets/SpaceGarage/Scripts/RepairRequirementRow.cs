using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceGarage
{
    public sealed class RepairRequirementRow : MonoBehaviour
    {
        [SerializeField] private Image icon = null;
        [SerializeField] private TMP_Text materialName = null;
        [SerializeField] private TMP_Text amountText = null;
        public void Display(RepairRequirement requirement, ShipInventory inventory)
        {
            if (icon != null) { icon.sprite = requirement.material.icon; icon.enabled = icon.sprite != null; }
            if (materialName != null) materialName.text = requirement.material.displayName;
            if (amountText != null)
                amountText.text = requirement.Remaining == 0 ? "ครบแล้ว" : $"ต้องใช้ {requirement.Remaining} · มี {(inventory != null ? inventory.Count(requirement.material.itemId) : 0)}";
        }
    }
}
