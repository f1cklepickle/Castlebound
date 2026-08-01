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
    }
}
