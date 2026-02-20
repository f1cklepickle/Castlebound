using UnityEngine;

namespace Castlebound.Gameplay.Castle
{
    public class BarrierAssemblyRunner
    {
        private readonly BarrierTilemapLayoutSource source;
        private readonly GameObject barrierPrefab;
        private readonly Transform generatedParent;

        public BarrierAssemblyRunner(BarrierTilemapLayoutSource source, GameObject barrierPrefab, Transform generatedParent)
        {
            this.source = source;
            this.barrierPrefab = barrierPrefab;
            this.generatedParent = generatedParent;
        }

        public void RebuildNow()
        {
            if (source == null || barrierPrefab == null || generatedParent == null)
            {
                return;
            }

            BarrierAssemblyBuilder.Rebuild(generatedParent, barrierPrefab, source.GetSlots());
        }
    }
}
