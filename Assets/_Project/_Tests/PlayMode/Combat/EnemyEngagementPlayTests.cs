using System.Collections;
using System.Reflection;
using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Castlebound.Tests.PlayMode.Combat
{
    public class EnemyEngagementPlayTests
    {
        [UnityTest]
        public IEnumerator SharedSurfaceGap_CompletesBarrierAttackFromEveryCardinalDirection()
        {
            Vector2[] directions =
            {
                Vector2.up,
                Vector2.down,
                Vector2.left,
                Vector2.right
            };

            for (int i = 0; i < directions.Length; i++)
            {
                Vector2 direction = directions[i];
                GameObject barrier = CreateBarrier(direction, i);
                GameObject enemy = CreateEnemyForBarrier(barrier.transform, direction, i);

                try
                {
                    yield return new WaitForSeconds(0.05f);

                    Assert.That(
                        barrier.GetComponent<BarrierHealth>().CurrentHealth,
                        Is.EqualTo(9),
                        $"The shared engagement contract should complete a hit from {direction}.");
                }
                finally
                {
                    Object.Destroy(enemy);
                    Object.Destroy(barrier);
                }

                yield return null;
            }
        }

        private static GameObject CreateBarrier(Vector2 direction, int index)
        {
            var barrier = new GameObject($"Barrier_{index}");
            barrier.transform.position = direction * 1.4f;
            barrier.transform.rotation = Quaternion.Euler(
                0f,
                0f,
                Mathf.Abs(direction.x) > 0f ? 90f : 0f);
            var collider = barrier.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(2f, 1f);
            barrier.AddComponent<BarrierHealth>();
            return barrier;
        }

        private static GameObject CreateEnemyForBarrier(
            Transform barrier,
            Vector2 direction,
            int index)
        {
            var enemy = new GameObject($"Enemy_{index}");
            var body = enemy.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            var collider = enemy.AddComponent<CircleCollider2D>();
            collider.radius = 0.5f;

            var controller = enemy.AddComponent<EnemyController2D>();
            controller.enabled = false;
            controller.Debug_SetTargetDecision(barrier, barrier, EnemyTargetType.Barrier);
            enemy.GetComponent<EnemyLocomotion>()
                .SetMovementState(EnemyController2D.State.HOLD);

            var facing = enemy.GetComponent<EnemyFacing>();
            SetField(facing, "attackAlignmentThreshold", 180f);

            var attack = enemy.AddComponent<EnemyAttack>();
            SetField(attack, "windupSeconds", 0.01f);
            attack.CooldownSeconds = 10f;
            Physics2D.SyncTransforms();
            return enemy;
        }

        private static void SetField(object instance, string fieldName, object value)
        {
            instance.GetType()
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(instance, value);
        }
    }
}
