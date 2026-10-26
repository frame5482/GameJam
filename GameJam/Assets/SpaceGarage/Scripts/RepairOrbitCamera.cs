using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

namespace SpaceGarage
{
    [DefaultExecutionOrder(-100)]
    public sealed class RepairOrbitCamera : MonoBehaviour
    {
        [SerializeField] private CustomerRepairManager workshop = null;
        [SerializeField] private Transform orbitRig = null;
        [Header("Cinemachine cameras for each page")]
        [SerializeField] private CinemachineCamera mainMenuCamera = null;
        [SerializeField] private CinemachineCamera repairCamera = null;
        [SerializeField] private ShipCustomization customization = null;
        [SerializeField] private CinemachineCamera customizationCamera = null;
        [SerializeField] private float mouseSensitivity = 0.15f;
        [SerializeField] private float zoomSensitivity = 0.015f;
        [SerializeField] private float minimumDistance = 3;
        [SerializeField] private float maximumDistance = 60;
        [SerializeField] private float minimumPitch = -80;
        [SerializeField] private float maximumPitch = 80;
        private float yaw, pitch, distance;
        private bool initialized;

        private void Awake()
        {
            if (workshop == null) workshop = FindFirstObjectByType<CustomerRepairManager>();
            if (customization == null) customization = FindFirstObjectByType<ShipCustomization>();
            foreach (var camera in FindObjectsByType<CinemachineCamera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (mainMenuCamera == null && camera.name == "MainMenuCamera") mainMenuCamera = camera;
                if (repairCamera == null && camera.name == "CinemachineCamera") repairCamera = camera;
                if (customizationCamera == null && camera.name == "CustomizationCamera") customizationCamera = camera;
            }
        }

        private static void SetCamera(CinemachineCamera camera, bool selected)
        {
            if (camera == null) return;
            if (selected && !camera.gameObject.activeSelf) camera.gameObject.SetActive(true);
            if (camera.enabled != selected) camera.enabled = selected;
        }

        private void LateUpdate()
        {
            bool repairing = workshop != null && workshop.IsOpen;
            bool customizing = customization != null && customization.IsOpen;
            var selectedCamera = repairing ? repairCamera : customizing ? customizationCamera : mainMenuCamera;
            if (selectedCamera == null) selectedCamera = mainMenuCamera;
            SetCamera(mainMenuCamera, mainMenuCamera == selectedCamera);
            SetCamera(repairCamera, repairCamera == selectedCamera);
            SetCamera(customizationCamera, customizationCamera == selectedCamera);
            if (workshop == null || !workshop.IsOpen || workshop.StayPoint == null) { initialized = false; return; }
            if (repairCamera != null) orbitRig = repairCamera.transform;
            if (orbitRig == null) return;
            Vector3 center = workshop.StayPoint.position;
            if (!initialized)
            {
                Vector3 offset = orbitRig.position - center;
                distance = Mathf.Clamp(offset.magnitude, minimumDistance, maximumDistance);
                if (offset.sqrMagnitude < 0.01f) offset = new Vector3(0, 5, -10);
                yaw = Mathf.Atan2(-offset.x, -offset.z) * Mathf.Rad2Deg;
                pitch = Mathf.Asin(Mathf.Clamp(offset.normalized.y, -1, 1)) * Mathf.Rad2Deg;
                initialized = true;
            }
            var mouse = Mouse.current;
            if (mouse != null && Application.isFocused)
            {
                if (mouse.rightButton.isPressed)
                {
                    Vector2 delta = mouse.delta.ReadValue();
                    yaw += delta.x * mouseSensitivity; pitch -= delta.y * mouseSensitivity;
                }
                distance = Mathf.Clamp(distance - mouse.scroll.ReadValue().y * zoomSensitivity, minimumDistance, maximumDistance);
            }
            pitch = Mathf.Clamp(pitch, minimumPitch, maximumPitch);
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
            orbitRig.SetPositionAndRotation(center - rotation * Vector3.forward * distance, rotation);
        }
    }
}
