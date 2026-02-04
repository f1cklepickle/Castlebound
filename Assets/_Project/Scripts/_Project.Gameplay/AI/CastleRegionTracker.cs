using System;
using System.Collections.Generic;
using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    /// <summary>
    /// Tracks which actors are inside the castle polygon region.
    /// </summary>
    public class CastleRegionTracker : MonoBehaviour
    {
        public static CastleRegionTracker Instance { get; private set; }
        public static event Action InstanceReady;

        public event Action OnPlayerEntered;
        public event Action OnPlayerExited;
        public event Action<EnemyController2D> OnEnemyEntered;
        public event Action<EnemyController2D> OnEnemyExited;

        private readonly HashSet<EnemyController2D> _enemiesInside = new HashSet<EnemyController2D>();

        public bool PlayerInside { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple CastleRegionTracker instances found. Using the first one.");
            }

            Instance = this;
            InstanceReady?.Invoke();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.CompareTag("Player"))
            {
                PlayerInside = true;
                OnPlayerEntered?.Invoke();
            }

            var enemy = col.GetComponent<EnemyController2D>();
            if (enemy != null)
            {
                _enemiesInside.Add(enemy);
                OnEnemyEntered?.Invoke(enemy);
            }
        }

        private void OnTriggerExit2D(Collider2D col)
        {
            if (col.CompareTag("Player"))
            {
                PlayerInside = false;
                OnPlayerExited?.Invoke();
            }

            var enemy = col.GetComponent<EnemyController2D>();
            if (enemy != null)
            {
                _enemiesInside.Remove(enemy);
                OnEnemyExited?.Invoke(enemy);
            }
        }

        public bool EnemyInside(EnemyController2D enemy)
        {
            return _enemiesInside.Contains(enemy);
        }

#if UNITY_EDITOR
        public void Debug_ForceInstanceForTests()
        {
            Instance = this;
            InstanceReady?.Invoke();
        }
#endif
    }
}
