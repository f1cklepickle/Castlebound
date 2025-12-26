using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Combat
{
    public class PlayerHitFeedbackTests
    {
        [Test]
        public void PlayerHealth_TakeDamage_RaisesPlayerHitFeedbackCue()
        {
            var channel = ScriptableObject.CreateInstance<FeedbackEventChannel>();

            var player = new GameObject("Player");
            player.tag = "Player";

            var health = player.AddComponent<Health>();
            var field = typeof(Health).GetField("playerHitFeedbackChannel", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(field, "Health should define a playerHitFeedbackChannel field for player-hit feedback.");
            field.SetValue(health, channel);
            var currentField = typeof(Health).GetField("current", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(currentField, "Health should define a current field for tracking health.");
            currentField.SetValue(health, health.Max);

            var raised = false;
            FeedbackCue received = default;
            channel.Raised += cue =>
            {
                raised = true;
                received = cue;
            };

            health.TakeDamage(1);

            Assert.IsTrue(raised, "Player hit feedback cue should be raised on player damage.");
            Assert.AreEqual(FeedbackCueType.PlayerHit, received.Type, "Player hit feedback cue type should be PlayerHit.");
            Assert.That(received.Position, Is.EqualTo(player.transform.position));

            Object.DestroyImmediate(channel);
            Object.DestroyImmediate(player);
        }
    }
}
