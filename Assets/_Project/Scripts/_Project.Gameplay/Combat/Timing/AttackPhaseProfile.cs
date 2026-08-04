namespace Castlebound.Gameplay.Combat
{
    public readonly struct AttackPhaseProfile
    {
        public float WindupWeight { get; }
        public float ActiveWeight { get; }
        public float RecoveryWeight { get; }

        public AttackPhaseProfile(float windupWeight, float activeWeight, float recoveryWeight)
        {
            WindupWeight = NormalizeWeight(windupWeight);
            ActiveWeight = NormalizeWeight(activeWeight);
            RecoveryWeight = NormalizeWeight(recoveryWeight);
        }

        private static float NormalizeWeight(float value)
        {
            return float.IsNaN(value) || float.IsInfinity(value) || value < 0f ? 0f : value;
        }
    }
}
