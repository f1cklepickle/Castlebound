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

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(field, $"Missing field {fieldName}.");
            field.SetValue(target, value);
        }
    }
}
