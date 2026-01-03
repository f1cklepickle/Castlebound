using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.AI;

namespace Castlebound.Tests.Gate
{
    public class BarrierEnemyPushThresholdTests
    {
        [Test]
        public void ResolveOverlap_PushesEnemyOut_UntilPastAnchorThreshold()
        {
            var barrierGo = new GameObject("Barrier");
            var barrierCollider = barrierGo.AddComponent<BoxCollider2D>();
            barrierCollider.size = new Vector2(2f, 2f);
            barrierGo.AddComponent<SpriteRenderer>();

            var hold = barrierGo.AddComponent<EnemyBarrierHoldBehavior>();
            var anchorGo = new GameObject("Anchor");
            anchorGo.transform.position = new Vector2(2f, 0f);
            hold.Debug_SetAnchor(anchorGo.transform);

            var barrierHealth = barrierGo.AddComponent<BarrierHealth>();
            var field = typeof(BarrierHealth).GetField("enemyPushInDistance", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            field.SetValue(barrierHealth, 0.7f);

            var enemyGo = new GameObject("Enemy");
            var enemy = enemyGo.AddComponent<CircleCollider2D>();
            enemy.radius = 0.4f;

            enemyGo.transform.position = new Vector2(1.1f, 0f);
            Physics2D.SyncTransforms();

            Vector2 before = enemyGo.transform.position;
            BarrierOverlapResolver.ResolveOverlap(barrierCollider, enemy, isPlayer: false);
            Vector2 after = enemyGo.transform.position;

            Assert.Greater(after.x, before.x, "Enemy should be pushed out before passing the anchor threshold.");

            Object.DestroyImmediate(enemyGo);
            Object.DestroyImmediate(anchorGo);
            Object.DestroyImmediate(barrierGo);
        }
    }
}
