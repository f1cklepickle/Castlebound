using System.Collections.Generic;
using UnityEngine;

namespace Castlebound.Gameplay.Spawning
{
    public class EnemySpawnerRunner : MonoBehaviour
    {
        [System.Serializable]
        private struct EnemyPrefabMapping
        {
            public string enemyTypeId;
            public GameObject prefab;
        }

        [SerializeField] private EnemySpawnScheduleAsset scheduleAsset;
        [SerializeField] private List<SpawnPointMarker> spawnMarkers = new List<SpawnPointMarker>();
        [SerializeField] private List<EnemyPrefabMapping> prefabMappings = new List<EnemyPrefabMapping>();

        private EnemySpawner _spawner;
        private Dictionary<string, GameObject> _prefabMap;

        private void Start()
        {
            BuildPrefabMap();

            if (scheduleAsset == null)
            {
                Debug.LogWarning("EnemySpawnerRunner: no schedule asset assigned.");
                enabled = false;
                return;
            }

            var spawnPoints = new List<SpawnPoint>(spawnMarkers.Count);
            foreach (var marker in spawnMarkers)
            {
                if (marker != null)
                {
                    spawnPoints.Add(marker.ToSpawnPoint());
                }
            }

            _spawner = new EnemySpawner(scheduleAsset.ToRuntimeSchedule(), spawnPoints);
        }

        private void Update()
        {
            if (_spawner == null)
            {
                return;
            }

            var ready = _spawner.Tick(Time.deltaTime);
            foreach (var request in ready)
            {
                if (!_prefabMap.TryGetValue(request.EnemyTypeId, out var prefab) || prefab == null)
                {
                    Debug.LogWarning($"EnemySpawnerRunner: no prefab for enemy type '{request.EnemyTypeId}'.");
                    continue;
                }

                Instantiate(prefab, request.Position, Quaternion.identity);
            }
        }

        private void BuildPrefabMap()
        {
            _prefabMap = new Dictionary<string, GameObject>();
            foreach (var mapping in prefabMappings)
            {
                if (string.IsNullOrWhiteSpace(mapping.enemyTypeId) || mapping.prefab == null)
                {
                    continue;
                }

                _prefabMap[mapping.enemyTypeId] = mapping.prefab;
            }
        }
    }
}
