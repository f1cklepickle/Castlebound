using System.Collections.Generic;
using Castlebound.Gameplay.AI;
using Castlebound.Gameplay.Barrier;
using UnityEngine;

namespace Castlebound.Gameplay.Castle
{
    [RequireComponent(typeof(BarrierPressureTracker))]
    public class BarrierPulseEmitter : MonoBehaviour
    {
        [SerializeField] private Transform pulseOrigin;
        [SerializeField] private float pulseDuration = 1.5f;
        [SerializeField] private float pulseRadius = 3f;
        [SerializeField] private float pulseStrength = 5f;
        [SerializeField] private int pulseLoopCount = 1;
        [SerializeField] private float pulseInsideThreshold = 1.2f;

        public bool IsPulseActive => isPulseActive;
        public float CurrentRadius => currentRadius;
        public float PulseProgress01 => pulseProgress01;
        public float PulseDuration => pulseDuration;
        public Vector3 PulseOriginPosition => ResolvePulseOriginPosition();

        BarrierPressureTracker pressureTracker;
        float elapsed;
        float previousRadius;
        bool isPulseActive;
        int completedLoops;
        float currentRadius;
        float pulseProgress01;

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
            completedLoops = 0;
            currentRadius = 0f;
            pulseProgress01 = 0f;
        }

        void TickPulse(float dt)
        {
            if (!isPulseActive)
            {
                return;
            }

            if (pulseLoopCount < 1)
            {
                pulseLoopCount = 1;
            }

            float stepDuration = Mathf.Max(0f, pulseDuration);

            if (stepDuration <= 0f)
            {
                ApplyWavefront(0f, pulseRadius);
                completedLoops = pulseLoopCount;
                isPulseActive = false;
                currentRadius = pulseRadius;
                pulseProgress01 = 1f;
                return;
            }

            elapsed += dt;

            while (elapsed >= stepDuration && isPulseActive)
            {
                ApplyWavefront(previousRadius, pulseRadius);
                elapsed -= stepDuration;
                completedLoops++;

                if (completedLoops >= pulseLoopCount)
                {
                    isPulseActive = false;
                    elapsed = 0f;
                    previousRadius = pulseRadius;
                    currentRadius = pulseRadius;
                    pulseProgress01 = 1f;
                    return;
                }

                previousRadius = 0f;
                currentRadius = 0f;
                pulseProgress01 = 0f;
            }

            if (!isPulseActive)
            {
                return;
            }

            float t = Mathf.Clamp01(elapsed / stepDuration);
            float liveRadius = pulseRadius * t;
            ApplyWavefront(previousRadius, liveRadius);
            previousRadius = liveRadius;
            currentRadius = liveRadius;
            pulseProgress01 = t;
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

            Vector2 origin = ResolvePulseOriginPosition();

            int count = fallback != null ? fallback.Length : allEnemies.Count;
            for (int i = 0; i < count; i++)
            {
                var enemy = fallback != null ? fallback[i] : allEnemies[i];
                if (enemy == null)
                {
                    continue;
                }

                var enemyCollider = enemy.GetComponent<Collider2D>();
                if (enemyCollider == null)
                {
                    continue;
                }

                var barrierCollider = GetComponent<Collider2D>();
                if (barrierCollider != null && BarrierSideClassifier.IsEnemyPastThreshold(barrierCollider, enemyCollider, pulseInsideThreshold))
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
                // While pulse is active, keep enemies outside the castle from moving
                // inward past the expanding front by applying outward pressure.
                if (dist <= maxRadius)
                {
                    Vector2 dir = (enemyPos - origin).normalized;
                    knockback.AddKnockback(dir * pulseStrength, 6f);
                }
            }
        }

        void OnDrawGizmosSelected()
        {
            var origin = ResolvePulseOriginPosition();
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.8f);
            Gizmos.DrawWireSphere(origin, pulseRadius);
        }

        void OnDrawGizmos()
        {
            if (!Application.isPlaying || !isPulseActive)
            {
                return;
            }

            var origin = ResolvePulseOriginPosition();
            float stepDuration = Mathf.Max(0f, pulseDuration);
            float t = stepDuration > 0f ? Mathf.Clamp01(elapsed / stepDuration) : 1f;
            float liveRadius = pulseRadius * t;
            Gizmos.color = new Color(1f, 0.6f, 0.1f, 0.9f);
            Gizmos.DrawWireSphere(origin, liveRadius);
        }

        Vector3 ResolvePulseOriginPosition()
        {
            if (pulseOrigin != null)
            {
                return pulseOrigin.position;
            }

            var originTransform = transform.Find("PulseOrigin");
            if (originTransform != null)
            {
                pulseOrigin = originTransform;
                return originTransform.position;
            }

            return transform.position;
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
