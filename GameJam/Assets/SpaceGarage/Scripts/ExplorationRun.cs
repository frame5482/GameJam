using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SpaceGarage
{
    [DisallowMultipleComponent]
    public sealed class ExplorationRun : MonoBehaviour
    {
        [Header("Run settings")]
        [SerializeField] private ShipController ship = null;
        [SerializeField, Min(0.1f)] private float duration = 30f;
        [SerializeField] private string returnSceneName = "Repair";
        [Header("Your Canvas panels (SetActive, no generated UI)")]
        [SerializeField] private GameObject gameOverPanel = null;
        [SerializeField] private GameObject successPanel = null;
        [SerializeField] private Button gameOverBackButton = null;
        [SerializeField] private Button successBackButton = null;
        [Header("Your optional text fields")]
        [SerializeField] private TMP_Text timerText = null;
        [SerializeField] private TMP_Text gameOverTitleText = null;
        [SerializeField] private TMP_Text successTitleText = null;
        [SerializeField] private TMP_Text bonusText = null;
        [Header("Your result list containers and existing material row prefab")]
        [SerializeField] private RectTransform gameOverMaterialsContainer = null;
        [SerializeField] private RectTransform successMaterialsContainer = null;
        [SerializeField] private RepairMaterialDragItem resultRowPrefab = null;
        [Header("Your result statistics text fields")]
        [SerializeField] private TMP_Text gameOverKillsText = null;
        [SerializeField] private TMP_Text gameOverCollectedText = null;
        [SerializeField] private TMP_Text successKillsText = null;
        [SerializeField] private TMP_Text successCollectedText = null;
        public long Kills { get; private set; }
        public long Collected => rewards.Collected;
        public float Remaining { get; private set; }
        public bool HasEnded { get; private set; }
        public bool Succeeded { get; private set; }
        private readonly ExplorationRewards rewards = new ExplorationRewards();
        private readonly Dictionary<string, ShipMaterialData> collectedMaterials = new Dictionary<string, ShipMaterialData>();
        private readonly Dictionary<string, int> grantedBonuses = new Dictionary<string, int>();
        private readonly List<RepairMaterialDragItem> resultRows = new List<RepairMaterialDragItem>();
        private ShipInventory inventory;
        private bool started, loadingReturn, ownsPause;
        private float previousTimeScale;

        private void Awake()
        {
            Remaining = Mathf.Max(0.1f, duration);
            SetPanel(gameOverPanel, false);
            SetPanel(successPanel, false);
            if (gameOverTitleText != null) gameOverTitleText.text = "Game Over";
            if (successTitleText != null) successTitleText.text = "Exploration Complete";
            if (bonusText != null) bonusText.text = string.Empty;
            RefreshTimer();
        }
        private void OnEnable()
        {
            if (started && !HasEnded)
            {
                inventory.pickupCollected += RecordPickup;
                Enemy.Defeated += RecordKill;
                if (ship != null) ship.died.AddListener(GameOver);
            }
            if (gameOverBackButton != null) gameOverBackButton.onClick.AddListener(ReturnToRepair);
            if (successBackButton != null && successBackButton != gameOverBackButton)
                successBackButton.onClick.AddListener(ReturnToRepair);
        }
        private void Start()
        {
            if (ship == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) ship = player.GetComponent<ShipController>();
            }
            if (ship == null || ship.Inventory == null)
            {
                Debug.LogWarning("ExplorationRun needs the player ShipController and its inventory.", this);
                return;
            }
            inventory = ship.Inventory;
            inventory.pickupCollected += RecordPickup;
            Enemy.Defeated += RecordKill;
            ship.died.AddListener(GameOver);
            started = true;
            if (!ship.IsAlive) GameOver();
        }
        private void Update()
        {
            if (!started || HasEnded) return;
            if (ship == null || !ship.IsAlive) { GameOver(); return; }
            Remaining = Mathf.Max(0, Remaining - Time.deltaTime);
            RefreshTimer();
            if (Remaining <= 0) Finish(true);
        }
        private void RefreshTimer()
        { if (timerText != null) timerText.text = Mathf.CeilToInt(Remaining).ToString(); }
        private void RecordPickup(ShipMaterialData material, string itemId, int amount)
        {
            if (HasEnded) return;
            rewards.Record(itemId, amount);
            if (material != null && !string.IsNullOrWhiteSpace(itemId)) collectedMaterials[itemId] = material;
        }
        private void RecordKill(Enemy enemy, ShipHitSource source)
        {
            if (started && !HasEnded && enemy != null && enemy.gameObject.scene == gameObject.scene &&
                source == ShipHitSource.Player && Kills < long.MaxValue) Kills++;
        }
        private void ShowResultStatistics(bool success)
        {
            var killsText = success ? successKillsText : gameOverKillsText;
            var collectedText = success ? successCollectedText : gameOverCollectedText;
            if (killsText != null) killsText.text = $"Kills: {Kills:N0}";
            if (collectedText != null) collectedText.text = $"Collected: {Collected:N0}";
        }
        private void GameOver() => Finish(false);
        private void Finish(bool success)
        {
            if (HasEnded || !started) return;
            HasEnded = true;
            Succeeded = success;
            UnsubscribeRun();
            if (success) GrantBonus();
            ShowResultMaterials(success);
            ShowResultStatistics(success);
            if (ResourceManager.Instance != null) ResourceManager.Instance.SaveProgress();
            // Stop gameplay Update methods too: timeScale alone does not prevent firing/overlap checks.
            if (ship != null) ship.enabled = false;
            foreach (var enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
                if (enemy.gameObject.scene == gameObject.scene) enemy.enabled = false;
            foreach (var manager in FindObjectsByType<EnemySpawnManager>(FindObjectsSortMode.None))
                if (manager.gameObject.scene == gameObject.scene) manager.enabled = false;
            foreach (var gun in FindObjectsByType<ShipGun>(FindObjectsSortMode.None))
                if (gun.gameObject.scene == gameObject.scene) gun.enabled = false;
            foreach (var behaviour in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
                if (behaviour is IShipProjectile && behaviour.gameObject.scene == gameObject.scene) behaviour.enabled = false;
            previousTimeScale = Time.timeScale;
            ownsPause = true;
            Time.timeScale = 0;
            SetPanel(gameOverPanel, !success);
            SetPanel(successPanel, success);
            if ((success ? successPanel : gameOverPanel) == null)
                Debug.LogWarning("Assign the result panels and Back buttons on ExplorationRun. The result is ready but its Canvas panel is missing.", this);
        }
        private void GrantBonus()
        {
            Dictionary<string, int> rolled = rewards.Roll(count => Random.Range(0, count));
            var text = new StringBuilder("Bonus Materials");
            int grantedTotal = 0;
            foreach (var entry in rolled)
            {
                int granted = Mathf.Min(entry.Value, int.MaxValue - inventory.Count(entry.Key));
                if (granted <= 0 || !inventory.Add(entry.Key, granted)) continue;
                grantedBonuses[entry.Key] = granted;
                grantedTotal += granted;
                string name = entry.Key;
                if (ResourceManager.Instance != null)
                    foreach (var data in ResourceManager.Instance.MaterialCatalog)
                        if (data != null && data.itemId == entry.Key)
                        { name = string.IsNullOrWhiteSpace(data.displayName) ? data.itemId : data.displayName; break; }
                text.Append('\n').Append(name).Append(" +").Append(granted);
            }
            if (grantedTotal == 0) text.Append("\nNone");
            if (bonusText != null) bonusText.text = text.ToString();
        }
        private ShipMaterialData FindMaterial(string itemId)
        {
            if (collectedMaterials.TryGetValue(itemId, out var material) && material != null) return material;
            if (ResourceManager.Instance != null)
                foreach (var data in ResourceManager.Instance.MaterialCatalog)
                    if (data != null && data.itemId == itemId) return data;
            return null;
        }

        private void ShowResultMaterials(bool success)
        {
            var container = success ? successMaterialsContainer : gameOverMaterialsContainer;
            if (container == null || resultRowPrefab == null)
            {
                Debug.LogWarning("Assign the result Materials Container and Result Row Prefab on ExplorationRun to show collected materials.", this);
                return;
            }
            if (resultRowPrefab.transform == container || container.IsChildOf(resultRowPrefab.transform) ||
                resultRowPrefab.GetComponentInChildren<ExplorationRun>(true) != null)
            {
                Debug.LogWarning("Use a separate material row prefab, not the result container or entire result panel.", this);
                return;
            }
            foreach (var entry in rewards.CollectedItems)
            {
                grantedBonuses.TryGetValue(entry.Key, out int bonus);
                var row = Instantiate(resultRowPrefab, container, false);
                row.name = "Result_" + entry.Key;
                row.DisplayResult(FindMaterial(entry.Key), entry.Key, entry.Value, success ? bonus : 0);
                row.gameObject.SetActive(true);
                resultRows.Add(row);
            }
        }

        public void ReturnToRepair()
        {
            if (!HasEnded || loadingReturn) return;
            if (string.IsNullOrWhiteSpace(returnSceneName) || !Application.CanStreamedLevelBeLoaded(returnSceneName))
            { Debug.LogWarning("Return scene is missing from the Build Profile: " + returnSceneName, this); return; }
            loadingReturn = true;
            if (ResourceManager.Instance != null) ResourceManager.Instance.SaveProgress();
            try
            {
                var operation = SceneManager.LoadSceneAsync(returnSceneName, LoadSceneMode.Single);
                if (operation == null) { loadingReturn = false; return; }
                ownsPause = false;
                Time.timeScale = 1;
            }
            catch (System.Exception error)
            { loadingReturn = false; Debug.LogException(error, this); }
        }
        private void SetPanel(GameObject panel, bool visible)
        {
            if (panel == null) return;
            if (transform.IsChildOf(panel.transform))
            { Debug.LogWarning("Result panels must not contain the ExplorationRun component. Keep it on the always-active MainUI.", this); return; }
            panel.SetActive(visible);
        }
        private void UnsubscribeRun()
        {
            Enemy.Defeated -= RecordKill;
            if (inventory != null) inventory.pickupCollected -= RecordPickup;
            if (ship != null) ship.died.RemoveListener(GameOver);
        }
        private void OnDisable()
        {
            UnsubscribeRun();
            if (gameOverBackButton != null) gameOverBackButton.onClick.RemoveListener(ReturnToRepair);
            if (successBackButton != null && successBackButton != gameOverBackButton)
                successBackButton.onClick.RemoveListener(ReturnToRepair);
        }
        private void OnDestroy()
        {
            UnsubscribeRun();
            foreach (var row in resultRows)
                if (row != null) { row.gameObject.SetActive(false); Destroy(row.gameObject); }
            if (ownsPause) Time.timeScale = previousTimeScale;
        }
    }
}
