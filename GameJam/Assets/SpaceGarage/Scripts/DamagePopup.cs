using TMPro;
using UnityEngine;

// Restores the component already serialized on VFX/DMG.prefab.
public sealed class DamagePopup : MonoBehaviour
{
    [SerializeField] private TMP_Text damageText = null;
    [SerializeField] private float launchSpeed = 4f;
    [SerializeField] private float gravity = 10f;
    [SerializeField] private Vector2 sideSpeed = new Vector2(0.3f, 0.9f);
    [SerializeField] private Vector3 spawnOffset = new Vector3(0, 0.1f, 0);
    [SerializeField, Min(0.01f)] private float lifetime = 1f;
    [SerializeField, Min(0)] private float fadeDelay = 0.7f;
    [SerializeField, Min(0)] private float fontSize = 0;
    [SerializeField, Min(0.01f)] private float sizeScale = 1f;
    [SerializeField, Min(0)] private float popScale = 1.25f;
    [SerializeField, Min(0.01f)] private float popDuration = 0.15f;
    private Camera viewCamera;
    private Vector3 velocity, originalScale;
    private Color originalColor;
    private float elapsed;
    private bool initialized;

    public void Initialize(float amount, Camera camera)
    {
        if (damageText == null) damageText = GetComponentInChildren<TMP_Text>(true);
        viewCamera = camera;
        originalScale = transform.localScale * sizeScale;
        transform.position += spawnOffset;
        Vector3 up = camera != null ? camera.transform.up : Vector3.up;
        Vector3 right = camera != null ? camera.transform.right : Vector3.right;
        float side = Random.Range(Mathf.Min(sideSpeed.x, sideSpeed.y), Mathf.Max(sideSpeed.x, sideSpeed.y));
        velocity = up * launchSpeed + right * side * (Random.value < 0.5f ? -1 : 1);
        if (damageText != null)
        {
            damageText.text = amount.ToString("0.#");
            originalColor = damageText.color;
            if (fontSize > 0) damageText.fontSize = fontSize;
        }
        elapsed = 0;
        initialized = true;
        Destroy(gameObject, Mathf.Max(0.01f, lifetime));
    }

    private void Update()
    {
        if (!initialized) return;
        float dt = Time.deltaTime;
        elapsed += dt;
        transform.position += velocity * dt;
        velocity += Vector3.down * gravity * dt;
        transform.localScale = originalScale * Mathf.Lerp(popScale, 1, Mathf.Clamp01(elapsed / Mathf.Max(0.01f, popDuration)));
        if (damageText != null)
        {
            var color = originalColor;
            color.a *= elapsed < fadeDelay ? 1 : Mathf.Clamp01((lifetime - elapsed) / Mathf.Max(0.01f, lifetime - fadeDelay));
            damageText.color = color;
        }
    }

    private void LateUpdate()
    {
        if (viewCamera == null) viewCamera = Camera.main;
        if (viewCamera != null) transform.rotation = viewCamera.transform.rotation;
    }
}
