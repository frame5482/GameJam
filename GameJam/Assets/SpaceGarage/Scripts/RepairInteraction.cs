using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace SpaceGarage
{
    public sealed class RepairInteraction : MonoBehaviour
    {
        [SerializeField] private CustomerRepairManager workshop = null;
        [SerializeField] private Camera viewCamera = null;
        [SerializeField] private LayerMask partLayers = ~0;
        public event Action HoverChanged;
        public RepairableShipPart HoveredPart { get; private set; }
        public bool IsDragging => dragSource != null;
        public Camera ViewCamera => viewCamera != null ? viewCamera : Camera.main;
        private RepairMaterialDragItem dragSource;
        private readonly List<RaycastResult> uiHits = new List<RaycastResult>();

        private void Update()
        {
            if (workshop == null || workshop.State != CustomerRepairState.Docked || Mouse.current == null || !Application.isFocused)
            { SetHover(null); return; }
            Vector2 position = Mouse.current.position.ReadValue();
            SetHover(BlockingUI(position) ? null : FindPart(position));
        }

        public bool BeginDrag(RepairMaterialDragItem source, ShipMaterialData material)
        {
            if (IsDragging || workshop == null || workshop.State != CustomerRepairState.Docked || material == null ||
                ResourceManager.Instance == null || ResourceManager.Instance.Inventory.Count(material.itemId) <= 0) return false;
            dragSource = source;
            return true;
        }
        public int EndDrag(RepairMaterialDragItem source, ShipMaterialData material, Vector2 screenPosition)
        {
            if (dragSource != source) return 0;
            dragSource = null;
            var part = BlockingUI(screenPosition) ? null : FindPart(screenPosition);
            int accepted = workshop != null ? workshop.TryDeliver(part, material) : 0;
            SetHover(part);
            return accepted;
        }
        public void CancelDrag(RepairMaterialDragItem source)
        { if (dragSource == source) dragSource = null; }

        private bool BlockingUI(Vector2 position)
        {
            if (EventSystem.current == null) return false;
            uiHits.Clear();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = position }, uiHits);
            foreach (var hit in uiHits)
                if (hit.gameObject.GetComponentInParent<RepairMaterialDragItem>() != null || hit.gameObject.GetComponentInParent<Selectable>() != null)
                    return true;
            return false;
        }
        private RepairableShipPart FindPart(Vector2 screenPosition)
        {
            if (workshop == null || workshop.State != CustomerRepairState.Docked) return null;
            if (viewCamera == null) viewCamera = Camera.main;
            if (viewCamera == null || !viewCamera.pixelRect.Contains(screenPosition)) return null;
            Ray ray = viewCamera.ScreenPointToRay(screenPosition);
            RepairableShipPart selected = null; float closest = float.PositiveInfinity;
            foreach (var hit in Physics.RaycastAll(ray, 1000, partLayers, QueryTriggerInteraction.Collide))
            {
                var part = hit.collider.GetComponentInParent<RepairableShipPart>();
                if (part == null || part.Owner != workshop.Customer || hit.distance >= closest) continue;
                selected = part; closest = hit.distance;
            }
            return selected;
        }
        private void SetHover(RepairableShipPart part)
        {
            if (HoveredPart == part) return;
            if (HoveredPart != null) HoveredPart.SetHovered(false);
            HoveredPart = part;
            if (HoveredPart != null) HoveredPart.SetHovered(true);
            HoverChanged?.Invoke();
        }
        private void OnDisable() { dragSource = null; SetHover(null); }
    }
}
