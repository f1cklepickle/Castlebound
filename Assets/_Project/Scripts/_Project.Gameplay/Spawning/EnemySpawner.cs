using System.Collections.Generic;
using System.Linq;

namespace Castlebound.Gameplay.Spawning
{
    public class EnemySpawner
    {
        private readonly EnemySpawnSchedule _schedule;
        private readonly List<SpawnPoint> _spawnPoints;
        private int _gateIndex;

        public EnemySpawner(EnemySpawnSchedule schedule, IEnumerable<SpawnPoint> spawnPoints)
        {
            _schedule = schedule;
            _spawnPoints = spawnPoints?.ToList() ?? new List<SpawnPoint>();
            _gateIndex = 0;
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
                var spawnPoint = _spawnPoints[_gateIndex];
                _gateIndex = (_gateIndex + 1) % _spawnPoints.Count;

                readySpawns.Add(new SpawnRequest(current.EnemyTypeId, spawnPoint.GateId, spawnPoint.Position));

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
    }
}
