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
        private EnemyWaveSpawner _waveSpawner;
        private int _aliveCount;
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

            var waveSchedule = scheduleAsset.ToRuntimeWaveSchedule();
            var hasAuthoredWaves = scheduleAsset != null && waveSchedule != null && waveSchedule.HasAuthoredWaves;

            if (hasAuthoredWaves)
            {
                _waveSpawner = new EnemyWaveSpawner(waveSchedule, spawnPoints);
            }
            else
            {
                _spawner = new EnemySpawner(scheduleAsset.ToRuntimeSchedule(), spawnPoints);
            }
        }

        private void Update()
        {
            if (_waveSpawner != null)
            {
                var ready = _waveSpawner.Tick(Time.deltaTime, _aliveCount);
                SpawnReady(ready);
            }
            else if (_spawner != null)
            {
                var ready = _spawner.Tick(Time.deltaTime);
                SpawnReady(ready);
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

        private void SpawnReady(List<SpawnRequest> ready)
        {
            foreach (var request in ready)
            {
                if (!_prefabMap.TryGetValue(request.EnemyTypeId, out var prefab) || prefab == null)
                {
                    Debug.LogWarning($"EnemySpawnerRunner: no prefab for enemy type '{request.EnemyTypeId}'.");
                    continue;
                }

                var instance = Instantiate(prefab, request.Position, Quaternion.identity);
                _aliveCount++;
                // TODO: hook enemy death/despawn to decrement _aliveCount.
            }
        }
    }
}
