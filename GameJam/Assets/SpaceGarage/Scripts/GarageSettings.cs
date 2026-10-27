using UnityEngine;

namespace SpaceGarage
{
    public static class GarageSettings
    {
        const string Prefix = "SpaceGarage.Settings.";
        public static float MasterVolume { get; private set; } = 1;
        public static float MusicVolume { get; private set; } = 1;
        public static float SfxVolume { get; private set; } = 1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Load()
        {
            MasterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(Prefix + "Master", 1));
            MusicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(Prefix + "Music", 1));
            SfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(Prefix + "Sfx", 1));
            AudioListener.volume = MasterVolume;
            if (PlayerPrefs.HasKey(Prefix + "Width"))
                ApplyDisplay(DisplayWidth, DisplayHeight, Fullscreen, false);
        }

        public static void SetMaster(float value) { MasterVolume = Mathf.Clamp01(value); AudioListener.volume = MasterVolume; PlayerPrefs.SetFloat(Prefix + "Master", MasterVolume); }
        public static void SetMusic(float value) { MusicVolume = Mathf.Clamp01(value); PlayerPrefs.SetFloat(Prefix + "Music", MusicVolume); }
        public static void SetSfx(float value) { SfxVolume = Mathf.Clamp01(value); PlayerPrefs.SetFloat(Prefix + "Sfx", SfxVolume); }
        public static void Save() => PlayerPrefs.Save();

        public static void ApplyDisplay(int width, int height, bool fullscreen, bool save = true)
        {
            if (width < 1 || height < 1) return;
            // Preserve the user's Editor Game View and layout.
            if (!Application.isEditor)
                Screen.SetResolution(width, height, fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
            if (!save) return;
            PlayerPrefs.SetInt(Prefix + "Width", width);
            PlayerPrefs.SetInt(Prefix + "Height", height);
            PlayerPrefs.SetInt(Prefix + "Fullscreen", fullscreen ? 1 : 0);
            Save();
        }

        public static int DisplayWidth => PlayerPrefs.GetInt(Prefix + "Width", Screen.width);
        public static int DisplayHeight => PlayerPrefs.GetInt(Prefix + "Height", Screen.height);
        public static bool Fullscreen => PlayerPrefs.GetInt(Prefix + "Fullscreen", Screen.fullScreen ? 1 : 0) != 0;
    }
}
