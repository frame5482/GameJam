using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceGarage
{
    public sealed class SettingsUI : MonoBehaviour
    {
        [Header("User-created audio sliders (0 to 1)")]
        [SerializeField] Slider masterVolume;
        [SerializeField] Slider musicVolume;
        [SerializeField] Slider sfxVolume;
        [Header("User-created display controls")]
        [SerializeField] TMP_Dropdown resolutionDropdown;
        [SerializeField] Toggle fullscreenToggle;
        [SerializeField] Button applyDisplayButton;
        readonly List<Vector2Int> resolutions = new List<Vector2Int>();

        void OnEnable()
        {
            BindSlider(masterVolume, GarageSettings.MasterVolume, GarageSettings.SetMaster);
            BindSlider(musicVolume, GarageSettings.MusicVolume, GarageSettings.SetMusic);
            BindSlider(sfxVolume, GarageSettings.SfxVolume, GarageSettings.SetSfx);
            PopulateResolutions();
            if (fullscreenToggle != null) fullscreenToggle.SetIsOnWithoutNotify(GarageSettings.Fullscreen);
            if (applyDisplayButton != null) applyDisplayButton.onClick.AddListener(ApplyDisplay);
            UISfxManager.BindButtons(gameObject);
        }

        static void BindSlider(Slider slider, float value, UnityEngine.Events.UnityAction<float> listener)
        {
            if (slider == null) return;
            slider.minValue = 0; slider.maxValue = 1; slider.wholeNumbers = false;
            slider.SetValueWithoutNotify(value);
            slider.onValueChanged.AddListener(listener);
        }

        void PopulateResolutions()
        {
            resolutions.Clear();
            foreach (var resolution in Screen.resolutions) AddResolution(resolution.width, resolution.height);
            AddResolution(Screen.width, Screen.height);
            AddResolution(GarageSettings.DisplayWidth, GarageSettings.DisplayHeight);
            resolutions.Sort((a, b) => a.x != b.x ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y));
            if (resolutionDropdown == null) return;
            var labels = new List<string>();
            int selected = 0;
            for (int i = 0; i < resolutions.Count; i++)
            {
                var size = resolutions[i]; labels.Add(size.x + " x " + size.y);
                if (size.x == GarageSettings.DisplayWidth && size.y == GarageSettings.DisplayHeight) selected = i;
            }
            resolutionDropdown.ClearOptions(); resolutionDropdown.AddOptions(labels);
            resolutionDropdown.SetValueWithoutNotify(selected); resolutionDropdown.RefreshShownValue();
        }

        void AddResolution(int width, int height)
        {
            var size = new Vector2Int(width, height);
            if (width > 0 && height > 0 && !resolutions.Contains(size)) resolutions.Add(size);
        }

        public void ApplyDisplay()
        {
            int index = resolutionDropdown != null ? resolutionDropdown.value : -1;
            var size = index >= 0 && index < resolutions.Count ? resolutions[index] :
                new Vector2Int(GarageSettings.DisplayWidth, GarageSettings.DisplayHeight);
            GarageSettings.ApplyDisplay(size.x, size.y,
                fullscreenToggle != null ? fullscreenToggle.isOn : GarageSettings.Fullscreen);
        }

        void OnDisable()
        {
            if (masterVolume != null) masterVolume.onValueChanged.RemoveListener(GarageSettings.SetMaster);
            if (musicVolume != null) musicVolume.onValueChanged.RemoveListener(GarageSettings.SetMusic);
            if (sfxVolume != null) sfxVolume.onValueChanged.RemoveListener(GarageSettings.SetSfx);
            if (applyDisplayButton != null) applyDisplayButton.onClick.RemoveListener(ApplyDisplay);
            GarageSettings.Save();
        }
    }
}
