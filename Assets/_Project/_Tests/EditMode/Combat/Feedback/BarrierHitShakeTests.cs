using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Combat
{
    public class BarrierHitShakeTests
    {
        [Test]
        public void ShouldRespondToCue_ReturnsTrue_ForMatchingBarrierTarget()
        {
            var go = new GameObject("Barrier");
            var shake = go.AddComponent<BarrierHitShake>();

            var cue = new FeedbackCue(FeedbackCueType.EnemyHitBarrier, Vector3.zero, go.GetInstanceID());
            Assert.IsTrue(shake.ShouldRespondToCue(cue), "Barrier should respond when cue target matches this barrier.");

            Object.DestroyImmediate(go);
        }

        [Test]
        public void ShouldRespondToCue_ReturnsFalse_ForDifferentBarrierTarget()
        {
            var go = new GameObject("BarrierA");
            var shake = go.AddComponent<BarrierHitShake>();
            var other = new GameObject("BarrierB");

            var cue = new FeedbackCue(FeedbackCueType.EnemyHitBarrier, Vector3.zero, other.GetInstanceID());
            Assert.IsFalse(shake.ShouldRespondToCue(cue), "Barrier should ignore cue targeted at a different barrier.");

            Object.DestroyImmediate(other);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void ShouldRespondToCue_ReturnsFalse_ForNonBarrierFeedbackType()
        {
            var go = new GameObject("Barrier");
            var shake = go.AddComponent<BarrierHitShake>();

            var cue = new FeedbackCue(FeedbackCueType.PlayerHit, Vector3.zero, go.GetInstanceID());
            Assert.IsFalse(shake.ShouldRespondToCue(cue), "Barrier shake should ignore non-barrier feedback types.");

            Object.DestroyImmediate(go);
        }
    }
}
