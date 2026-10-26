using System;
using UnityEngine;

namespace SpaceGarage
{
    public sealed class ShipCustomization : MonoBehaviour
    {
        [SerializeField] private CustomerRepairManager workshop = null;
        [Header("Preview position and four part mounts")]
        [SerializeField] private Transform previewRoot = null;
        [SerializeField] private Transform gunMount = null;
        [SerializeField] private Transform armorPlateMount = null;
        [SerializeField] private Transform wingMount = null;
        [SerializeField] private Transform jetMount = null;
        [Header("Player ship docking")]
        [SerializeField, Min(0.01f)] private float dockingFlightSpeed = 25;
        public event Action Changed;
        public bool IsOpen { get; private set; }
        public ShipPartSlot SelectedSlot { get; private set; }
        public ShipPartStats SelectedPart { get; private set; }
        private ResourceManager resources;
        private enum DockingState { Docked, Departing, Away, Returning }
        private DockingState dockingState;
        private Vector3 dockLocalPosition;
        private bool dockCaptured;
        private Vector3 DockPosition => previewRoot.parent != null ? previewRoot.parent.TransformPoint(dockLocalPosition) : dockLocalPosition;
        private readonly CustomizationPreviewPart[] parts = new CustomizationPreviewPart[4];
        private readonly ShipPartStats[] displayed = new ShipPartStats[4];
        private void OnEnable()
        {
            resources = ResourceManager.Instance;
            if (resources != null) resources.changed.AddListener(ResourceChanged);
            if (workshop != null) workshop.Changed += RefreshPresentation;
            if (previewRoot != null && !dockCaptured) { dockLocalPosition = previewRoot.localPosition; dockCaptured = true; }
            RefreshPresentation();
        }
        public void OpenCustomization()
        {
            if (IsOpen || (workshop != null && workshop.IsOpen)) return;
            if (resources == null || resources.CustomizationCatalog == null || previewRoot == null)
            { Debug.LogWarning("Customization requires ResourceManager with Customization Catalog and Preview Root.", this); return; }
            IsOpen = true; previewRoot.gameObject.SetActive(true);
            SelectedSlot = ShipPartSlot.Gun; SelectedPart = resources.Equipped(SelectedSlot);
            RefreshPreview(); Changed?.Invoke();
        }
        public void CloseCustomization()
        {
            if (!IsOpen) return;
            IsOpen = false; RefreshPresentation();
            resources.SaveProgress(); Changed?.Invoke();
        }
        public void SelectCategory(int slot)
        {
            if (!IsOpen || slot < 0 || slot >= 4) return;
            SelectedSlot = (ShipPartSlot)slot; SelectedPart = resources.Equipped(SelectedSlot);
            Highlight(); Changed?.Invoke();
        }
        public void SelectPart(ShipPartStats part)
        {
            if (!IsOpen || part == null || part.slot != SelectedSlot || resources.CustomizationCatalog.Find(part.partId) != part) return;
            SelectedPart = part; Changed?.Invoke();
        }
        public void EquipSelected()
        { if (IsOpen && resources.TryEquip(SelectedPart)) resources.SaveProgress(); }
        public void UnlockSelected()
        { if (IsOpen && resources.TryUnlock(SelectedPart)) resources.SaveProgress(); }
        private void ResourceChanged()
        { RefreshPresentation(); if (IsOpen) Changed?.Invoke(); }
        private void RefreshPresentation()
        {
            if (previewRoot == null) return;
            bool available = resources != null && resources.CustomizationCatalog != null;
            if (!available) { previewRoot.gameObject.SetActive(false); return; }
            bool repairing = workshop != null && workshop.IsOpen;
            if (repairing && (dockingState == DockingState.Docked || dockingState == DockingState.Returning))
                dockingState = DockingState.Departing;
            else if (!repairing && (dockingState == DockingState.Away || dockingState == DockingState.Departing))
            {
                if (dockingState == DockingState.Away && workshop != null && workshop.InPoint != null)
                    previewRoot.position = workshop.InPoint.position;
                dockingState = DockingState.Returning;
            }
            previewRoot.gameObject.SetActive(dockingState != DockingState.Away);
            if (previewRoot.gameObject.activeSelf) { RefreshPreview(); FaceExit(); SetPreviewEngines(); }
        }
        private void FaceExit()
        {
            if (previewRoot == null || workshop == null || workshop.OutPoint == null) return;
            Vector3 heading = workshop.OutPoint.position - DockPosition;
            if (heading.sqrMagnitude < 0.000001f) return;
            Vector3 forward = heading.normalized;
            previewRoot.rotation = Quaternion.LookRotation(forward,
                Mathf.Abs(Vector3.Dot(forward, Vector3.up)) > 0.999f ? Vector3.forward : Vector3.up);
        }
        private void Update()
        {
            if (previewRoot == null || (dockingState != DockingState.Departing && dockingState != DockingState.Returning)) return;
            Vector3 destination = dockingState == DockingState.Departing && workshop != null && workshop.OutPoint != null
                ? workshop.OutPoint.position : DockPosition;
            previewRoot.position = Vector3.MoveTowards(previewRoot.position, destination, Mathf.Max(0.01f, dockingFlightSpeed) * Time.deltaTime);
            FaceExit();
            if ((previewRoot.position - destination).sqrMagnitude > 0.0004f) return;
            previewRoot.position = destination;
            bool leaving = dockingState == DockingState.Departing;
            dockingState = leaving ? DockingState.Away : DockingState.Docked;
            SetPreviewEngines();
            if (leaving) previewRoot.gameObject.SetActive(false);
        }
        private void SetPreviewEngines()
        {
            bool flying = dockingState == DockingState.Departing || dockingState == DockingState.Returning;
            foreach (var effect in previewRoot.GetComponentsInChildren<ShipBoostEffect>(true)) effect.SetFlightState(flying, false);
        }
        private void RefreshPreview()
        {
            Transform[] mounts = { gunMount, armorPlateMount, wingMount, jetMount };
            for (int i = 0; i < 4; i++)
            {
                var data = resources.Equipped((ShipPartSlot)i);
                if (displayed[i] == data && parts[i] != null) continue;
                if (parts[i] != null) { parts[i].gameObject.SetActive(false); Destroy(parts[i].gameObject); }
                parts[i] = null; displayed[i] = data;
                if (data == null) continue;
                Transform parent = mounts[i] != null && mounts[i].IsChildOf(previewRoot) ? mounts[i] : previewRoot;
                var wrapper = new GameObject(((ShipPartSlot)i) + "_Customization"); wrapper.transform.SetParent(parent, false);
                var model = Instantiate(data.partPrefab, wrapper.transform, false);
                model.transform.localPosition = Vector3.zero; model.transform.localRotation = Quaternion.identity;
                foreach (var gun in model.GetComponentsInChildren<ShipGun>(true)) gun.enabled = false;
                foreach (var effect in model.GetComponentsInChildren<ShipBoostEffect>(true)) effect.SetFlightState(false, false);
                foreach (var body in model.GetComponentsInChildren<Rigidbody>(true)) { body.isKinematic = true; body.useGravity = false; }
                parts[i] = wrapper.AddComponent<CustomizationPreviewPart>(); parts[i].Initialize((ShipPartSlot)i, model.transform);
            }
            Highlight();
        }
        private void Highlight()
        { for (int i = 0; i < 4; i++) if (parts[i] != null) parts[i].SetSelected(IsOpen && i == (int)SelectedSlot); }
        private void ClearPreview()
        {
            for (int i = 0; i < 4; i++)
            { if (parts[i] != null) { parts[i].gameObject.SetActive(false); Destroy(parts[i].gameObject); } parts[i] = null; displayed[i] = null; }
        }
        private void OnDisable()
        {
            if (resources != null) resources.changed.RemoveListener(ResourceChanged);
            if (workshop != null) workshop.Changed -= RefreshPresentation;
            IsOpen = false; ClearPreview();
            if (previewRoot != null)
            {
                if (dockCaptured) previewRoot.localPosition = dockLocalPosition;
                previewRoot.gameObject.SetActive(false);
            }
            dockingState = DockingState.Docked;
        }
    }
}
