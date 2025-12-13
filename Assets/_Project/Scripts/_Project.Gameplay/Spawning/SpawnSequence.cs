namespace Castlebound.Gameplay.Spawning
{
    public class SpawnSequence
    {
        public string EnemyTypeId { get; }
        public int SpawnCount { get; }
        public float IntervalSeconds { get; }
        public float InitialDelaySeconds { get; }

        private int _remaining;
        private float _timeUntilNext;

        public SpawnSequence(string enemyTypeId, int spawnCount, float intervalSeconds, float initialDelaySeconds)
        {
            EnemyTypeId = enemyTypeId;
            SpawnCount = spawnCount;
            IntervalSeconds = intervalSeconds;
            InitialDelaySeconds = initialDelaySeconds;

            _remaining = spawnCount;
            _timeUntilNext = initialDelaySeconds;
        }

        public bool HasRemaining => _remaining > 0;

        public void AdvanceTime(float deltaTime)
        {
            if (_remaining <= 0)
            {
                return;
            }

            _timeUntilNext -= deltaTime;
        }

        public bool IsReadyToSpawn() => _remaining > 0 && _timeUntilNext < 0f;

        public void ConsumeSpawn()
        {
            if (_remaining <= 0)
            {
                return;
            }

            _remaining--;
            if (_remaining > 0)
            {
                _timeUntilNext += IntervalSeconds;
            }
        }
    }
}
