using System.Collections;
using System.Reflection;
using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Castlebound.Tests.Combat
{
    public class FeedbackRoutingPlayTests
    {
        [UnityTest]
        public IEnumerator EnemyHitCue_FlashesOnlyTargetedEnemyAndRestoresOnDisable()
        {
            var channel = ScriptableObject.CreateInstance<FeedbackEventChannel>();
            var target = CreateEnemyFlash("Target", channel, Color.white);
            var other = CreateEnemyFlash("Other", channel, Color.green);

            channel.Raise(new FeedbackCue(
                FeedbackCueType.PlayerHitEnemy,
                target.GameObject.transform.position,
                target.GameObject.GetInstanceID()));
            yield return null;

            Assert.That(target.Renderer.color, Is.EqualTo(new Color(1f, 0.2f, 0.2f, 1f)));
            Assert.That(other.Renderer.color, Is.EqualTo(Color.green));

            target.Listener.enabled = false;
            Assert.That(target.Renderer.color, Is.EqualTo(Color.white));

            Object.Destroy(target.GameObject);
            Object.Destroy(other.GameObject);
            Object.Destroy(channel);
        }

        [UnityTest]
        public IEnumerator PlayerHitCue_PreservesScreenFlashAndRestoresOnDisable()
        {
            var channel = ScriptableObject.CreateInstance<FeedbackEventChannel>();
            var overlay = new GameObject("PlayerHitFlash", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var image = overlay.GetComponent<Image>();
            var flash = overlay.AddComponent<PlayerHitScreenFlash>();
            SetField(flash, "feedbackChannel", channel);
            SetField(flash, "overlayImage", image);
            flash.enabled = false;
            flash.enabled = true;

            channel.Raise(new FeedbackCue(FeedbackCueType.PlayerHit, Vector3.zero));
            yield return null;

            Assert.That(image.color.a, Is.GreaterThan(0f));

            flash.enabled = false;
            Assert.That(image.color.a, Is.Zero);

            Object.Destroy(overlay);
            Object.Destroy(channel);
        }

        [UnityTest]
        public IEnumerator StaggerOverlay_RemainsYellowUntilTargetRefreshIsAcknowledged()
        {
            var channel = ScriptableObject.CreateInstance<FeedbackEventChannel>();
            var enemy = new GameObject("StaggeredEnemy");
            var renderer = enemy.AddComponent<SpriteRenderer>();
            var attack = enemy.AddComponent<EnemyAttack>();
            var receiver = enemy.AddComponent<EnemyStaggerReceiver>();
            receiver.Configure(true, 1f, attack);
            var listener = enemy.AddComponent<HitFlashListener>();
            SetField(listener, "feedbackChannel", channel);
            SetField(listener, "targetRenderer", renderer);
            SetField(listener, "staggerReceiver", receiver);
            listener.enabled = false;
            listener.enabled = true;

            receiver.TryStagger();
            yield return null;
            Assert.That(renderer.color, Is.EqualTo(new Color(1f, 0.85f, 0f, 1f)));

            channel.Raise(new FeedbackCue(
                FeedbackCueType.PlayerHitEnemy,
                enemy.transform.position,
                enemy.GetInstanceID()));
            yield return null;
            Assert.That(renderer.color, Is.EqualTo(new Color(1f, 0.2f, 0.2f, 1f)),
                "Damage feedback should remain readable while the enemy is staggered.");

            yield return new WaitForSeconds(0.11f);
            Assert.That(renderer.color, Is.EqualTo(new Color(1f, 0.85f, 0f, 1f)),
                "The stagger overlay should resume after the damage flash completes.");

            receiver.Tick(1f);
            yield return null;
            Assert.That(renderer.color, Is.EqualTo(new Color(1f, 0.85f, 0f, 1f)));

            receiver.AcknowledgeTargetRefresh();
            yield return null;
            Assert.That(renderer.color, Is.EqualTo(Color.white));

            Object.Destroy(enemy);
            Object.Destroy(channel);
        }

        private static FlashContext CreateEnemyFlash(string name, FeedbackEventChannel channel, Color originalColor)
        {
            var enemy = new GameObject(name);
            var renderer = enemy.AddComponent<SpriteRenderer>();
            renderer.color = originalColor;
            var listener = enemy.AddComponent<HitFlashListener>();
            SetField(listener, "feedbackChannel", channel);
            SetField(listener, "targetRenderer", renderer);
            listener.enabled = false;
            listener.enabled = true;
            return new FlashContext(enemy, renderer, listener);
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(field, $"Missing field {fieldName}.");
            field.SetValue(target, value);
        }

        private readonly struct FlashContext
        {
            public GameObject GameObject { get; }
            public SpriteRenderer Renderer { get; }
            public HitFlashListener Listener { get; }

            public FlashContext(GameObject gameObject, SpriteRenderer renderer, HitFlashListener listener)
            {
                GameObject = gameObject;
                Renderer = renderer;
                Listener = listener;
            }
        }
    }
}
