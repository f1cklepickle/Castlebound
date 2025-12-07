using System.Collections.Generic;
using System.Linq;

namespace Castlebound.Gameplay.Spawning
{
    public class EnemySpawnSchedule
    {
        private readonly Queue<SpawnSequence> _sequences;

        public EnemySpawnSchedule(IEnumerable<SpawnSequence> sequences)
        {
            _sequences = new Queue<SpawnSequence>(sequences ?? Enumerable.Empty<SpawnSequence>());
        }

        public SpawnSequence CurrentSequence => _sequences.Count > 0 ? _sequences.Peek() : null;

        public bool HasMoreSequences => _sequences.Count > 0;

        public void AdvanceToNextSequence()
        {
            if (_sequences.Count > 0)
            {
                _sequences.Dequeue();
            }
        }
    }
}
