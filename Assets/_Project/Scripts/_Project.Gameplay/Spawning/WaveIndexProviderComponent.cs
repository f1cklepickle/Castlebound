using UnityEngine;

namespace Castlebound.Gameplay.Spawning
{
    public class WaveIndexProviderComponent : MonoBehaviour, IWaveIndexProvider
    {
        public int CurrentWaveIndex { get; set; } = 1;
    }
}
