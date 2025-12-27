using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Combat
{
    public class PlayerHitEnemyFeedbackTests
    {
        [Test]
        public void PlayerHitbox_HitEnemy_RaisesPlayerHitEnemyFeedbackCue()
        {
            var channel = ScriptableObject.CreateInstance<FeedbackEventChannel>();

            var hitboxGo = new GameObject("PlayerHitbox");
            hitboxGo.AddComponent<BoxCollider2D>();
            var hitbox = hitboxGo.AddComponent<Hitbox>();

            var field = typeof(Hitbox).GetField("playerHitEnemyFeedbackChannel", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(field, "Hitbox should define a playerHitEnemyFeedbackChannel field for player-hit-enemy feedback.");
            field.SetValue(hitbox, channel);

            var enemy = new GameObject("Enemy");
            enemy.tag = "Enemy";
            var enemyCollider = enemy.AddComponent<BoxCollider2D>();

            var tryHit = typeof(Hitbox).GetMethod("TryHit", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(tryHit, "Hitbox should define a TryHit method for processing hits.");

            var raised = false;
            FeedbackCue received = default;
            channel.Raised += cue =>
            {
                raised = true;
                received = cue;
            };

            var awake = typeof(Hitbox).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(awake, "Hitbox should define an Awake method for initialization.");
            awake.Invoke(hitbox, null);
            hitbox.Activate();
            tryHit.Invoke(hitbox, new object[] { enemyCollider });

            Assert.IsTrue(raised, "Player hit enemy feedback cue should be raised when the hitbox hits an enemy.");
            Assert.AreEqual(FeedbackCueType.PlayerHitEnemy, received.Type, "Feedback cue type should be PlayerHitEnemy.");
            Assert.That(received.Position, Is.EqualTo(enemy.transform.position));
            Assert.That(received.TargetInstanceId, Is.EqualTo(enemy.GetInstanceID()));

            Object.DestroyImmediate(channel);
            Object.DestroyImmediate(hitboxGo);
            Object.DestroyImmediate(enemy);
        }
    }
}
