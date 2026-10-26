using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceGarage
{
    public sealed class newGame : MonoBehaviour
    {
        [SerializeField, Tooltip("Start a new game automatically once per scene per play session.")]
        private bool onStart;
        private static readonly HashSet<string> autoStartedScenes = new HashSet<string>();
        private bool restarting;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetAutoStartState() => autoStartedScenes.Clear();

        private void Start()
        {
            if (onStart && autoStartedScenes.Add(SceneManager.GetActiveScene().path)) NewGame();
        }

        public void NewGame()
        {
            if (restarting) return;
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || string.IsNullOrEmpty(scene.path) ||
                !Application.CanStreamedLevelBeLoaded(scene.path))
            {
                Debug.LogWarning("Cannot start a new game: add the current scene to the active Build Profile Scene List.", this);
                return;
            }
            var resources = ResourceManager.Instance;
            if (resources == null)
            {
                Debug.LogWarning("Cannot start a new game: ResourceManager is missing.", this);
                return;
            }
            restarting = true;
            if (!resources.ResetForNewGame()) { restarting = false; return; }
            Time.timeScale = 1;
            try { SceneManager.LoadScene(scene.path, LoadSceneMode.Single); }
            catch (System.Exception error)
            {
                restarting = false;
                Debug.LogException(error, this);
            }
        }
    }
}
