using Castlebound.Gameplay.Spawning;

namespace Castlebound.Gameplay.Castle
{
    public static class VaultInteractionPolicy
    {
        public static bool CanOpen(bool playerInRange, WavePhase phase)
        {
            return playerInRange && phase == WavePhase.PreWave;
        }

        public static VaultInteractionVisualState GetVisualState(bool playerInRange, WavePhase phase)
        {
            if (!playerInRange)
            {
                return VaultInteractionVisualState.Hidden;
            }

            return phase == WavePhase.PreWave
                ? VaultInteractionVisualState.Accessible
                : VaultInteractionVisualState.Blocked;
        }
    }
}
