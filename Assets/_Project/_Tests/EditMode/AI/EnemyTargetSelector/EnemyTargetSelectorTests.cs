using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.AI;

namespace Castlebound.Tests.AI
{
    public class EnemyTargetSelectorTests
    {
        [Test]
        public void Outside_PlayerInside_TargetsHomeBarrier()
        {
            var player = new GameObject("Player").transform;
            player.tag = "Player";

            var barrier = new GameObject("HomeBarrier").transform;
            var barrierHealth = barrier.gameObject.AddComponent<BarrierHealth>();
            barrierHealth.Repair(); // ensure intact

            var enemyPosition = new Vector2(-5f, 0f);
            bool enemyInside = false;
            bool playerInside = true;

            var decision = EnemyTargetSelector.Select(new EnemyTargetSelector.Input
            {
                EnemyPosition = enemyPosition,
                EnemyInside = enemyInside,
                PlayerInside = playerInside,
                Player = player,
                HomeBarrier = barrier
            });

            Assert.AreSame(barrier, decision.SteerTarget, "Outside while player is inside should steer to home barrier.");
            Assert.AreSame(barrier, decision.AttackTarget, "Outside while player is inside should attack home barrier.");
            Assert.AreEqual(EnemyTargetType.Barrier, decision.TargetType, "Target type should be Barrier when outside with intact home barrier.");

            Object.DestroyImmediate(player.gameObject);
            Object.DestroyImmediate(barrier.gameObject);
        }

        [Test]
        public void Inside_PlayerInside_WithBrokenHome_TargetsPlayer()
        {
            var player = new GameObject("Player").transform;
            player.tag = "Player";

            var barrier = new GameObject("HomeBarrier").transform;
            var barrierHealth = barrier.gameObject.AddComponent<BarrierHealth>();
            barrierHealth.TakeDamage(barrierHealth.MaxHealth); // broken

            var enemyPosition = new Vector2(0.5f, 0f);
            bool enemyInside = true;
            bool playerInside = true;

            var decision = EnemyTargetSelector.Select(new EnemyTargetSelector.Input
            {
                EnemyPosition = enemyPosition,
                EnemyInside = enemyInside,
                PlayerInside = playerInside,
                Player = player,
                HomeBarrier = barrier
            });

            Assert.AreSame(player, decision.SteerTarget, "Once inside, steering should switch to player even if home barrier is broken.");
            Assert.AreSame(player, decision.AttackTarget, "Once inside, attack target should be player.");
            Assert.AreEqual(EnemyTargetType.Player, decision.TargetType, "Target type should be Player when inside with player inside.");

            Object.DestroyImmediate(player.gameObject);
            Object.DestroyImmediate(barrier.gameObject);
        }

        [Test]
        public void Outside_NearBrokenHomeBarrier_TargetsPlayer()
        {
            var player = new GameObject("Player").transform;
            player.tag = "Player";

            var barrier = new GameObject("HomeBarrier").transform;
            var barrierHealth = barrier.gameObject.AddComponent<BarrierHealth>();
            barrierHealth.TakeDamage(barrierHealth.MaxHealth); // broken

            var enemyPosition = new Vector2(-0.4f, 0f); // within passThroughRadius default (0.6)
            bool enemyInside = false;
            bool playerInside = true;

            var decision = EnemyTargetSelector.Select(new EnemyTargetSelector.Input
            {
                EnemyPosition = enemyPosition,
                EnemyInside = enemyInside,
                PlayerInside = playerInside,
                Player = player,
                HomeBarrier = barrier,
                PassThroughRadius = 0.6f
            });

            Assert.AreSame(player, decision.SteerTarget, "Outside near a broken home barrier should steer to player.");
            Assert.AreSame(player, decision.AttackTarget, "Outside near a broken home barrier should attack player.");
            Assert.AreEqual(EnemyTargetType.Player, decision.TargetType, "Target type should be Player when near a broken home barrier.");

            Object.DestroyImmediate(player.gameObject);
            Object.DestroyImmediate(barrier.gameObject);
        }
    }
}
