using UnityEngine;

namespace SpaceGarage
{
    // One initialization shared by On Start and the Start menu, persisted across launches.
    public sealed class newGame : MonoBehaviour
    {
        private const string CompletedKey = "SpaceGarage.NewGame.Completed";
        [SerializeField, Tooltip("Run New Game once on Start. PlayerPrefs prevents subsequent runs.")]
        private bool onStart;
        [SerializeField, Tooltip("Runtime status loaded from PlayerPrefs; checked after initialization succeeds.")]
        private bool completed;

        private void Awake() => completed = PlayerPrefs.GetInt(CompletedKey, 0) == 1;

        private void Start()
        {
            if (onStart) NewGame();
        }

        public void NewGame()
        {
            TryInitializeOnce();
            completed = PlayerPrefs.GetInt(CompletedKey, 0) == 1;
        }

        public static bool TryInitializeOnce()
        {
            if (PlayerPrefs.GetInt(CompletedKey, 0) == 1) return true;
            var resources = ResourceManager.Instance;
            if (resources == null || !resources.ResetForNewGame())
            {
                Debug.LogWarning("New Game was not completed: ResourceManager is missing or initialization failed.");
                return false;
            }
            // Record only after the new inventory and JSON save have succeeded.
            PlayerPrefs.SetInt(CompletedKey, 1);
            PlayerPrefs.Save();
            foreach (var initializer in FindObjectsByType<newGame>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                initializer.completed = true;
            return true;
        }
    }
}
