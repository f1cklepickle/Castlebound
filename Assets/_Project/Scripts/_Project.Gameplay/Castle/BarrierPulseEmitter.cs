using System.Collections.Generic;
using Castlebound.Gameplay.AI;
using UnityEngine;

namespace Castlebound.Gameplay.Castle
{
    [RequireComponent(typeof(BarrierPressureTracker))]
    public class BarrierPulseEmitter : MonoBehaviour
    {
        [SerializeField] private Transform pulseOrigin;
        [SerializeField] private float pulseDuration = 1.5f;
        [SerializeField] private float pulseRadius = 15f;
        [SerializeField] private float pulseStrength = 5f;

        public bool IsPulseActive => isPulseActive;

        BarrierPressureTracker pressureTracker;
        float elapsed;
        float previousRadius;
        bool isPulseActive;
        readonly HashSet<EnemyController2D> pushedEnemies = new HashSet<EnemyController2D>();

        void Awake()
        {
            pressureTracker = GetComponent<BarrierPressureTracker>();
            if (pulseOrigin == null)
            {
                var originTransform = transform.Find("PulseOrigin");
                pulseOrigin = originTransform != null ? originTransform : transform;
            }
        }

        void OnEnable()
        {
            if (pressureTracker != null)
            {
                pressureTracker.OnPressureTriggered += HandlePressureTriggered;
            }
        }

        void OnDisable()
        {
            if (pressureTracker != null)
            {
                pressureTracker.OnPressureTriggered -= HandlePressureTriggered;
            }
        }

        void Update()
        {
            if (!isPulseActive)
            {
                return;
            }

            TickPulse(Time.deltaTime);
        }

        void HandlePressureTriggered(string _)
        {
            StartPulse();
        }

        void StartPulse()
        {
            if (isPulseActive)
            {
                return;
            }

            isPulseActive = true;
            elapsed = 0f;
            previousRadius = 0f;
            pushedEnemies.Clear();
        }

        void TickPulse(float dt)
        {
            if (!isPulseActive)
            {
                return;
            }

            elapsed += dt;
            float t = pulseDuration > 0f ? Mathf.Clamp01(elapsed / pulseDuration) : 1f;
            float currentRadius = pulseRadius * t;

            ApplyWavefront(previousRadius, currentRadius);
            previousRadius = currentRadius;

            if (elapsed >= pulseDuration)
            {
                isPulseActive = false;
            }
        }

        void ApplyWavefront(float minRadius, float maxRadius)
        {
            if (pulseOrigin == null)
            {
                return;
            }

            var allEnemies = EnemyController2D.All;
            EnemyController2D[] fallback = null;
            if (allEnemies == null || allEnemies.Count == 0)
            {
                fallback = Object.FindObjectsOfType<EnemyController2D>();
                if (fallback == null || fallback.Length == 0)
                {
                    return;
                }
            }

            Vector2 origin = pulseOrigin.position;

            int count = fallback != null ? fallback.Length : allEnemies.Count;
            for (int i = 0; i < count; i++)
            {
                var enemy = fallback != null ? fallback[i] : allEnemies[i];
                if (enemy == null || pushedEnemies.Contains(enemy))
                {
                    continue;
                }

                if (IsEnemyInside(enemy))
                {
                    continue;
                }

                var knockback = enemy.GetComponent<EnemyKnockbackReceiver>();
                if (knockback == null)
                {
                    continue;
                }

                Vector2 enemyPos = enemy.transform.position;
                float dist = Vector2.Distance(origin, enemyPos);
                if (dist <= maxRadius && dist > minRadius)
                {
                    Vector2 dir = (enemyPos - origin).normalized;
                    knockback.AddKnockback(dir * pulseStrength, 6f);
                    pushedEnemies.Add(enemy);
                }
            }
        }

        bool IsEnemyInside(EnemyController2D enemy)
        {
            var state = enemy.GetComponent<EnemyRegionState>();
            if (state != null)
            {
                return state.EnemyInside;
            }

            var region = CastleRegionTracker.Instance;
            return region != null && region.EnemyInside(enemy);
        }

#if UNITY_EDITOR
        public void Debug_StartPulse()
        {
            StartPulse();
        }

        public void Debug_TickPulse(float dt)
        {
            TickPulse(dt);
        }

        public void Debug_ForceSubscribe()
        {
            if (pressureTracker == null)
            {
                pressureTracker = GetComponent<BarrierPressureTracker>();
            }

            if (pressureTracker != null)
            {
                pressureTracker.OnPressureTriggered += HandlePressureTriggered;
            }
        }
#endif
    }
}
