using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
        [SerializeField] private Button combatButton = null;
        [SerializeField] private string combatSceneName = "SampleScene";
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
        private bool loadingCombat;
        private readonly List<RepairMaterialDragItem> inventoryRows = new List<RepairMaterialDragItem>();
        private readonly List<RepairRequirementRow> requirementRows = new List<RepairRequirementRow>();

        private void OnEnable()
        {
            if (workshop == null) workshop = FindFirstObjectByType<CustomerRepairManager>();
            if (interaction == null) interaction = FindFirstObjectByType<RepairInteraction>();
            if (workshop != null) workshop.Changed += Refresh;
            if (interaction != null) interaction.HoverChanged += Refresh;
            if (repairCustomerButton != null) repairCustomerButton.onClick.AddListener(OpenRepair);
            if (combatButton != null) combatButton.onClick.AddListener(EnterCombat);
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

        public void EnterCombat()
        {
            if (loadingCombat || (workshop != null && workshop.IsOpen)) return;
            if (string.IsNullOrWhiteSpace(combatSceneName) || !Application.CanStreamedLevelBeLoaded(combatSceneName))
            {
                Debug.LogWarning("Cannot enter combat: add the combat scene to the active Build Profile Scene List and check Combat Scene Name: " + combatSceneName, this);
                return;
            }
            loadingCombat = true;
            Refresh();
            try { SceneManager.LoadSceneAsync(combatSceneName, LoadSceneMode.Single); }
            catch (System.Exception error)
            {
                loadingCombat = false; Refresh();
                Debug.LogException(error, this);
            }
        }

        public void Refresh()
        {
            bool open = workshop != null && workshop.IsOpen;
            if (mainMenuPanel != null && mainMenuPanel != gameObject) mainMenuPanel.SetActive(!open);
            if (repairPanel != null && repairPanel != gameObject) repairPanel.SetActive(open);
            if (repairCustomerButton != null) repairCustomerButton.interactable = !open;
            if (combatButton != null) combatButton.interactable = !open && !loadingCombat;
            if (submitButton != null) submitButton.interactable = workshop != null && workshop.CanSubmit;
            if (moneyText != null) moneyText.text = resources != null ? resources.Money.ToString("N0") : "0";
            if (paymentText != null) paymentText.text = workshop != null ? workshop.QuotedPayment.ToString("N0") : "0";
            if (jobStatusText != null && workshop != null)
            {
                switch (workshop.State)
                {
                    case CustomerRepairState.Arriving: jobStatusText.text = "Customer ship approaching"; break;
                    case CustomerRepairState.Docked: jobStatusText.text = workshop.CanSubmit ? "Repairs complete. Ready to deliver" : "Drag materials onto ship parts to repair"; break;
                    case CustomerRepairState.Departing: jobStatusText.text = "Job delivered. Ship departing"; break;
                    case CustomerRepairState.WaitingForNext: jobStatusText.text = "Waiting for the next customer"; break;
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
            if (damageText != null) damageText.text = $"Damage: {part.DamagePercent}%";
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
            if (tooltipPanel == null || !tooltipPanel.gameObject.activeInHierarchy) return;
            var part = interaction != null ? interaction.HoveredPart : null;
            var viewCamera = interaction != null ? interaction.ViewCamera : Camera.main;
            if (part == null || viewCamera == null) return;
            var parent = tooltipPanel.parent as RectTransform;
            var canvas = tooltipPanel.GetComponentInParent<Canvas>();
            if (parent == null || canvas == null) return;
            Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            Vector3 screenPosition = viewCamera.WorldToScreenPoint(part.transform.position);
            if (screenPosition.z <= 0) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPosition, camera, out var point)) return;
            point += tooltipOffset;
            Vector2 size = tooltipPanel.rect.size;
            point.x = Mathf.Clamp(point.x, parent.rect.xMin + size.x * tooltipPanel.pivot.x, parent.rect.xMax - size.x * (1 - tooltipPanel.pivot.x));
            point.y = Mathf.Clamp(point.y, parent.rect.yMin + size.y * tooltipPanel.pivot.y, parent.rect.yMax - size.y * (1 - tooltipPanel.pivot.y));
            if (TryGetShipRect(part.Owner, viewCamera, parent, camera, out var shipRect))
            {
                float gap = Mathf.Max(12, Mathf.Abs(tooltipOffset.x));
                Vector2 pivot = tooltipPanel.pivot;
                Vector2[] candidates = {
                    new Vector2(shipRect.xMax + gap + size.x * pivot.x, point.y),
                    new Vector2(shipRect.xMin - gap - size.x * (1 - pivot.x), point.y),
                    new Vector2(point.x, shipRect.yMax + gap + size.y * pivot.y),
                    new Vector2(point.x, shipRect.yMin - gap - size.y * (1 - pivot.y))
                };
                float bestVisibleArea = -1;
                foreach (var candidate in candidates)
                {
                    var rect = new Rect(candidate - Vector2.Scale(size, pivot), size);
                    float width = Mathf.Max(0, Mathf.Min(rect.xMax, parent.rect.xMax) - Mathf.Max(rect.xMin, parent.rect.xMin));
                    float height = Mathf.Max(0, Mathf.Min(rect.yMax, parent.rect.yMax) - Mathf.Max(rect.yMin, parent.rect.yMin));
                    float visibleArea = width * height;
                    if (visibleArea <= bestVisibleArea) continue;
                    bestVisibleArea = visibleArea; point = candidate;
                }
            }
            tooltipPanel.position = parent.TransformPoint(point);
        }

        private static bool TryGetShipRect(CustomerRepairShip ship, Camera viewCamera,
            RectTransform parent, Camera uiCamera, out Rect rect)
        {
            rect = default;
            if (ship == null) return false;
            bool found = false;
            Vector2 min = Vector2.zero, max = Vector2.zero;
            foreach (var renderer in ship.GetComponentsInChildren<Renderer>())
            {
                if (!renderer.enabled || (!(renderer is MeshRenderer) && !(renderer is SkinnedMeshRenderer))) continue;
                var bounds = renderer.bounds;
                for (int x = -1; x <= 1; x += 2)
                    for (int y = -1; y <= 1; y += 2)
                        for (int z = -1; z <= 1; z += 2)
                        {
                            var screen = viewCamera.WorldToScreenPoint(bounds.center + Vector3.Scale(bounds.extents, new Vector3(x, y, z)));
                            if (screen.z <= 0 || !RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screen, uiCamera, out var local)) continue;
                            if (!found) { min = max = local; found = true; }
                            else { min = Vector2.Min(min, local); max = Vector2.Max(max, local); }
                        }
            }
            if (found) rect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            return found;
        }
        private void OnDisable()
        {
            if (resources != null) resources.changed.RemoveListener(Refresh);
            resources = null;
            if (workshop != null) workshop.Changed -= Refresh;
            if (interaction != null) interaction.HoverChanged -= Refresh;
            if (repairCustomerButton != null) repairCustomerButton.onClick.RemoveListener(OpenRepair);
            if (combatButton != null) combatButton.onClick.RemoveListener(EnterCombat);
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
