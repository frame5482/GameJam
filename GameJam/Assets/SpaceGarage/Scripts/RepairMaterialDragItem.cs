using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SpaceGarage
{
    public sealed class RepairMaterialDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Image icon = null;
        [SerializeField] private TMP_Text materialName = null;
        [SerializeField] private TMP_Text ownedText = null;
        [SerializeField] private RectTransform dragGhostPrefab = null;
        [Header("Result row: your bonus image object")]
        [SerializeField] private GameObject bonusIcon = null;
        private ShipMaterialData material;
        private ResourceManager resources;
        private RepairInteraction interaction;
        private RectTransform ghost;
        private Canvas canvas;
        private bool dragging; // Combat notification instances have no repair interaction.

        public void DisplayPickup(ShipMaterialData data, string itemId, int amount)
        {
            Bind(data, null, null);
            if (materialName != null)
                materialName.text = data != null && !string.IsNullOrWhiteSpace(data.displayName) ? data.displayName : itemId;
            if (ownedText != null) ownedText.text = $"+{amount}";
        }

        public void Bind(ShipMaterialData data, ResourceManager storage, RepairInteraction input)
        {
            ShowBonusIcon(false);
            material = data; resources = storage; interaction = input;
            if (icon != null) { icon.sprite = data != null ? data.icon : null; icon.enabled = icon.sprite != null; }
            if (materialName != null) materialName.text = data != null ? data.displayName : string.Empty;
            if (ownedText != null) ownedText.text = storage != null && data != null ? storage.Inventory.Count(data.itemId).ToString() : "0";
        }

        public void DisplayResult(ShipMaterialData data, string itemId, long collected, int bonus)
        {
            Bind(data, null, null);
            if (materialName != null)
                materialName.text = data != null && !string.IsNullOrWhiteSpace(data.displayName) ? data.displayName : itemId;
            if (ownedText != null)
                ownedText.text = bonus > 0 ? $"+{collected} +{bonus}" : $"+{collected}";
            ShowBonusIcon(bonus > 0);
        }

        private void ShowBonusIcon(bool visible)
        {
            // The marker must be a child image, never the row itself or its container.
            if (bonusIcon != null && bonusIcon != gameObject && bonusIcon.transform.IsChildOf(transform))
                bonusIcon.SetActive(visible);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || interaction == null || resources == null ||
                !interaction.BeginDrag(this, material)) return;
            dragging = true;
            canvas = GetComponentInParent<Canvas>();
            if (canvas != null) canvas = canvas.rootCanvas;
            if (canvas != null && dragGhostPrefab != null)
            {
                ghost = Instantiate(dragGhostPrefab, canvas.transform, false);
                ghost.SetAsLastSibling();
                var group = ghost.GetComponent<CanvasGroup>(); if (group == null) group = ghost.gameObject.AddComponent<CanvasGroup>();
                group.blocksRaycasts = false; group.interactable = false;
                var image = ghost.GetComponentInChildren<Image>(); if (image != null) image.sprite = material.icon;
                ghost.gameObject.SetActive(true);
                MoveGhost(eventData.position);
            }
        }
        public void OnDrag(PointerEventData eventData) { if (dragging) MoveGhost(eventData.position); }
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!dragging) return;
            dragging = false;
            var input = interaction;
            if (input != null) input.EndDrag(this, material, eventData.position);
            ClearGhost();
        }
        private void MoveGhost(Vector2 position)
        {
            if (ghost == null || canvas == null) return;
            var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle((RectTransform)canvas.transform, position, camera, out var world))
                ghost.position = world;
        }
        private void ClearGhost()
        {
            if (ghost == null) return;
            if (Application.isPlaying) Destroy(ghost.gameObject); else DestroyImmediate(ghost.gameObject);
            ghost = null;
        }
        private void OnDisable()
        {
            if (dragging && interaction != null) interaction.CancelDrag(this);
            dragging = false; ClearGhost();
        }
    }
}
