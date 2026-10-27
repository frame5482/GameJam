using System.Collections.Generic;
using UnityEngine;

namespace SpaceGarage
{
    public sealed class EnemySpawnManager : MonoBehaviour
    {
        [SerializeField] private Enemy enemyPrefab = null;
        [Header("Random part pools (ShipPartStats assets)")]
        [SerializeField] private List<ShipPartStats> gunPool = new List<ShipPartStats>();
        [SerializeField] private List<ShipPartStats> armorPlatePool = new List<ShipPartStats>();
        [SerializeField] private List<ShipPartStats> wingPool = new List<ShipPartStats>();
        [SerializeField] private List<ShipPartStats> jetPool = new List<ShipPartStats>();
        [Header("Spawn around Tag Player")]
        [SerializeField, Min(0)] private float minimumSpawnDistance = 40f;
        [SerializeField, Min(0)] private float maximumSpawnDistance = 60f;
        [SerializeField, Min(0.01f)] private float spawnInterval = 5f;
        [SerializeField, Min(1)] private int maxAlive = 3;
        [SerializeField] private bool spawnImmediately = true;
        [SerializeField, Min(0)] private float spawnClearance = 3f;
        [SerializeField] private LayerMask spawnBlockingLayers = ~0;
        [SerializeField, Min(1)] private int spawnAttempts = 12;
        private readonly List<Enemy> spawned = new List<Enemy>();
        private Transform player;
        private float nextSpawnAt, nextPlayerSearch;
        private bool warned;
        public int AliveCount { get { Prune(); return spawned.Count; } }

        private void OnEnable() => nextSpawnAt = Time.time + (spawnImmediately ? 0 : Mathf.Max(0.01f, spawnInterval));
        private void OnValidate() { warned = false; }
        private void Update()
        {
            if (Time.time < nextSpawnAt) return;
            nextSpawnAt = Time.time + Mathf.Max(0.01f, spawnInterval);
            SpawnOne();
        }

        public Enemy SpawnOne()
        {
            Prune();
            if (spawned.Count >= Mathf.Max(1, maxAlive)) return null;
            if ((player == null || !player.gameObject.activeInHierarchy) && Time.time >= nextPlayerSearch)
            {
                var found = GameObject.FindGameObjectWithTag("Player");
                player = found != null ? found.transform : null;
                nextPlayerSearch = Time.time + 0.5f;
            }
            if (player == null || !player.gameObject.activeInHierarchy) return null;
            var playerShip = player.GetComponent<ShipController>();
            if (playerShip != null && !playerShip.IsAlive) return null;
            if (enemyPrefab == null || enemyPrefab.GetComponent<ShipController>() != null ||
                !Pick(gunPool, ShipPartSlot.Gun, out var gun) || !Pick(armorPlatePool, ShipPartSlot.ArmorPlate, out var armor) ||
                !Pick(wingPool, ShipPartSlot.Wing, out var wing) || !Pick(jetPool, ShipPartSlot.Jet, out var jet))
            {
                if (!warned) Debug.LogWarning("EnemySpawnManager needs an Enemy-only prefab and at least one valid asset with a part prefab in each matching pool.", this);
                warned = true;
                return null;
            }
            float min = Mathf.Max(0, minimumSpawnDistance), max = Mathf.Max(min, maximumSpawnDistance);
            for (int attempt = 0; attempt < Mathf.Max(1, spawnAttempts); attempt++)
            {
                Vector3 position = player.position + Random.onUnitSphere * Random.Range(min, max);
                if (spawnClearance > 0 && Physics.CheckSphere(position, spawnClearance, spawnBlockingLayers, QueryTriggerInteraction.Ignore)) continue;
                Vector3 direction = player.position - position;
                var rotation = direction.sqrMagnitude > 0.001f ? Quaternion.LookRotation(direction.normalized, Vector3.up) : Quaternion.identity;
                var enemy = Instantiate(enemyPrefab, position, rotation);
                if (!enemy.Initialize(gun, armor, wing, jet, player)) { Destroy(enemy.gameObject); return null; }
                enemy.gameObject.SetActive(true);
                spawned.Add(enemy);
                warned = false;
                return enemy;
            }
            return null;
        }

        private static bool Pick(List<ShipPartStats> pool, ShipPartSlot slot, out ShipPartStats chosen)
        {
            chosen = null;
            int count = 0;
            if (pool == null) return false;
            foreach (var part in pool)
                if (Enemy.Valid(part, slot) && Random.Range(0, ++count) == 0) chosen = part;
            return chosen != null;
        }

        private void Prune() => spawned.RemoveAll(enemy => enemy == null || !enemy.IsAlive);
    }
}
