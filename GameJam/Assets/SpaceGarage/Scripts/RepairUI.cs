using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace SpaceGarage
{
    public sealed class RepairUI : MonoBehaviour
    {
        [SerializeField] private CustomerRepairManager workshop = null;
        [SerializeField] private RepairInteraction interaction = null;
        [Header("Your Canvas panels and buttons")]
        [SerializeField] private GameObject mainMenuPanel = null;
        [SerializeField] private GameObject repairPanel = null;
        [SerializeField] private Button repairCustomerButton = null;
        [SerializeField] private Button submitButton = null;
        [SerializeField] private TMP_Text moneyText = null;
        [SerializeField] private TMP_Text paymentText = null;
        [SerializeField] private TMP_Text jobStatusText = null;
        [Header("Bottom inventory bar, layout supplied by you")]
        [SerializeField] private RectTransform inventoryContainer = null;
        [SerializeField] private RepairMaterialDragItem inventoryItemPrefab = null;
        [Header("Existing tooltip, shown by SetActive")]
        [SerializeField] private RectTransform tooltipPanel = null;
        [SerializeField] private TMP_Text partNameText = null;
        [SerializeField] private TMP_Text damageText = null;
        [SerializeField] private RectTransform requirementContainer = null;
        [SerializeField] private RepairRequirementRow requirementRowPrefab = null;
        [SerializeField] private Vector2 tooltipOffset = new Vector2(20, -20);
        private ResourceManager resources;
        private readonly List<RepairMaterialDragItem> inventoryRows = new List<RepairMaterialDragItem>();
        private readonly List<RepairRequirementRow> requirementRows = new List<RepairRequirementRow>();

        private void OnEnable()
        {
            if (workshop == null) workshop = FindFirstObjectByType<CustomerRepairManager>();
            if (interaction == null) interaction = FindFirstObjectByType<RepairInteraction>();
            if (workshop != null) workshop.Changed += Refresh;
            if (interaction != null) interaction.HoverChanged += Refresh;
            if (repairCustomerButton != null) repairCustomerButton.onClick.AddListener(OpenRepair);
            if (submitButton != null) submitButton.onClick.AddListener(Submit);
            if (tooltipPanel != null)
            {
                var group = tooltipPanel.GetComponent<CanvasGroup>(); if (group == null) group = tooltipPanel.gameObject.AddComponent<CanvasGroup>();
                group.blocksRaycasts = false; group.interactable = false;
            }
            BindResources(); Refresh();
        }
        private void Update() => BindResources();
        private void BindResources()
        {
            if (resources == ResourceManager.Instance) return;
            if (resources != null) resources.changed.RemoveListener(Refresh);
            resources = ResourceManager.Instance;
            if (resources != null) resources.changed.AddListener(Refresh);
            Refresh();
        }
        private void OpenRepair() { if (workshop != null) workshop.OpenRepair(); }
        private void Submit() { if (workshop != null) workshop.SubmitRepair(); }

        public void Refresh()
        {
            bool open = workshop != null && workshop.IsOpen;
            if (mainMenuPanel != null && mainMenuPanel != gameObject) mainMenuPanel.SetActive(!open);
            if (repairPanel != null && repairPanel != gameObject) repairPanel.SetActive(open);
            if (repairCustomerButton != null) repairCustomerButton.interactable = !open;
            if (submitButton != null) submitButton.interactable = workshop != null && workshop.CanSubmit;
            if (moneyText != null) moneyText.text = resources != null ? resources.Money.ToString("N0") : "0";
            if (paymentText != null) paymentText.text = workshop != null ? workshop.QuotedPayment.ToString("N0") : "0";
            if (jobStatusText != null && workshop != null)
            {
                switch (workshop.State)
                {
                    case CustomerRepairState.Arriving: jobStatusText.text = "ยานลูกค้ากำลังเข้ามา"; break;
                    case CustomerRepairState.Docked: jobStatusText.text = workshop.CanSubmit ? "ซ่อมครบแล้ว กดส่งงานได้" : "ลากวัสดุไปเติมที่ชิ้นส่วน"; break;
                    case CustomerRepairState.Departing: jobStatusText.text = "ส่งงานแล้ว ยานกำลังออก"; break;
                    case CustomerRepairState.WaitingForNext: jobStatusText.text = "รอลูกค้าคันถัดไป"; break;
                    default: jobStatusText.text = string.Empty; break;
                }
            }
            RefreshInventory(); RefreshTooltip();
        }

        private void RefreshInventory()
        {
            if (resources == null || inventoryContainer == null || inventoryItemPrefab == null) return;
            int used = 0;
            foreach (var material in resources.MaterialCatalog)
            {
                if (material == null || resources.Inventory.Count(material.itemId) <= 0) continue;
                if (used >= inventoryRows.Count) inventoryRows.Add(Instantiate(inventoryItemPrefab, inventoryContainer, false));
                var row = inventoryRows[used++];
                row.Bind(material, resources, interaction); row.gameObject.SetActive(true);
            }
            for (int i = used; i < inventoryRows.Count; i++) inventoryRows[i].gameObject.SetActive(false);
        }

        private void RefreshTooltip()
        {
            var part = interaction != null ? interaction.HoveredPart : null;
            bool show = part != null && workshop != null && workshop.State == CustomerRepairState.Docked;
            if (tooltipPanel != null) tooltipPanel.gameObject.SetActive(show);
            if (!show) return;
            if (partNameText != null) partNameText.text = part.Slot.ToString();
            if (damageText != null) damageText.text = $"เสียหาย {part.DamagePercent}%";
            if (requirementContainer == null || requirementRowPrefab == null) return;
            for (int i = 0; i < part.Requirements.Count; i++)
            {
                if (i >= requirementRows.Count) requirementRows.Add(Instantiate(requirementRowPrefab, requirementContainer, false));
                requirementRows[i].Display(part.Requirements[i], resources != null ? resources.Inventory : null);
                requirementRows[i].gameObject.SetActive(true);
            }
            for (int i = part.Requirements.Count; i < requirementRows.Count; i++) requirementRows[i].gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            if (tooltipPanel == null || !tooltipPanel.gameObject.activeInHierarchy || Mouse.current == null) return;
            var parent = tooltipPanel.parent as RectTransform;
            var canvas = tooltipPanel.GetComponentInParent<Canvas>();
            if (parent == null || canvas == null) return;
            Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, Mouse.current.position.ReadValue(), camera, out var point)) return;
            point += tooltipOffset;
            Vector2 size = tooltipPanel.rect.size;
            point.x = Mathf.Clamp(point.x, parent.rect.xMin + size.x * tooltipPanel.pivot.x, parent.rect.xMax - size.x * (1 - tooltipPanel.pivot.x));
            point.y = Mathf.Clamp(point.y, parent.rect.yMin + size.y * tooltipPanel.pivot.y, parent.rect.yMax - size.y * (1 - tooltipPanel.pivot.y));
            tooltipPanel.position = parent.TransformPoint(point);
        }
        private void OnDisable()
        {
            if (resources != null) resources.changed.RemoveListener(Refresh);
            resources = null;
            if (workshop != null) workshop.Changed -= Refresh;
            if (interaction != null) interaction.HoverChanged -= Refresh;
            if (repairCustomerButton != null) repairCustomerButton.onClick.RemoveListener(OpenRepair);
            if (submitButton != null) submitButton.onClick.RemoveListener(Submit);
            if (tooltipPanel != null) tooltipPanel.gameObject.SetActive(false);
        }
        private void OnDestroy()
        {
            foreach (var row in inventoryRows) if (row != null) Destroy(row.gameObject);
            foreach (var row in requirementRows) if (row != null) Destroy(row.gameObject);
        }
    }
}
