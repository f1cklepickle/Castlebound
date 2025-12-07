using System.Collections.Generic;
using System.Linq;

namespace Castlebound.Gameplay.Spawning
{
    public interface ISpawnPointProvider
    {
        IReadOnlyList<SpawnPoint> GetAll();
    }

    public class InMemorySpawnPointProvider : ISpawnPointProvider
    {
        private readonly List<SpawnPoint> _spawnPoints;

        public InMemorySpawnPointProvider(IEnumerable<SpawnPoint> spawnPoints)
        {
            _spawnPoints = spawnPoints?.ToList() ?? new List<SpawnPoint>();
        }

        public IReadOnlyList<SpawnPoint> GetAll() => _spawnPoints;
    }
}
