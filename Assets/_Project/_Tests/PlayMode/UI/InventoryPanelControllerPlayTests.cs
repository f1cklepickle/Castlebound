using Castlebound.Gameplay.Inventory;
using Castlebound.Gameplay.Spawning;
using Castlebound.Gameplay.UI;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Castlebound.Tests.UI
{
    public class InventoryPanelControllerPlayTests
    {
        [UnityTest]
        public IEnumerator InventoryPanel_OpensBackpack_AndLocksVaultMidWave()
        {
            var root = new GameObject("InventoryPanelPlayRoot", typeof(Canvas));

            try
            {
                var backpack = root.AddComponent<BackpackInventoryStateComponent>();
                var vault = root.AddComponent<CastleInventoryStateComponent>();
                var phase = new WavePhaseTracker();
                var panel = root.AddComponent<InventoryPanelController>();

                panel.SetBackpackSource(backpack);
                panel.SetCastleInventorySource(vault);
                panel.SetPhaseTracker(phase);

                backpack.State.AddItem("weapon_sword", 1);
                panel.TogglePanel();
                yield return null;

                Assert.IsTrue(panel.IsOpen);
                Assert.That(panel.ActiveTab, Is.EqualTo(InventoryPanelTab.Backpack));

                phase.SetPhase(WavePhase.InWave);
                panel.SetActiveTab(InventoryPanelTab.Vault);
                yield return null;

                Assert.That(panel.ActiveTab, Is.EqualTo(InventoryPanelTab.Backpack));
                Assert.IsFalse(panel.VaultTabButton.interactable);
            }
            finally
            {
                Object.Destroy(root);
            }
        }
    }
}
