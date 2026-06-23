using Castlebound.Gameplay.Balance;
using Castlebound.Gameplay.Player;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Inventory
{
    public class PickupMagnetFieldTests
    {
        [Test]
        public void WaveEnd_UsesExtendedSweepProfile_UntilNextWaveStarts()
        {
            var economy = ScriptableObject.CreateInstance<EconomyBalanceTable>();
            economy.PickupMagnetRange = 8f;
            economy.PickupMagnetSpeed = 10.4f;
            economy.PickupSweepRange = 30f;
            economy.PickupSweepSpeed = 26f;
            var station = ScriptableObject.CreateInstance<GameBalanceStation>();
            station.Economy = economy;
            var player = new GameObject("Player");
            var field = player.AddComponent<PickupMagnetField>();
            field.BalanceStation = station;

            Assert.AreEqual(8f, field.ActiveRange);
            Assert.AreEqual(10.4f, field.ActiveSpeed);

            field.HandleWaveEnded();

            Assert.IsTrue(field.IsSweepActive);
            Assert.AreEqual(30f, field.ActiveRange);
            Assert.AreEqual(26f, field.ActiveSpeed);
            Assert.IsTrue(field.IsWithinRange(new Vector3(29f, 0f)));

            field.HandleWaveStarted(2);

            Assert.IsFalse(field.IsSweepActive);
            Assert.IsFalse(field.IsWithinRange(new Vector3(9f, 0f)));

            Object.DestroyImmediate(player);
            Object.DestroyImmediate(station);
            Object.DestroyImmediate(economy);
        }

        [Test]
        public void EconomyTuning_ClampsNegativeMagnetValues()
        {
            var economy = ScriptableObject.CreateInstance<EconomyBalanceTable>();

            economy.PickupMagnetRange = -1f;
            economy.PickupMagnetSpeed = -1f;
            economy.PickupSweepRange = -1f;
            economy.PickupSweepSpeed = -1f;

            Assert.AreEqual(0f, economy.PickupMagnetRange);
            Assert.AreEqual(0f, economy.PickupMagnetSpeed);
            Assert.AreEqual(0f, economy.PickupSweepRange);
            Assert.AreEqual(0f, economy.PickupSweepSpeed);

            Object.DestroyImmediate(economy);
        }
    }
}
