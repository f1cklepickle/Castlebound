using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.AI;
using UnityEngine.TestTools;

namespace Castlebound.Tests.PlayMode.Castle
{
    public class BarrierPulseEmitterPlayTests
    {
        [UnityTest]
        public IEnumerator PushesOutsideEnemies_WhenTriggered()
        {
            var regionGO = new GameObject("CastleRegion");
            var regionCollider = regionGO.AddComponent<BoxCollider2D>();
            regionCollider.isTrigger = true;
            regionCollider.size = new Vector2(10f, 10f);
            regionGO.AddComponent<Castlebound.Gameplay.AI.CastleRegionTracker>();

            var barrier = new GameObject("Barrier");
            var health = barrier.AddComponent<BarrierHealth>();
            var tracker = barrier.AddComponent<Castlebound.Gameplay.Castle.BarrierPressureTracker>();
            var emitter = AddEmitter(barrier);
            SetPrivateField(tracker, "breaksPerWave", 3);

            var origin = new GameObject("Origin");
            origin.transform.SetParent(barrier.transform, false);
            origin.transform.localPosition = Vector3.zero;
            SetPrivateField(emitter, "pulseOrigin", origin.transform);
            SetPrivateField(emitter, "pulseDuration", 0.6f);
            SetPrivateField(emitter, "pulseRadius", 6f);
            SetPrivateField(emitter, "pulseStrength", 6f);

            var outsideEnemy = CreateEnemy(new Vector2(3f, 0f));
            var insideEnemy = CreateEnemy(Vector2.zero);

            yield return null;

            SimulateEnter(regionGO.GetComponent<Castlebound.Gameplay.AI.CastleRegionTracker>(), insideEnemy.GetComponent<BoxCollider2D>());

            BreakBarrier(health);
            BreakBarrier(health);
            BreakBarrier(health);

            var outsideStart = outsideEnemy.transform.position;
            var insideStart = insideEnemy.transform.position;

            yield return new WaitForSeconds(1f);

            Assert.Greater(Vector2.Distance(outsideEnemy.transform.position, Vector2.zero), Vector2.Distance(outsideStart, Vector2.zero),
                "Outside enemy should be pushed outward.");
            Assert.That(insideEnemy.transform.position, Is.EqualTo(insideStart),
                "Inside enemy should not move from the pulse.");

            UnityEngine.Object.DestroyImmediate(regionGO);
            UnityEngine.Object.DestroyImmediate(barrier);
            UnityEngine.Object.DestroyImmediate(outsideEnemy);
            UnityEngine.Object.DestroyImmediate(insideEnemy);
        }

        private static Component AddEmitter(GameObject barrier)
        {
            var type = ResolveEmitterType();
            Assert.NotNull(type, "Expected Castlebound.Gameplay.Castle.BarrierPulseEmitter to exist.");
            return barrier.AddComponent(type);
        }

        private static Type ResolveEmitterType()
        {
            return Type.GetType("Castlebound.Gameplay.Castle.BarrierPulseEmitter, _Project.Gameplay");
        }

        private static void BreakBarrier(BarrierHealth barrier)
        {
            barrier.MaxHealth = 1;
            barrier.CurrentHealth = 1;
            barrier.TakeDamage(1);
            barrier.Repair();
        }

        private static GameObject CreateEnemy(Vector2 position)
        {
            var enemy = new GameObject("Enemy");
            enemy.transform.position = position;
            var rb = enemy.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            enemy.AddComponent<BoxCollider2D>();
            enemy.AddComponent<EnemyController2D>();
            enemy.AddComponent<EnemyRegionState>();
            enemy.AddComponent<EnemyKnockbackReceiver>();
            return enemy;
        }

        private static void SimulateEnter(Castlebound.Gameplay.AI.CastleRegionTracker tracker, Collider2D collider)
        {
            var enter = typeof(Castlebound.Gameplay.AI.CastleRegionTracker).GetMethod(
                "OnTriggerEnter2D",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            enter.Invoke(tracker, new object[] { collider });
        }

        private static void SetPrivateField(object target, string name, object value)
        {
            var field = target.GetType().GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.NotNull(field, $"Expected field {name} on {target.GetType().Name}.");
            field.SetValue(target, value);
        }
    }
}
