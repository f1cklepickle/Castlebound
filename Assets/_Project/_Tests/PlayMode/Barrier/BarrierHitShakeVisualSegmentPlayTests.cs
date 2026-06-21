using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Castlebound.Tests.Barrier
{
    public class BarrierHitShakeVisualSegmentPlayTests
    {
        [UnityTest]
        public IEnumerator EnemyHitBarrier_ShakesGateVisualWithoutMovingRootOrCollider()
        {
            var channel = ScriptableObject.CreateInstance<FeedbackEventChannel>();
            var barrier = new GameObject("Barrier");
            var collider = barrier.AddComponent<BoxCollider2D>();
            var gateShakeRoot = new GameObject("GateShakeRoot");
            gateShakeRoot.transform.SetParent(barrier.transform, false);
            var shake = barrier.AddComponent<BarrierHitShake>();

            SetField(shake, "feedbackChannel", channel);
            SetField(shake, "shakeTarget", gateShakeRoot.transform);
            SetField(shake, "durationSeconds", 0.05f);
            SetField(shake, "intensity", 0.2f);
            shake.enabled = false;
            shake.enabled = true;

            var rootBaseline = barrier.transform.position;
            var colliderBaseline = collider.bounds.center;
            channel.Raise(new FeedbackCue(FeedbackCueType.EnemyHitBarrier, rootBaseline, barrier.GetInstanceID()));

            yield return null;

            Assert.That(gateShakeRoot.transform.localPosition, Is.Not.EqualTo(Vector3.zero));
            Assert.That(barrier.transform.position, Is.EqualTo(rootBaseline));
            Assert.That(collider.bounds.center, Is.EqualTo(colliderBaseline));

            yield return new WaitForSecondsRealtime(0.1f);

            Assert.That(gateShakeRoot.transform.localPosition, Is.EqualTo(Vector3.zero));
            Object.Destroy(barrier);
            Object.Destroy(channel);
        }

        [UnityTest]
        public IEnumerator RepeatedHits_DuringShake_ReturnGateToAuthoredBaseline()
        {
            var context = CreateShakeContext(0.08f);
            context.Channel.Raise(CreateBarrierCue(context.Barrier));
            yield return null;

            context.Channel.Raise(CreateBarrierCue(context.Barrier));
            yield return new WaitForSecondsRealtime(0.15f);

            Assert.That(context.GateShakeRoot.localPosition, Is.EqualTo(Vector3.zero));
            context.Destroy();
        }

        [UnityTest]
        public IEnumerator Repair_DuringShake_RestoresGateImmediatelyAndKeepsBaseline()
        {
            var context = CreateShakeContext(0.2f, true);
            context.Channel.Raise(CreateBarrierCue(context.Barrier));
            yield return null;
            Assert.That(context.GateShakeRoot.localPosition, Is.Not.EqualTo(Vector3.zero));

            context.Health.TakeDamage(1);
            context.Health.Repair();

            Assert.That(context.GateShakeRoot.localPosition, Is.EqualTo(Vector3.zero));
            yield return new WaitForSecondsRealtime(0.25f);
            Assert.That(context.GateShakeRoot.localPosition, Is.EqualTo(Vector3.zero));
            context.Destroy();
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(field, $"Missing field {fieldName}.");
            field.SetValue(target, value);
        }

        private static ShakeContext CreateShakeContext(float duration, bool includeHealth = false)
        {
            var channel = ScriptableObject.CreateInstance<FeedbackEventChannel>();
            var barrier = new GameObject("Barrier");
            barrier.AddComponent<BoxCollider2D>();
            var health = includeHealth ? barrier.AddComponent<BarrierHealth>() : null;
            if (health != null)
            {
                health.MaxHealth = 1;
                health.CurrentHealth = 1;
            }

            var gateShakeRoot = new GameObject("GateShakeRoot");
            gateShakeRoot.transform.SetParent(barrier.transform, false);
            var shake = barrier.AddComponent<BarrierHitShake>();
            SetField(shake, "feedbackChannel", channel);
            SetField(shake, "shakeTarget", gateShakeRoot.transform);
            SetField(shake, "durationSeconds", duration);
            SetField(shake, "intensity", 0.2f);
            shake.enabled = false;
            shake.enabled = true;

            return new ShakeContext(barrier, gateShakeRoot.transform, channel, health);
        }

        private static FeedbackCue CreateBarrierCue(GameObject barrier)
        {
            return new FeedbackCue(FeedbackCueType.EnemyHitBarrier, barrier.transform.position, barrier.GetInstanceID());
        }

        private sealed class ShakeContext
        {
            public GameObject Barrier { get; }
            public Transform GateShakeRoot { get; }
            public FeedbackEventChannel Channel { get; }
            public BarrierHealth Health { get; }

            public ShakeContext(GameObject barrier, Transform gateShakeRoot, FeedbackEventChannel channel, BarrierHealth health)
            {
                Barrier = barrier;
                GateShakeRoot = gateShakeRoot;
                Channel = channel;
                Health = health;
            }

            public void Destroy()
            {
                Object.Destroy(Barrier);
                Object.Destroy(Channel);
            }
        }
    }
}
