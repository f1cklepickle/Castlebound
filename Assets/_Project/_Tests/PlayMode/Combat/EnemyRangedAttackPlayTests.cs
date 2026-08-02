using System.Collections;
using Castlebound.Gameplay.AI;
using Castlebound.Gameplay.Projectile;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Castlebound.Tests.PlayMode.Combat
{
    public class EnemyRangedAttackPlayTests
    {
        [UnityTest]
        public IEnumerator SpawnedRangedEnemy_InitializesRockAndLaunchesFromHandSocket()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemy_Ranged.prefab");
            var enemy = Object.Instantiate(prefab);
            var target = new GameObject("LockedTarget");
            ProjectileRuntime launched = null;

            try
            {
                target.transform.position = enemy.transform.position + Vector3.right * 2f;
                yield return null;

                var equipment = enemy.GetComponent<EnemyEquipment>();
                var delivery = enemy.GetComponent<EnemyProjectileAttackDelivery>();
                Assert.That(equipment.ActiveEquipment.EquipmentId, Is.EqualTo("rock"));
                Assert.IsTrue(enemy.transform.Find("VisualRoot/HandSocket/Weapon").GetComponent<SpriteRenderer>().enabled);

                Assert.IsTrue(delivery.TryDeliver(target.transform, equipment.ActiveEquipment));
                launched = delivery.LastLaunchedProjectile;
                Assert.NotNull(launched);
                Assert.That((Vector2)launched.transform.position,
                    Is.EqualTo((Vector2)enemy.transform.Find("VisualRoot/HandSocket").position));
            }
            finally
            {
                if (launched != null)
                {
                    Object.Destroy(launched.gameObject);
                }

                Object.Destroy(target);
                Object.Destroy(enemy);
            }
        }

        [UnityTest]
        public IEnumerator SpawnedRangedEnemy_HoldsPositionInsideMaximumAttackDistance()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemy_Ranged.prefab");
            var enemy = Object.Instantiate(prefab);
            var target = new GameObject("CloseTarget");

            try
            {
                enemy.GetComponent<EnemyController2D>().enabled = false;
                target.transform.position = enemy.transform.position + Vector3.right * 2f;
                yield return null;

                var locomotion = enemy.GetComponent<EnemyLocomotion>();
                var engagement = enemy.GetComponent<EnemyEngagement>();
                locomotion.ComputeBaseMovement(
                    (Vector2)enemy.transform.position,
                    target.transform,
                    null,
                    2f,
                    engagement.EngagementDistance,
                    engagement.ReleaseMargin,
                    1f,
                    8f,
                    2.8f,
                    2f,
                    8,
                    0.01f,
                    0.2f,
                    1f,
                    out Vector2 radial,
                    out Vector2 tangent);

                Assert.That(locomotion.CurrentState, Is.EqualTo(EnemyController2D.State.HOLD));
                Assert.That(radial, Is.EqualTo(Vector2.zero));
                Assert.That(tangent, Is.EqualTo(Vector2.zero));
            }
            finally
            {
                Object.Destroy(target);
                Object.Destroy(enemy);
            }
        }

        [UnityTest]
        public IEnumerator SpawnedRangedEnemy_UsesNeighborSeparationWithoutRetreating()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemy_Ranged.prefab");
            var enemy = Object.Instantiate(prefab);

            try
            {
                enemy.GetComponent<EnemyController2D>().enabled = false;
                yield return null;

                var locomotion = enemy.GetComponent<EnemyLocomotion>();
                locomotion.SetMovementState(EnemyController2D.State.HOLD);
                Vector2 radial = Vector2.zero;
                Vector2 tangent = Vector2.zero;
                locomotion.ApplyHoldMovementPolicy(
                    new EnemyHoldMovementContext(
                        Vector2.right,
                        Vector2.up,
                        hasNeighbors: true,
                        stableBias: Vector2.down,
                        speed: 8f),
                    ref radial,
                    ref tangent);

                Assert.That(radial, Is.EqualTo(Vector2.zero));
                Assert.That(tangent.x, Is.EqualTo(0f).Within(0.001f));
                Assert.That(tangent.y, Is.GreaterThan(0f));
            }
            finally
            {
                Object.Destroy(enemy);
            }
        }
    }
}
