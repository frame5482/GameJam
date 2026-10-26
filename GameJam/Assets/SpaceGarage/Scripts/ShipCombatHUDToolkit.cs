using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace SpaceGarage
{
    // The example's presentation is replaceable; gameplay remains in ShipController.
    public sealed class ShipCombatHUDToolkit : MonoBehaviour
    {
        [SerializeField] private ShipController ship = null;
        [SerializeField] private bool visible = true;
        [SerializeField] private bool showMouseReticle = true;
        [Header("Replace with your production UI assets (optional)")]
        [SerializeField] private VisualTreeAsset layout = null;
        [SerializeField] private PanelSettings panelSettings = null;
        private UIDocument document;
        private PanelSettings ownedPanel;
        private VisualElement view;
        private VisualElement hud, healthFill, boostFill, dodgeFill, reticle, materialList;
        private Label healthValue, boostValue, speedValue, dodgeValue, modeStatus, emptyMaterials;
        private readonly List<ShipMaterialRequirement> tracked = new List<ShipMaterialRequirement>();
        private readonly List<MaterialRow> rows = new List<MaterialRow>();
        private ShipInventory subscribedInventory;
        private float nextShipSearch;
        private bool layoutWarningShown;

        private sealed class MaterialRow
        {
            public VisualElement root;
            public Label count;
            public ShipMaterialRequirement requirement;
        }

        private void OnEnable() { layoutWarningShown = false; Build(); }

        private void Build()
        {
            if (view != null && document != null && ReferenceEquals(document.rootVisualElement, view) && view.panel != null) return;
            var tree = layout != null ? layout : Resources.Load<VisualTreeAsset>("SpaceGarageHUD/CombatHUD");
            if (tree == null)
            {
                if (!layoutWarningShown) Debug.LogWarning("Combat HUD needs a UXML Layout or the example resource.", this);
                layoutWarningShown = true;
                return;
            }
            document = GetComponent<UIDocument>();
            if (document == null) document = gameObject.AddComponent<UIDocument>();
            if (panelSettings != null) document.panelSettings = panelSettings;
            else if (document.panelSettings == null)
            {
                if (ownedPanel == null)
                {
                    ownedPanel = ScriptableObject.CreateInstance<PanelSettings>();
                    ownedPanel.name = "Combat HUD Example Panel (Runtime)";
                    ownedPanel.scaleMode = PanelScaleMode.ScaleWithScreenSize;
                    ownedPanel.referenceResolution = new Vector2Int(1920, 1080);
                    ownedPanel.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
                    ownedPanel.match = 0.5f;
                    ownedPanel.sortingOrder = 100;
                    ownedPanel.themeStyleSheet = Resources.Load<ThemeStyleSheet>("SpaceGarageHUD/CombatHUDTheme");
                }
                document.panelSettings = ownedPanel;
            }
            if (document.visualTreeAsset != tree) document.visualTreeAsset = tree;
            document.enabled = true;
            if (document.rootVisualElement == null) return;
            view = document.rootVisualElement;
            view.style.position = Position.Absolute;
            view.style.left = view.style.top = view.style.right = view.style.bottom = 0;
            view.style.width = Length.Percent(100);
            view.style.height = Length.Percent(100);
            view.style.flexGrow = 1;
            // This is a display-only HUD: it must not consume mouse aim/shooting or keyboard input.
            IgnorePicking(view);
            hud = view.Q<VisualElement>("combat-hud") ?? view;
            healthValue = view.Q<Label>("health-value");
            boostValue = view.Q<Label>("boost-value");
            speedValue = view.Q<Label>("speed-value");
            dodgeValue = view.Q<Label>("dodge-value");
            modeStatus = view.Q<Label>("mode-status");
            healthFill = view.Q<VisualElement>("health-fill");
            boostFill = view.Q<VisualElement>("boost-fill");
            dodgeFill = view.Q<VisualElement>("dodge-fill");
            reticle = view.Q<VisualElement>("mouse-reticle");
            materialList = view.Q<VisualElement>("materials-list");
            emptyMaterials = view.Q<Label>("materials-empty");
            RebuildMaterialRows();
            SetVisible(visible);
        }

        private static void IgnorePicking(VisualElement element)
        {
            element.pickingMode = PickingMode.Ignore;
            element.focusable = false;
            foreach (var child in element.Children()) IgnorePicking(child);
        }

        public void SetVisible(bool value)
        {
            visible = value;
            if (view != null) view.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void SetShip(ShipController value)
        {
            Unsubscribe();
            ship = value;
            BindInventory();
        }

        public void SetTrackedMaterials(IEnumerable<ShipMaterialRequirement> requirements)
        {
            tracked.Clear();
            if (requirements != null)
                foreach (var item in requirements)
                    if (item != null && item.material != null && !string.IsNullOrWhiteSpace(item.material.itemId))
                        tracked.Add(new ShipMaterialRequirement
                        { material = item.material, requiredAmount = Mathf.Max(1, item.requiredAmount) });
            RebuildMaterialRows();
        }

        private void Update()
        {
            if (view == null || document == null || !ReferenceEquals(document.rootVisualElement, view) || view.panel == null)
            { Build(); if (view == null) return; }
            SetVisible(visible);
            if (ship == null && Time.unscaledTime >= nextShipSearch)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                ship = player != null ? player.GetComponent<ShipController>() : null;
                nextShipSearch = Time.unscaledTime + 0.5f;
            }
            BindInventory();
            if (!visible) return;
            float hp = ship != null ? ship.Health : 0, hpMax = ship != null ? ship.Stats.health : 0;
            float fuel = ship != null ? ship.Boost : 0, fuelMax = ship != null ? ship.Stats.boosterCapacity : 0;
            SetText(healthValue, $"{Mathf.CeilToInt(hp)} / {Mathf.CeilToInt(hpMax)}");
            SetText(boostValue, $"{Mathf.CeilToInt(fuel)} / {Mathf.CeilToInt(fuelMax)}");
            SetText(speedValue, (ship != null ? ship.CurrentSpeed : 0).ToString("0.0"));
            SetText(dodgeValue, ship == null ? "--" : ship.IsRolling ? "ROLLING" :
                ship.CanDodge ? "READY" : $"{ship.DodgeCooldownRemaining:0.0}s");
            SetText(modeStatus, ship == null ? "NO SHIP LINK" : !ship.IsAlive ? "SYSTEM OFFLINE" :
                ship.IsRolling ? "EVASIVE MANEUVER" : ship.IsBoosting ? "BOOST ACTIVE" : "CRUISE // ONLINE");
            Fill(healthFill, hpMax > 0 ? hp / hpMax : 0);
            Fill(boostFill, fuelMax > 0 ? fuel / fuelMax : 0);
            Fill(dodgeFill, ship != null ? ship.DodgeReadyProgress : 0);
            hud.EnableInClassList("boosting", ship != null && ship.IsBoosting);
            hud.EnableInClassList("low-health", ship != null && hpMax > 0 && hp <= hpMax * 0.25f);
        }

        private void LateUpdate()
        {
            if (reticle == null || view == null || view.panel == null) return;
            bool show = visible && showMouseReticle && Mouse.current != null && Application.isFocused;
            Vector2 mouse = show ? Mouse.current.position.ReadValue() : Vector2.zero;
            show &= mouse.x >= 0 && mouse.y >= 0 && mouse.x <= Screen.width && mouse.y <= Screen.height;
            reticle.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            if (!show) return;
            Vector2 position = RuntimePanelUtils.ScreenToPanel(view.panel, new Vector2(mouse.x, Screen.height - mouse.y));
            reticle.style.left = position.x - 15;
            reticle.style.top = position.y - 15;
        }

        private static void SetText(Label label, string text) { if (label != null && label.text != text) label.text = text; }
        private static void Fill(VisualElement element, float ratio)
        { if (element != null) element.style.width = Length.Percent(Mathf.Clamp01(ratio) * 100); }

        private void RebuildMaterialRows()
        {
            rows.Clear();
            if (materialList == null) return;
            materialList.Clear();
            foreach (var item in tracked)
            {
                var row = new VisualElement(); row.AddToClassList("material-row");
                var icon = new Image { sprite = item.material.icon, scaleMode = ScaleMode.ScaleToFit };
                icon.AddToClassList("material-icon"); row.Add(icon);
                var name = new Label(string.IsNullOrWhiteSpace(item.material.displayName) ? item.material.itemId : item.material.displayName);
                name.AddToClassList("material-name"); row.Add(name);
                var count = new Label(); count.AddToClassList("material-count"); row.Add(count);
                IgnorePicking(row);
                materialList.Add(row);
                rows.Add(new MaterialRow { root = row, count = count, requirement = item });
            }
            if (emptyMaterials != null) emptyMaterials.style.display = rows.Count == 0 ? DisplayStyle.Flex : DisplayStyle.None;
            RefreshMaterials();
        }

        private void BindInventory()
        {
            var next = ship != null ? ship.Inventory : null;
            if (ReferenceEquals(next, subscribedInventory)) return;
            Unsubscribe();
            subscribedInventory = next;
            if (subscribedInventory != null) subscribedInventory.changed.AddListener(RefreshMaterials);
            RefreshMaterials();
        }
        private void RefreshMaterials()
        {
            foreach (var row in rows)
            {
                int amount = subscribedInventory != null ? subscribedInventory.Count(row.requirement.material.itemId) : 0;
                SetText(row.count, $"{amount} / {row.requirement.requiredAmount}");
                row.root.EnableInClassList("material-complete", amount >= row.requirement.requiredAmount);
            }
        }
        private void Unsubscribe()
        {
            if (subscribedInventory != null) subscribedInventory.changed.RemoveListener(RefreshMaterials);
            subscribedInventory = null;
        }
        private void OnDisable()
        {
            Unsubscribe();
            if (document != null) document.enabled = false;
            view = null;
            rows.Clear();
        }
        private void OnDestroy()
        {
            if (ownedPanel != null)
            {
                if (document != null && document.panelSettings == ownedPanel) document.panelSettings = null;
                Destroy(ownedPanel);
            }
        }
    }
}
