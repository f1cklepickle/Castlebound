using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.AI
{
    public class EnemyControllerKnockbackTests
    {
        [Test]
        public void AppliesKnockback_WhenTargetIsNull()
        {
            var previousSimulationMode = Physics2D.simulationMode;
            Physics2D.simulationMode = SimulationMode2D.Script;
            try
            {
                var enemy = new GameObject("Enemy");
                var rb = enemy.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.position = Vector2.zero;

                var controller = enemy.AddComponent<EnemyController2D>();
                var knockback = enemy.AddComponent<Castlebound.Gameplay.AI.EnemyKnockbackReceiver>();

                // Force no valid target path.
                typeof(EnemyController2D)
                    .GetField("target", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.SetValue(controller, null);

                knockback.AddKnockback(new Vector2(5f, 0f), 4f);

                var fixedUpdate = typeof(EnemyController2D).GetMethod(
                    "FixedUpdate",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                fixedUpdate.Invoke(controller, null);
                Physics2D.Simulate(Time.fixedDeltaTime);

                Assert.Greater(rb.position.x, 0.01f, "Enemy should move from knockback even with no target.");

                Object.DestroyImmediate(enemy);
            }
            finally
            {
                Physics2D.simulationMode = previousSimulationMode;
            }
        }
    }
}
