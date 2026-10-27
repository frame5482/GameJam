using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Audio only: preserves the user's button visuals and events.
[DisallowMultipleComponent]
public sealed class UISfxButtonHover : MonoBehaviour, IPointerEnterHandler
{
    void OnEnable() => UISfxManager.BindButton(GetComponent<Button>());
    public void OnPointerEnter(PointerEventData eventData)
    {
        var button = GetComponent<Button>();
        if (button != null && button.isActiveAndEnabled && button.IsInteractable()) UISfxManager.PlayHover();
    }
}
