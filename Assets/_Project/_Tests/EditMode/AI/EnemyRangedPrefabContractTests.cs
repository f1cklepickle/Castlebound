using Castlebound.Gameplay.AI;
using Castlebound.Gameplay.Projectile;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Castlebound.Tests.AI
{
    public class EnemyRangedPrefabContractTests
    {
        private const string MeleePrefabPath = "Assets/_Project/Prefabs/Enemy.prefab";
        private const string RangedPrefabPath = "Assets/_Project/Prefabs/Enemy_Ranged.prefab";
        private const string RockProjectilePath = "Assets/_Project/Prefabs/Projectile_Rock.prefab";

        [Test]
        public void EnemyPrefabs_UseInterchangeableDeliveryAndDataDrivenEquipment()
        {
            var meleePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MeleePrefabPath);
            var rangedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(RangedPrefabPath);

            Assert.NotNull(meleePrefab);
            Assert.NotNull(rangedPrefab);
            Assert.IsInstanceOf<EnemyMeleeAttackDelivery>(meleePrefab.GetComponent<EnemyAttack>().AttackDeliverySource);
            Assert.That(meleePrefab.GetComponent<EnemyEquipment>().SpawnEquipment.EquipmentId, Is.EqualTo("unarmed"));

            var rangedDelivery = rangedPrefab.GetComponent<EnemyProjectileAttackDelivery>();
            var rangedEquipment = rangedPrefab.GetComponent<EnemyEquipment>().SpawnEquipment;
            Assert.NotNull(rangedDelivery);
            Assert.IsInstanceOf<EnemyProjectileAttackDelivery>(rangedPrefab.GetComponent<EnemyAttack>().AttackDeliverySource);
            Assert.That(rangedEquipment.EquipmentId, Is.EqualTo("rock"));
            Assert.That(rangedEquipment.CompatibleRole, Is.EqualTo(EnemyAttackRole.Ranged));
            Transform visualRoot = rangedPrefab.transform.Find("VisualRoot");
            Assert.That(rangedPrefab.GetComponent<EnemyFacing>().VisualTransform, Is.EqualTo(visualRoot));
            Assert.That(rangedDelivery.LaunchPoint, Is.EqualTo(rangedPrefab.transform.Find("VisualRoot/HandSocket")));
            Assert.That(rangedPrefab.tag, Is.EqualTo("Enemy"));
            Assert.That(LayerMask.LayerToName(rangedPrefab.layer), Is.EqualTo("Enemies"));
        }

        [Test]
        public void RockProjectilePrefab_UsesReusableProjectileRuntimeContract()
        {
            var rockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(RockProjectilePath);

            Assert.NotNull(rockPrefab);
            Assert.That(rockPrefab.tag, Is.EqualTo("Projectile"));
            Assert.NotNull(rockPrefab.GetComponent<ProjectileRuntime>());
            Assert.IsInstanceOf<CircleCollider2D>(rockPrefab.GetComponent<Collider2D>());
            Assert.IsTrue(rockPrefab.GetComponent<Collider2D>().isTrigger);
            Assert.That(rockPrefab.GetComponent<Rigidbody2D>().bodyType, Is.EqualTo(RigidbodyType2D.Kinematic));
            Assert.NotNull(rockPrefab.GetComponent<SpriteRenderer>().sprite);
            Assert.That(rockPrefab.GetComponent<ProjectileSpin>().DegreesPerSecond, Is.Not.EqualTo(0f));
        }

        [Test]
        public void EnemyEquipmentAssets_HaveStableUniqueIdsAndCompatibleRoles()
        {
            var unarmed = AssetDatabase.LoadAssetAtPath<EnemyEquipmentDefinition>(
                "Assets/_Project/Items/Definitions/EnemyEquipment_Unarmed.asset");
            var club = AssetDatabase.LoadAssetAtPath<EnemyEquipmentDefinition>(
                "Assets/_Project/Items/Definitions/EnemyEquipment_Club.asset");
            var rock = AssetDatabase.LoadAssetAtPath<EnemyEquipmentDefinition>(
                "Assets/_Project/Items/Definitions/EnemyEquipment_Rock.asset");

            Assert.That(unarmed.EquipmentId, Is.EqualTo("unarmed"));
            Assert.That(club.EquipmentId, Is.EqualTo("club"));
            Assert.That(rock.EquipmentId, Is.EqualTo("rock"));
            Assert.That(club.CompatibleRole, Is.EqualTo(EnemyAttackRole.Melee));
            Assert.That(rock.CompatibleRole, Is.EqualTo(EnemyAttackRole.Ranged));
            Assert.NotNull(rock.ProjectilePrefab);
        }
    }
}
