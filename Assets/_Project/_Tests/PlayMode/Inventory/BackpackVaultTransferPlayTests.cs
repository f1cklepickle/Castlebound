using System.Collections;
using System.Reflection;
using Castlebound.Gameplay.Inventory;
using Castlebound.Gameplay.Spawning;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Castlebound.Tests.Inventory
{
    public class BackpackVaultTransferPlayTests
    {
        [UnityTest]
        public IEnumerator WaveEndEvent_TransfersBackpackToVault()
        {
            var runnerObject = new GameObject("WaveRunner");
            var playerObject = new GameObject("PlayerInventory");

            try
            {
                var runner = runnerObject.AddComponent<EnemySpawnerRunner>();
                _ = runner.PhaseTracker;

                var backpack = playerObject.AddComponent<BackpackInventoryStateComponent>();
                var vault = playerObject.AddComponent<CastleInventoryStateComponent>();
                var transfer = playerObject.AddComponent<BackpackVaultTransfer>();
                transfer.Configure(backpack, vault, runner);
                backpack.State.AddItem("weapon_sword", 1);

                InvokeWaveEnded(runner);

                Assert.That(vault.State.GetCount("weapon_sword"), Is.EqualTo(1));
                Assert.That(backpack.State.ItemCount, Is.EqualTo(0));
            }
            finally
            {
                Object.DestroyImmediate(runnerObject);
                Object.DestroyImmediate(playerObject);
            }

            yield break;
        }

        private static void InvokeWaveEnded(EnemySpawnerRunner runner)
        {
            var method = typeof(EnemySpawnerRunner).GetMethod(
                "HandleWaveEnded",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method, "Expected EnemySpawnerRunner.HandleWaveEnded for wave-end event contract.");
            method.Invoke(runner, null);
        }
    }
}
