using Castlebound.Gameplay.AI;
using Castlebound.Gameplay.Projectile;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Combat
{
    public class EnemyProjectileAttackDeliveryTests
    {
        [Test]
        public void TryDeliver_LaunchesActiveRangedEquipmentTowardLockedTarget()
        {
            var enemy = new GameObject("RangedEnemy");
            var target = new GameObject("LockedTarget");
            var prefabObject = new GameObject("RockProjectilePrefab");
            prefabObject.AddComponent<CircleCollider2D>();
            prefabObject.AddComponent<Rigidbody2D>();
            var projectilePrefab = prefabObject.AddComponent<ProjectileRuntime>();
            var equipment = ScriptableObject.CreateInstance<EnemyEquipmentDefinition>();
            ProjectileRuntime launched = null;

            try
            {
                var delivery = enemy.AddComponent<EnemyProjectileAttackDelivery>();
                enemy.transform.position = new Vector3(1f, 2f);
                target.transform.position = new Vector3(4f, 2f);
                equipment.CompatibleRole = EnemyAttackRole.Ranged;
                equipment.ProjectilePrefab = projectilePrefab;
                equipment.ProjectileSpeed = 7f;
                equipment.ProjectileDamage = 2;
                equipment.ProjectileLifetime = 3f;
                equipment.ProjectileTargetLayerMask = 1 << 6;

                Assert.IsTrue(delivery.TryDeliver(target.transform, equipment));
                launched = delivery.LastLaunchedProjectile;
                Assert.NotNull(launched);
                Assert.That((Vector2)launched.transform.position, Is.EqualTo(new Vector2(1f, 2f)));
            }
            finally
            {
                if (launched != null)
                {
                    Object.DestroyImmediate(launched.gameObject);
                }

                Object.DestroyImmediate(equipment);
                Object.DestroyImmediate(prefabObject);
                Object.DestroyImmediate(target);
                Object.DestroyImmediate(enemy);
            }
        }

        [Test]
        public void CanDeliver_RejectsMeleeEquipment()
        {
            var enemy = new GameObject("RangedEnemy");
            var target = new GameObject("LockedTarget");
            var equipment = ScriptableObject.CreateInstance<EnemyEquipmentDefinition>();
            try
            {
                equipment.CompatibleRole = EnemyAttackRole.Melee;
                var delivery = enemy.AddComponent<EnemyProjectileAttackDelivery>();

                Assert.IsFalse(delivery.CanDeliver(target.transform, equipment));
            }
            finally
            {
                Object.DestroyImmediate(equipment);
                Object.DestroyImmediate(target);
                Object.DestroyImmediate(enemy);
            }
        }

        [Test]
        public void CaptureEquipmentSnapshot_RemainsStableWhenActiveEquipmentChanges()
        {
            var enemy = new GameObject("RangedEnemy");
            var rock = ScriptableObject.CreateInstance<EnemyEquipmentDefinition>();
            var replacement = ScriptableObject.CreateInstance<EnemyEquipmentDefinition>();
            try
            {
                var equipment = enemy.AddComponent<EnemyEquipment>();
                var attack = enemy.AddComponent<EnemyAttack>();
                equipment.Equip(rock);

                var snapshot = attack.CaptureEquipmentSnapshot();
                equipment.Equip(replacement);

                Assert.That(snapshot, Is.SameAs(rock));
                Assert.That(equipment.ActiveEquipment, Is.SameAs(replacement));
            }
            finally
            {
                Object.DestroyImmediate(replacement);
                Object.DestroyImmediate(rock);
                Object.DestroyImmediate(enemy);
            }
        }
    }
}
