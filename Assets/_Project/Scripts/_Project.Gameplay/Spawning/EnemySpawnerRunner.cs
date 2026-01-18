using System;
using System.Collections.Generic;
using UnityEngine;

namespace Castlebound.Gameplay.Spawning
{
    public class EnemySpawnerRunner : MonoBehaviour
    {
        public event Action<int> OnWaveStarted;
        public event Action OnWaveEnded;

        [System.Serializable]
        private struct EnemyPrefabMapping
        {
            public string enemyTypeId;
            public GameObject prefab;
        }

        [SerializeField] private EnemySpawnScheduleAsset scheduleAsset;
        [SerializeField] private List<SpawnPointMarker> spawnMarkers = new List<SpawnPointMarker>();
        [SerializeField] private List<EnemyPrefabMapping> prefabMappings = new List<EnemyPrefabMapping>();
        [SerializeField] private bool autoFindMarkers = true;

        private EnemySpawner _spawner;
        private EnemyWaveSpawner _waveSpawner;
        private int _aliveCount;
        private Dictionary<string, GameObject> _prefabMap;
        private WavePhaseTracker phaseTracker;

        public WavePhaseTracker PhaseTracker => phaseTracker ??= new WavePhaseTracker();

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
            var markersToUse = spawnMarkers;
            if ((markersToUse == null || markersToUse.Count == 0) && autoFindMarkers)
            {
                markersToUse = new List<SpawnPointMarker>(FindObjectsOfType<SpawnPointMarker>());
            }

            foreach (var marker in markersToUse)
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
                _waveSpawner.OnWaveStarted += HandleWaveStarted;
                _waveSpawner.OnWaveEnded += HandleWaveEnded;
            }
            else
            {
                _spawner = new EnemySpawner(scheduleAsset.ToRuntimeSchedule(), spawnPoints);
            }

            phaseTracker = phaseTracker ?? new WavePhaseTracker();
            phaseTracker.SetPhase(WavePhase.InWave);
        }

        private void OnDestroy()
        {
            if (_waveSpawner != null)
            {
                _waveSpawner.OnWaveStarted -= HandleWaveStarted;
                _waveSpawner.OnWaveEnded -= HandleWaveEnded;
            }
        }

        private void Update()
        {
            if (phaseTracker != null && phaseTracker.CurrentPhase != WavePhase.InWave)
            {
                return;
            }

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

        private void HandleWaveStarted(int waveIndex)
        {
            OnWaveStarted?.Invoke(waveIndex);
        }

        private void HandleWaveEnded()
        {
            OnWaveEnded?.Invoke();
            phaseTracker.SetPhase(WavePhase.PreWave);
        }

        public void StartNextWaveFromMenu()
        {
            if (_waveSpawner == null)
            {
                return;
            }

            _waveSpawner.AdvanceFromPreWave();
            phaseTracker.SetPhase(WavePhase.InWave);
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

                var lifetime = instance.GetComponent<SpawnedEntityLifetime>();
                if (lifetime == null)
                {
                    lifetime = instance.AddComponent<SpawnedEntityLifetime>();
                }
                lifetime.Initialize(() =>
                {
                    _aliveCount = Mathf.Max(0, _aliveCount - 1);
                });
            }
        }
    }
}
