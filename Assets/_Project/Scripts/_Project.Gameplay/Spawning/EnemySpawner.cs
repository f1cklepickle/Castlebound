using System.Collections.Generic;
using System.Linq;

namespace Castlebound.Gameplay.Spawning
{
    public class EnemySpawner
    {
        private readonly EnemySpawnSchedule _schedule;
        private readonly List<SpawnPoint> _spawnPoints;
        private SpawnSequence _orderedSequence;
        private List<SpawnPoint> _currentSpawnOrder;
        private int _currentSpawnOrderIndex;

        public EnemySpawner(EnemySpawnSchedule schedule, IEnumerable<SpawnPoint> spawnPoints)
        {
            _schedule = schedule;
            _spawnPoints = spawnPoints?.ToList() ?? new List<SpawnPoint>();
        }

        public List<SpawnRequest> Tick(float deltaTime)
        {
            var readySpawns = new List<SpawnRequest>();

            if (!_schedule.HasMoreSequences || _spawnPoints.Count == 0)
            {
                return readySpawns;
            }

            var current = _schedule.CurrentSequence;
            current.AdvanceTime(deltaTime);

            while (current.IsReadyToSpawn())
            {
                EnsureSpawnOrder(current);
                var spawnPoint = _currentSpawnOrder[_currentSpawnOrderIndex++];

                readySpawns.Add(new SpawnRequest(current.EnemyTypeId, spawnPoint));

                current.ConsumeSpawn();

                if (!current.HasRemaining)
                {
                    _schedule.AdvanceToNextSequence();
                    if (!_schedule.HasMoreSequences)
                    {
                        break;
                    }

                    current = _schedule.CurrentSequence;
                    current.AdvanceTime(0f); // ensure readiness recalculated on new sequence
                }
            }

            return readySpawns;
        }

        private void EnsureSpawnOrder(SpawnSequence sequence)
        {
            if (ReferenceEquals(_orderedSequence, sequence))
            {
                return;
            }

            _orderedSequence = sequence;
            _currentSpawnOrder = SpawnMarkerOrderBuilder.BuildGateOrder(
                _spawnPoints,
                sequence.SpawnCount,
                SpawnMarkerStrategy.RoundRobin);
            _currentSpawnOrderIndex = 0;
        }
    }
}
