using Castlebound.Gameplay.Castle;
using Castlebound.Gameplay.Inventory;
using Castlebound.Gameplay.Spawning;
using Castlebound.Gameplay.UI;
using NUnit.Framework;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;

namespace Castlebound.Tests.PlayMode.Castle
{
    public class VaultWorldInteractionPlayTests
    {
        [UnityTest]
        public IEnumerator TryOpenVault_OpensInventoryPanelVaultView_WhenPreWaveAndInRange()
        {
            var root = new GameObject("VaultWorldInteractionPlayRoot", typeof(Canvas));

            try
            {
                root.AddComponent<BackpackInventoryStateComponent>();
                var vault = root.AddComponent<CastleInventoryStateComponent>();
                var panel = root.AddComponent<InventoryPanelController>();
                var phase = new WavePhaseTracker();
                var interaction = root.AddComponent<VaultWorldInteraction>();

                panel.SetCastleInventorySource(vault);
                panel.SetPhaseTracker(phase);
                interaction.SetInventoryPanel(panel);
                interaction.SetPhaseTracker(phase);
                interaction.SetPlayerInRange(true);
                vault.State.AddItem("potion_health", 1);

                yield return null;

                Assert.IsTrue(interaction.TryOpenVault());
                yield return null;

                Assert.IsTrue(panel.IsOpen);
                AssertTextExists(root, "potion_health x1");
            }
            finally
            {
                Object.Destroy(root);
            }
        }

        private static void AssertTextExists(GameObject root, string expected)
        {
            var labels = root.GetComponentsInChildren<TextMeshProUGUI>(true);
            for (int i = 0; i < labels.Length; i++)
            {
                if (labels[i].text == expected)
                {
                    return;
                }
            }

            Assert.Fail($"Expected inventory panel text '{expected}'.");
        }
    }
}
