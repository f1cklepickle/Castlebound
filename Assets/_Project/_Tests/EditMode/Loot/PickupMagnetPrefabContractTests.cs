using Castlebound.Gameplay.Balance;
using Castlebound.Gameplay.Loot;
using Castlebound.Gameplay.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Castlebound.Tests.Loot
{
    public class PickupMagnetPrefabContractTests
    {
        private static readonly string[] PickupPaths =
        {
            "Assets/_Project/Prefabs/Pickups/Pickup_Gold_1.prefab",
            "Assets/_Project/Prefabs/Pickups/Pickup_Potion_Health.prefab",
            "Assets/_Project/Prefabs/Pickups/Pickup_Weapon_Sword.prefab",
            "Assets/_Project/Prefabs/Pickups/Pickup_Weapon_IronClub.prefab",
            "Assets/_Project/Prefabs/Pickups/Pickup_Weapon_Club.prefab",
            "Assets/_Project/Prefabs/Pickups/Pickup_Weapon_RustyDagger.prefab"
        };

        [TestCaseSource(nameof(PickupPaths))]
        public void PickupPrefab_HasMagnetMotion(string path)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            Assert.IsNotNull(prefab, path);
            Assert.IsNotNull(prefab.GetComponent<PickupMagnetMotion>(), path);
        }

        [Test]
        public void PlayerPrefab_HasConfiguredMagnetField()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Player.prefab");

            Assert.IsNotNull(prefab);
            var field = prefab.GetComponent<PickupMagnetField>();
            Assert.IsNotNull(field);
            var expectedStation = AssetDatabase.LoadAssetAtPath<GameBalanceStation>(
                "Assets/_Project/Balance/GameBalanceStation.asset");
            Assert.AreSame(expectedStation, field.BalanceStation);
        }
    }
}
