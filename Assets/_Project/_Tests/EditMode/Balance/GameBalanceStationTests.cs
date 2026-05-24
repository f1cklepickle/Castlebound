using Castlebound.Gameplay.Balance;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Balance
{
    public class GameBalanceStationTests
    {
        [Test]
        public void Station_ExposesAssignedCategoryTables()
        {
            var station = ScriptableObject.CreateInstance<GameBalanceStation>();
            var tower = ScriptableObject.CreateInstance<TowerBalanceTable>();
            var barrier = ScriptableObject.CreateInstance<BarrierBalanceTable>();
            var wave = ScriptableObject.CreateInstance<WaveBalanceTable>();
            var enemy = ScriptableObject.CreateInstance<EnemyBalanceTable>();
            var player = ScriptableObject.CreateInstance<PlayerBalanceTable>();
            var economy = ScriptableObject.CreateInstance<EconomyBalanceTable>();

            try
            {
                station.Tower = tower;
                station.Barrier = barrier;
                station.Wave = wave;
                station.Enemy = enemy;
                station.Player = player;
                station.Economy = economy;

                Assert.AreSame(tower, station.Tower);
                Assert.AreSame(barrier, station.Barrier);
                Assert.AreSame(wave, station.Wave);
                Assert.AreSame(enemy, station.Enemy);
                Assert.AreSame(player, station.Player);
                Assert.AreSame(economy, station.Economy);
            }
            finally
            {
                Object.DestroyImmediate(economy);
                Object.DestroyImmediate(player);
                Object.DestroyImmediate(enemy);
                Object.DestroyImmediate(wave);
                Object.DestroyImmediate(barrier);
                Object.DestroyImmediate(tower);
                Object.DestroyImmediate(station);
            }
        }

        [Test]
        public void Station_AllowsPartialCategoryReferences()
        {
            var station = ScriptableObject.CreateInstance<GameBalanceStation>();
            var tower = ScriptableObject.CreateInstance<TowerBalanceTable>();

            try
            {
                station.Tower = tower;

                Assert.AreSame(tower, station.Tower);
                Assert.IsNull(station.Barrier);
                Assert.IsNull(station.Wave);
                Assert.IsNull(station.Enemy);
                Assert.IsNull(station.Player);
                Assert.IsNull(station.Economy);
            }
            finally
            {
                Object.DestroyImmediate(tower);
                Object.DestroyImmediate(station);
            }
        }

        [Test]
        public void CategoryTables_AreScriptableObjectAssets()
        {
            Assert.IsTrue(typeof(ScriptableObject).IsAssignableFrom(typeof(TowerBalanceTable)));
            Assert.IsTrue(typeof(ScriptableObject).IsAssignableFrom(typeof(BarrierBalanceTable)));
            Assert.IsTrue(typeof(ScriptableObject).IsAssignableFrom(typeof(WaveBalanceTable)));
            Assert.IsTrue(typeof(ScriptableObject).IsAssignableFrom(typeof(EnemyBalanceTable)));
            Assert.IsTrue(typeof(ScriptableObject).IsAssignableFrom(typeof(PlayerBalanceTable)));
            Assert.IsTrue(typeof(ScriptableObject).IsAssignableFrom(typeof(EconomyBalanceTable)));
        }
    }
}
