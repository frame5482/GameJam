using UnityEngine;
using UnityEngine.UI;

namespace SpaceGarage
{
    [DefaultExecutionOrder(-100)]
    public sealed class StartMenuUI : MonoBehaviour
    {
        [Header("User-created panels (SetActive)")]
        [SerializeField] GameObject startPanel;
        [SerializeField] GameObject settingsPanel;
        [SerializeField] GameObject mainUI;
        [SerializeField, Tooltip("Accepts a scene UI panel, GameObject, or RepairUI component. The controller may live elsewhere in the scene.")]
        Object repairUI;
        RepairUI repairController;
        [Header("User-created buttons")]
        [SerializeField] Button startButton;
        [SerializeField] Button settingsButton;
        [SerializeField] Button exitButton;
        [SerializeField] Button settingsBackButton;
        static bool enteredMain;
        bool starting;
        bool showingSettings;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetSession() => enteredMain = false;

        void Awake()
        {
            repairController = repairUI as RepairUI;
            var root = repairUI as GameObject;
            if (root == null && repairUI is Component component) root = component.gameObject;
            if (repairController == null && root != null)
                repairController = root.GetComponentInChildren<RepairUI>(true);
            if (repairController == null)
                repairController = FindFirstObjectByType<RepairUI>(FindObjectsInactive.Include);
            ShowPanels();
        }

        void OnEnable()
        {
            if (startButton != null) startButton.onClick.AddListener(StartGame);
            if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
            if (exitButton != null) exitButton.onClick.AddListener(ExitGame);
            if (settingsBackButton != null) settingsBackButton.onClick.AddListener(CloseSettings);
        }

        public void StartGame()
        {
            if (starting || enteredMain) return;
            starting = true;
            var resources = ResourceManager.Instance;
            if (resources == null || !resources.BeginGame())
            {
                Debug.LogWarning("Cannot start: ResourceManager is missing or progress could not be initialized.", this);
                starting = false;
                return;
            }
            enteredMain = true;
            showingSettings = false;
            ShowPanels();
        }

        public void OpenSettings() { showingSettings = true; ShowPanels(); }
        public void CloseSettings() { GarageSettings.Save(); showingSettings = false; ShowPanels(); }
        public void ExitGame()
        {
            GarageSettings.Save();
            if (ResourceManager.Instance != null && ResourceManager.Instance.HasStartedGame)
                ResourceManager.Instance.SaveProgress();
            Application.Quit();
        }

        void ShowPanels()
        {
            bool main = enteredMain && !showingSettings;
            if (repairController != null) repairController.SetMainMenuVisible(main);
            if (mainUI != null) mainUI.SetActive(main);
            if (startPanel != null) startPanel.SetActive(!enteredMain && !showingSettings);
            if (settingsPanel != null) settingsPanel.SetActive(showingSettings);
        }

        void OnDisable()
        {
            if (startButton != null) startButton.onClick.RemoveListener(StartGame);
            if (settingsButton != null) settingsButton.onClick.RemoveListener(OpenSettings);
            if (exitButton != null) exitButton.onClick.RemoveListener(ExitGame);
            if (settingsBackButton != null) settingsBackButton.onClick.RemoveListener(CloseSettings);
        }
    }
}
