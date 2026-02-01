using System;
using System.Collections.Generic;

namespace Castlebound.Gameplay.Castle
{
    public class BarrierBreakPressureTrigger
    {
        private readonly int breaksPerWave;
        private readonly Dictionary<string, BarrierState> barrierStates = new Dictionary<string, BarrierState>();

        public BarrierBreakPressureTrigger(int breaksPerWave)
        {
            this.breaksPerWave = Math.Max(1, breaksPerWave);
        }

        public bool RegisterBreak(string barrierId, int waveIndex)
        {
            if (string.IsNullOrWhiteSpace(barrierId))
            {
                return false;
            }

            if (!barrierStates.TryGetValue(barrierId, out var state))
            {
                state = new BarrierState { waveIndex = waveIndex };
            }
            else if (state.waveIndex != waveIndex)
            {
                state.waveIndex = waveIndex;
                state.breakCount = 0;
                state.triggered = false;
            }

            state.breakCount++;

            bool shouldTrigger = !state.triggered && state.breakCount >= breaksPerWave;
            if (shouldTrigger)
            {
                state.triggered = true;
            }

            barrierStates[barrierId] = state;
            return shouldTrigger;
        }

        private struct BarrierState
        {
            public int waveIndex;
            public int breakCount;
            public bool triggered;
        }
    }
}
