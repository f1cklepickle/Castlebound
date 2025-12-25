using System;
using UnityEngine;

namespace Castlebound.Gameplay.Spawning
{
    public class SpawnedEntityLifetime : MonoBehaviour
    {
        private Action _onDespawn;
        private bool _notified;

        public void Initialize(Action onDespawn)
        {
            _onDespawn = onDespawn;
        }

        private void OnDisable()
        {
            NotifyOnce();
        }

        private void OnDestroy()
        {
            NotifyOnce();
        }

        private void NotifyOnce()
        {
            if (_notified)
            {
                return;
            }

            _notified = true;
            _onDespawn?.Invoke();
        }

#if UNITY_EDITOR
        // Test-only hook to trigger the despawn notification deterministically in EditMode.
        public void NotifyForTests()
        {
            NotifyOnce();
        }
#endif
    }
}
