namespace Castlebound.Gameplay.Spawning
{
    [System.Serializable]
    public struct SpawnSequenceConfig
    {
        public string enemyTypeId;
        public int spawnCount;
        public float intervalSeconds;
        public float initialDelaySeconds;
    }
}
