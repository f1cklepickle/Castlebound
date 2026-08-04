namespace Castlebound.Gameplay.Combat
{
    public readonly struct AttackSwingTiming
    {
        public float AttackRate { get; }
        public float Duration { get; }
        public float WindupDuration { get; }
        public float ActiveDuration { get; }
        public float RecoveryDuration { get; }

        public AttackSwingTiming(float attackRate, AttackPhaseProfile phaseProfile)
        {
            AttackRate = AttackRatePolicy.Normalize(attackRate);
            Duration = 1f / AttackRate;

            float totalWeight = phaseProfile.WindupWeight
                + phaseProfile.ActiveWeight
                + phaseProfile.RecoveryWeight;
            if (totalWeight <= 0f)
            {
                WindupDuration = 0f;
                ActiveDuration = 0f;
                RecoveryDuration = Duration;
                return;
            }

            WindupDuration = Duration * (phaseProfile.WindupWeight / totalWeight);
            ActiveDuration = Duration * (phaseProfile.ActiveWeight / totalWeight);
            RecoveryDuration = Duration - WindupDuration - ActiveDuration;
        }
    }
}
