using UnityEngine;

namespace SpaceGarage
{
    // Preserve existing UnityEvent references; never reset progress from a Start button.
    public sealed class newGame : MonoBehaviour
    {
        public void NewGame()
        {
            if (ResourceManager.Instance != null) ResourceManager.Instance.BeginGame();
        }
    }
}