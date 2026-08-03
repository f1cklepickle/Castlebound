using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Castlebound.Gameplay.AI;
using Castlebound.Gameplay.Combat;
using Castlebound.Gameplay.Inventory;
using Castlebound.Gameplay.Spawning;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Castlebound.Tests.PlayMode.Spawning
{
    public class GoblinEquipmentWavePlayTests
    {
        [UnityTest]
        public IEnumerator SpawnReady_AppliesCompatibleRequestedEquipmentBeforeFirstUpdate()
        {
            var unarmed = CreateEquipment("unarmed", EnemyAttackRole.Melee);
            var club = CreateEquipment("club", EnemyAttackRole.Melee);
            var playerClub = ScriptableObject.CreateInstance<WeaponDefinition>();
            playerClub.CombatProfile = club.CombatProfile;
            var prefab = CreateEnemyPrefab("EquipmentMeleePrefab", unarmed, ranged: false);
            var runnerObject = new GameObject("EquipmentSpawnerRunner");
            var runner = runnerObject.AddComponent<EnemySpawnerRunner>();

            try
            {
                SetPrefabMap(runner, EnemyArchetypeIds.GoblinMelee, prefab);
                InvokeSpawnReady(runner, new SpawnRequest(
                    EnemyArchetypeIds.GoblinMelee,
                    "GateA",
                    "Center",
                    Vector2.zero,
                    Vector2.down,
                    club));

                var clone = GameObject.Find("EquipmentMeleePrefab(Clone)");
                Assert.NotNull(clone);
                Assert.That(clone.GetComponent<EnemyEquipment>().ActiveEquipment, Is.SameAs(club));
                Assert.That(
                    clone.GetComponent<EnemyEquipment>().ActiveEquipment.CombatProfile,
                    Is.SameAs(playerClub.CombatProfile));
                yield return null;
                Assert.That(clone.GetComponent<EnemyEquipment>().ActiveEquipment, Is.SameAs(club));
            }
            finally
            {
                DestroyByName("EquipmentMeleePrefab(Clone)");
                Object.DestroyImmediate(runnerObject);
                Object.DestroyImmediate(prefab);
                Object.DestroyImmediate(playerClub);
                DestroyEquipment(club);
                DestroyEquipment(unarmed);
            }
        }

        [UnityTest]
        public IEnumerator SpawnReady_IncompatibleEquipmentKeepsSafePrefabDefault()
        {
            var rock = CreateEquipment("rock", EnemyAttackRole.Ranged);
            var club = CreateEquipment("club", EnemyAttackRole.Melee);
            var prefab = CreateEnemyPrefab("EquipmentRangedPrefab", rock, ranged: true);
            var runnerObject = new GameObject("EquipmentSpawnerRunner");
            var runner = runnerObject.AddComponent<EnemySpawnerRunner>();

            try
            {
                SetPrefabMap(runner, EnemyArchetypeIds.GoblinRanged, prefab);
                LogAssert.Expect(
                    LogType.Error,
                    "EnemySpawnerRunner: equipment 'club' is incompatible with Ranged enemy type 'goblin_ranged'; keeping prefab default.");
                InvokeSpawnReady(runner, new SpawnRequest(
                    EnemyArchetypeIds.GoblinRanged,
                    "GateA",
                    "Center",
                    Vector2.zero,
                    Vector2.down,
                    club));

                var clone = GameObject.Find("EquipmentRangedPrefab(Clone)");
                Assert.NotNull(clone);
                Assert.That(clone.GetComponent<EnemyEquipment>().ActiveEquipment, Is.SameAs(rock));
                yield return null;
            }
            finally
            {
                DestroyByName("EquipmentRangedPrefab(Clone)");
                Object.DestroyImmediate(runnerObject);
                Object.DestroyImmediate(prefab);
                DestroyEquipment(club);
                DestroyEquipment(rock);
            }
        }

        private static EnemyEquipmentDefinition CreateEquipment(string id, EnemyAttackRole role)
        {
            var definition = ScriptableObject.CreateInstance<EnemyEquipmentDefinition>();
            var profile = ScriptableObject.CreateInstance<CombatEquipmentProfile>();
            profile.EquipmentId = id;
            profile.RequiredCapabilities = role == EnemyAttackRole.Ranged
                ? CombatEquipmentCapability.ProjectileDelivery
                : CombatEquipmentCapability.MeleeDelivery;
            definition.CombatProfile = profile;
            definition.CompatibleRole = role;
            return definition;
        }

        private static void DestroyEquipment(EnemyEquipmentDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            Object.DestroyImmediate(definition.CombatProfile);
            Object.DestroyImmediate(definition);
        }

        private static GameObject CreateEnemyPrefab(
            string name,
            EnemyEquipmentDefinition defaultEquipment,
            bool ranged)
        {
            var prefab = new GameObject(name);
            var equipment = prefab.AddComponent<EnemyEquipment>();
            typeof(EnemyEquipment).GetField("spawnEquipment", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(equipment, defaultEquipment);
            var attack = prefab.AddComponent<EnemyAttack>();
            if (ranged)
            {
                var delivery = prefab.AddComponent<EnemyProjectileAttackDelivery>();
                attack.AttackDeliverySource = delivery;
            }

            return prefab;
        }

        private static void SetPrefabMap(EnemySpawnerRunner runner, string enemyTypeId, GameObject prefab)
        {
            var map = new Dictionary<string, GameObject> { [enemyTypeId] = prefab };
            typeof(EnemySpawnerRunner).GetField("_prefabMap", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(runner, map);
        }

        private static void InvokeSpawnReady(EnemySpawnerRunner runner, SpawnRequest request)
        {
            var spawnReady = typeof(EnemySpawnerRunner).GetMethod(
                "SpawnReady",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(spawnReady);
            spawnReady.Invoke(runner, new object[] { new List<SpawnRequest> { request } });
        }

        private static void DestroyByName(string objectName)
        {
            var instance = GameObject.Find(objectName);
            if (instance != null)
            {
                Object.DestroyImmediate(instance);
            }
        }
    }
}
