using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Castlebound.Gameplay.Spawning
{
    public class EnemyWaveSpawner
    {
        public event Action<int> OnWaveStarted;
        public event Action OnWaveEnded;

        private readonly WaveScheduleRuntime _waveSchedule;
        private readonly List<SpawnPoint> _spawnPoints;

        private int _currentWaveIndex = 1;
        private WaveRuntime _currentWave;
        private List<SpawnRequest> _pendingWaveRequests;
        private int _nextSpawnRequestIndex;

        // Timing per sequence
        private readonly List<SequenceTimer> _sequenceTimers = new List<SequenceTimer>();

        private float _gapTimer;
        private bool _waitingForClear;
        private bool _requireClear;
        private bool _waveEndAnnounced;

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
                if (_requireClear && currentAlive > 0)
                {
                    return ready;
                }

                _gapTimer -= deltaTime;
                if (_gapTimer > 0f)
                {
                    return ready;
                }

                if (!_waveEndAnnounced)
                {
                    _waveEndAnnounced = true;
                    OnWaveEnded?.Invoke();
                }

                return ready;
            }

            if (_pendingWaveRequests == null || _nextSpawnRequestIndex >= _pendingWaveRequests.Count)
            {
                // No more spawns in this wave; enter wait-for-clear/gap if needed.
                EnterPostWaveWait();
                return ready;
            }

            // Advance timers and emit ready spawns in order.
            UpdateSequenceTimers(deltaTime);
            EmitReadySpawns(ready);

            if (_nextSpawnRequestIndex >= _pendingWaveRequests.Count)
            {
                EnterPostWaveWait();
            }

            return ready;
        }

        private void PrepareWave(int waveIndex)
        {
            _currentWave = _waveSchedule?.GetWave(waveIndex);
            _nextSpawnRequestIndex = 0;
            _pendingWaveRequests = null;
            _gapTimer = 0f;
            _waitingForClear = false;
            _sequenceTimers.Clear();

            if (_currentWave == null || _spawnPoints.Count == 0)
            {
                return;
            }

            var requests = new List<SpawnRequest>();

            foreach (var seq in _currentWave.Sequences)
            {
                var gateOrder = SpawnMarkerOrderBuilder.BuildGateOrder(_spawnPoints, seq.spawnCount, _currentWave.Strategy, _currentWave.Seed);
                var startsAt = requests.Count;
                foreach (var gate in gateOrder)
                {
                    requests.Add(new SpawnRequest(seq.enemyTypeId, gate.GateId, gate.Position));
                }

                _sequenceTimers.Add(new SequenceTimer
                {
                    NextIndex = startsAt,
                    EndIndexExclusive = requests.Count,
                    TimeUntilNext = seq.initialDelaySeconds,
                    Interval = seq.intervalSeconds
                });
            }

            _pendingWaveRequests = requests;

            OnWaveStarted?.Invoke(_currentWaveIndex);
        }

        private void EnterPostWaveWait()
        {
            if (_currentWave == null)
            {
                return;
            }

            _waitingForClear = true;
            _requireClear = _currentWave.WaitForClear;
            _gapTimer = _currentWave.GapSeconds;
            _waveEndAnnounced = false;
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

        public void AdvanceFromPreWave()
        {
            if (!_waitingForClear || !_waveEndAnnounced || _currentWave == null)
            {
                return;
            }

            _waitingForClear = false;
            _waveEndAnnounced = false;
            AdvanceToNextWave();
        }

        private void UpdateSequenceTimers(float deltaTime)
        {
            for (int i = 0; i < _sequenceTimers.Count; i++)
            {
                if (_sequenceTimers[i].IsComplete)
                {
                    continue;
                }

                _sequenceTimers[i] = _sequenceTimers[i].Advance(deltaTime);
            }
        }

        private void EmitReadySpawns(List<SpawnRequest> ready)
        {
            for (int i = 0; i < _sequenceTimers.Count; i++)
            {
                var timer = _sequenceTimers[i];
                while (!timer.IsComplete && timer.TimeUntilNext <= 0f && _nextSpawnRequestIndex < _pendingWaveRequests.Count)
                {
                    ready.Add(_pendingWaveRequests[_nextSpawnRequestIndex]);
                    _nextSpawnRequestIndex++;
                    timer = timer.Consume();
                }

                _sequenceTimers[i] = timer;
            }
        }

        private struct SequenceTimer
        {
            public int NextIndex;
            public int EndIndexExclusive;
            public float TimeUntilNext;
            public float Interval;

            public bool IsComplete => NextIndex >= EndIndexExclusive;

            public SequenceTimer Advance(float deltaTime)
            {
                if (IsComplete)
                {
                    return this;
                }

                TimeUntilNext -= deltaTime;
                return this;
            }

            public SequenceTimer Consume()
            {
                NextIndex++;
                if (IsComplete)
                {
                    return this;
                }

                TimeUntilNext += Interval;
                return this;
            }
        }
    }
}
