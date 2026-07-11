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
        public IEnumerator TryOpenVault_OpensDedicatedVaultPanel_WhenPreWaveAndInRange()
        {
            var root = new GameObject("VaultWorldInteractionPlayRoot", typeof(Canvas));

            try
            {
                var backpack = root.AddComponent<BackpackInventoryStateComponent>();
                var vault = root.AddComponent<CastleInventoryStateComponent>();
                var panel = root.AddComponent<InventoryPanelController>();
                var vaultPanel = root.AddComponent<VaultPanelController>();
                var phase = new WavePhaseTracker();
                var interaction = root.AddComponent<VaultWorldInteraction>();

                panel.SetBackpackSource(backpack);
                panel.SetCastleInventorySource(vault);
                panel.SetPhaseTracker(phase);
                vaultPanel.SetInventorySources(backpack, vault, root.AddComponent<InventoryStateComponent>());
                vaultPanel.SetPhaseTracker(phase);
                interaction.SetInventoryPanel(panel);
                interaction.SetVaultPanel(vaultPanel);
                interaction.SetPhaseTracker(phase);
                interaction.SetPlayerInRange(true);
                backpack.State.AddItem("weapon_sword", 1);
                vault.State.AddItem("potion_health", 1);

                panel.TogglePanel();
                yield return null;

                Assert.IsTrue(interaction.TryOpenVault());
                yield return null;

                Assert.IsTrue(panel.IsOpen);
                Assert.IsTrue(vaultPanel.IsOpen);
                AssertTextExists(root, "weapon_sword x1");
                AssertTextExists(root, "potion_health x1");

                ClickButton(root, "X");
                yield return null;

                Assert.IsFalse(vaultPanel.IsOpen);
            }
            finally
            {
                Object.Destroy(root);
            }
        }

        private static void ClickButton(GameObject root, string label)
        {
            var buttons = root.GetComponentsInChildren<UnityEngine.UI.Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                var text = buttons[i].GetComponentInChildren<TextMeshProUGUI>(true);
                if (text != null && text.text == label)
                {
                    buttons[i].onClick.Invoke();
                    return;
                }
            }

            Assert.Fail($"Expected button '{label}'.");
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
