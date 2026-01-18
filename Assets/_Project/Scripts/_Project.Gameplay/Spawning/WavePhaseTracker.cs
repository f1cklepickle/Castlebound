using System;

namespace Castlebound.Gameplay.Spawning
{
    public class WavePhaseTracker
    {
        public event Action<WavePhase> PhaseChanged;

        public WavePhase CurrentPhase { get; private set; } = WavePhase.PreWave;

        public void SetPhase(WavePhase phase)
        {
            if (CurrentPhase == phase)
            {
                return;
            }

            CurrentPhase = phase;
            PhaseChanged?.Invoke(CurrentPhase);
        }
    }
}
