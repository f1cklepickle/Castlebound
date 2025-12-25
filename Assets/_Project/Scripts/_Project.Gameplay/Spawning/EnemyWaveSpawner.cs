using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Castlebound.Gameplay.Spawning
{
    public class EnemyWaveSpawner
    {
        private readonly WaveScheduleRuntime _waveSchedule;
        private readonly List<SpawnPoint> _spawnPoints;

        private int _currentWaveIndex = 1;
        private WaveRuntime _currentWave;
        private List<SpawnRequest> _pendingWaveRequests;
        private int _nextSpawnRequestIndex;

        private float _gapTimer;
        private bool _waitingForClear;

        public EnemyWaveSpawner(WaveScheduleRuntime waveSchedule, IEnumerable<SpawnPoint> spawnPoints)
        {
            _waveSchedule = waveSchedule;
            _spawnPoints = spawnPoints?.ToList() ?? new List<SpawnPoint>();
            PrepareWave(_currentWaveIndex);
        }

        public bool HasMoreWaves => _currentWave != null;

        public List<SpawnRequest> Tick(float deltaTime, int currentAlive)
        {
            var ready = new List<SpawnRequest>();

            if (_currentWave == null || _spawnPoints.Count == 0)
            {
                return ready;
            }

            // If maxAlive is set, do not emit spawns when at/above the cap.
            if (_currentWave.MaxAlive > 0 && currentAlive >= _currentWave.MaxAlive)
            {
                return ready;
            }

            // If we are in a wait-for-clear phase before advancing to next wave.
            if (_waitingForClear)
            {
                if (currentAlive > 0)
                {
                    return ready;
                }

                _gapTimer -= deltaTime;
                if (_gapTimer > 0f)
                {
                    return ready;
                }

                AdvanceToNextWave();
                if (_currentWave == null)
                {
                    return ready;
                }
            }

            if (_pendingWaveRequests == null || _nextSpawnRequestIndex >= _pendingWaveRequests.Count)
            {
                // No more spawns in this wave; enter wait-for-clear/gap if needed.
                EnterPostWaveWait();
                return ready;
            }

            // Emit next spawn immediately; per-sequence timing is preserved by precomputed requests.
            ready.Add(_pendingWaveRequests[_nextSpawnRequestIndex]);
            _nextSpawnRequestIndex++;

            return ready;
        }

        private void PrepareWave(int waveIndex)
        {
            _currentWave = _waveSchedule?.GetWave(waveIndex);
            _nextSpawnRequestIndex = 0;
            _pendingWaveRequests = null;
            _gapTimer = 0f;
            _waitingForClear = false;

            if (_currentWave == null || _spawnPoints.Count == 0)
            {
                return;
            }

            var requests = new List<SpawnRequest>();

            foreach (var seq in _currentWave.Sequences)
            {
                var gateOrder = SpawnMarkerOrderBuilder.BuildGateOrder(_spawnPoints, seq.spawnCount, _currentWave.Strategy, _currentWave.Seed);
                foreach (var gate in gateOrder)
                {
                    requests.Add(new SpawnRequest(seq.enemyTypeId, gate.GateId, gate.Position));
                }
            }

            _pendingWaveRequests = requests;
        }

        private void EnterPostWaveWait()
        {
            if (_currentWave == null)
            {
                return;
            }

            if (_currentWave.WaitForClear)
            {
                _waitingForClear = true;
                _gapTimer = _currentWave.GapSeconds;
                return;
            }

            _gapTimer = _currentWave.GapSeconds;
            AdvanceToNextWaveAfterGap();
        }

        private void AdvanceToNextWaveAfterGap()
        {
            _gapTimer -= Time.deltaTime;
            if (_gapTimer > 0f)
            {
                return;
            }

            AdvanceToNextWave();
        }

        private void AdvanceToNextWave()
        {
            _currentWaveIndex++;
            PrepareWave(_currentWaveIndex);
        }
    }
}
