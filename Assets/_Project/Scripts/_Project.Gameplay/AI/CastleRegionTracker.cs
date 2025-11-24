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

        private readonly HashSet<EnemyController2D> _enemiesInside = new HashSet<EnemyController2D>();

        public bool PlayerInside { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple CastleRegionTracker instances found. Using the first one.");
                return;
            }

            Instance = this;
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.CompareTag("Player"))
                PlayerInside = true;

            var enemy = col.GetComponent<EnemyController2D>();
            if (enemy != null)
                _enemiesInside.Add(enemy);
        }

        private void OnTriggerExit2D(Collider2D col)
        {
            if (col.CompareTag("Player"))
                PlayerInside = false;

            var enemy = col.GetComponent<EnemyController2D>();
            if (enemy != null)
                _enemiesInside.Remove(enemy);
        }

        public bool EnemyInside(EnemyController2D enemy)
        {
            return _enemiesInside.Contains(enemy);
        }
    }
}
