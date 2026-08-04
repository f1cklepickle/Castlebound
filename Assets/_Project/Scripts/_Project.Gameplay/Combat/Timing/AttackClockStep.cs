namespace Castlebound.Gameplay.Combat
{
    public readonly struct AttackClockStep
    {
        public bool ImpactOccurred { get; }
        public bool ActiveWindowOccurred { get; }
        public bool SwingCompleted { get; }
        public float UnusedDeltaTime { get; }

        public AttackClockStep(
            bool impactOccurred,
            bool activeWindowOccurred,
            bool swingCompleted,
            float unusedDeltaTime)
        {
            ImpactOccurred = impactOccurred;
            ActiveWindowOccurred = activeWindowOccurred;
            SwingCompleted = swingCompleted;
            UnusedDeltaTime = unusedDeltaTime;
        }
    }
}
