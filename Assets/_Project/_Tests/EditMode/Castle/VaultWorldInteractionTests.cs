using Castlebound.Gameplay.Castle;
using Castlebound.Gameplay.Inventory;
using Castlebound.Gameplay.Spawning;
using Castlebound.Gameplay.UI;
using NUnit.Framework;
using System.IO;
using TMPro;
using UnityEngine;

namespace Castlebound.Tests.Castle
{
    public class VaultWorldInteractionTests
    {
        private GameObject root;
        private InventoryPanelController panel;
        private VaultPanelController vaultPanel;
        private BackpackInventoryStateComponent backpack;
        private CastleInventoryStateComponent vault;
        private InventoryStateComponent activeInventory;
        private WavePhaseTracker phase;
        private VaultWorldInteraction interaction;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("VaultInteractionRoot", typeof(Canvas));
            backpack = root.AddComponent<BackpackInventoryStateComponent>();
            activeInventory = root.AddComponent<InventoryStateComponent>();
            vault = root.AddComponent<CastleInventoryStateComponent>();
            panel = root.AddComponent<InventoryPanelController>();
            vaultPanel = root.AddComponent<VaultPanelController>();
            phase = new WavePhaseTracker();
            panel.SetBackpackSource(backpack);
            panel.SetActiveInventorySource(activeInventory);
            panel.SetCastleInventorySource(vault);
            panel.SetPhaseTracker(phase);
            vaultPanel.SetInventorySources(backpack, vault, activeInventory);
            vaultPanel.SetPhaseTracker(phase);

            interaction = root.AddComponent<VaultWorldInteraction>();
            interaction.SetInventoryPanel(panel);
            interaction.SetVaultPanel(vaultPanel);
            interaction.SetPhaseTracker(phase);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root);
        }

        [Test]
        public void TryOpenVault_RequiresRange_AndPreWave()
        {
            vault.State.AddItem("potion_health", 2);

            Assert.IsFalse(interaction.TryOpenVault());

            interaction.SetPlayerInRange(true);
            phase.SetPhase(WavePhase.InWave);
            Assert.IsFalse(interaction.TryOpenVault());

            phase.SetPhase(WavePhase.PreWave);
            Assert.IsTrue(interaction.TryOpenVault());
            Assert.IsFalse(panel.IsOpen);
            Assert.IsTrue(vaultPanel.IsOpen);
            AssertTextExists("potion_health x2");
        }

        [Test]
        public void TryToggleVault_OpensWhenClosedAndAccessible()
        {
            vault.State.AddItem("potion_health", 1);
            interaction.SetPlayerInRange(true);
            phase.SetPhase(WavePhase.PreWave);

            Assert.IsTrue(interaction.TryToggleVault());

            Assert.IsTrue(vaultPanel.IsOpen);
            AssertTextExists("potion_health x1");
        }

        [Test]
        public void TryToggleVault_ClosesWhenVaultPanelIsOpen()
        {
            vault.State.AddItem("potion_health", 1);
            interaction.SetPlayerInRange(true);
            phase.SetPhase(WavePhase.PreWave);

            Assert.IsTrue(interaction.TryOpenVault());
            Assert.IsTrue(vaultPanel.IsOpen);

            Assert.IsTrue(interaction.TryToggleVault());

            Assert.IsFalse(vaultPanel.IsOpen);
        }

        [Test]
        public void TryToggleVault_DoesNotCloseOpenVault_WhenOutOfRange()
        {
            vault.State.AddItem("potion_health", 1);
            interaction.SetPlayerInRange(true);
            phase.SetPhase(WavePhase.PreWave);

            Assert.IsTrue(interaction.TryOpenVault());

            interaction.SetPlayerInRange(false);

            Assert.IsFalse(interaction.TryToggleVault());
            Assert.IsTrue(vaultPanel.IsOpen);
        }

        [Test]
        public void TryToggleVault_DoesNotOpenWhenOutOfRangeOrInWave()
        {
            vault.State.AddItem("potion_health", 1);

            Assert.IsFalse(interaction.TryToggleVault());
            Assert.IsFalse(vaultPanel.IsOpen);

            interaction.SetPlayerInRange(true);
            phase.SetPhase(WavePhase.InWave);

            Assert.IsFalse(interaction.TryToggleVault());
            Assert.IsFalse(vaultPanel.IsOpen);
        }

        [Test]
        public void TryOpenVault_RemainsOpenOnly_WhenVaultPanelIsAlreadyOpen()
        {
            vault.State.AddItem("potion_health", 1);
            interaction.SetPlayerInRange(true);
            phase.SetPhase(WavePhase.PreWave);

            Assert.IsTrue(interaction.TryOpenVault());
            Assert.IsTrue(interaction.TryOpenVault());

            Assert.IsTrue(vaultPanel.IsOpen);
        }

        [Test]
        public void TryOpenVault_KeepsBackpackPanelVisibleBesideVaultPanel()
        {
            backpack.State.AddItem("weapon_sword", 1);
            vault.State.AddItem("potion_health", 1);
            panel.TogglePanel();
            interaction.SetPlayerInRange(true);
            phase.SetPhase(WavePhase.PreWave);

            Assert.IsTrue(interaction.TryOpenVault());

            Assert.IsTrue(panel.IsOpen);
            Assert.IsTrue(vaultPanel.IsOpen);
            AssertTextExists("weapon_sword x1");
            AssertTextExists("potion_health x1");
        }

        [Test]
        public void ScreenPositionHitTest_UsesConfiguredCamera_AndTouchTarget()
        {
            var cameraObject = new GameObject("WorldCamera");
            var camera = cameraObject.AddComponent<UnityEngine.Camera>();
            camera.orthographic = true;
            camera.transform.position = new Vector3(0f, 0f, -10f);

            var target = root.AddComponent<CircleCollider2D>();
            target.radius = 1f;
            interaction.SetWorldCamera(camera);
            interaction.SetTouchTargetCollider(target);

            var center = camera.WorldToScreenPoint(Vector3.zero);
            var outside = camera.WorldToScreenPoint(new Vector3(5f, 5f, 0f));

            Assert.IsTrue(interaction.IsScreenPositionOverTouchTarget(center));
            Assert.IsFalse(interaction.IsScreenPositionOverTouchTarget(outside));

            Object.DestroyImmediate(cameraObject);
        }

        [Test]
        public void RefreshPlayerRange_PollsTouchTarget_WhenTriggerCallbacksAreMissed()
        {
            var target = root.AddComponent<CircleCollider2D>();
            target.isTrigger = true;
            target.radius = 2f;
            interaction.SetTouchTargetCollider(target);

            var player = new GameObject("Player", typeof(CircleCollider2D));
            player.tag = "Player";
            player.transform.position = new Vector3(1f, 0f, 0f);

            interaction.RefreshPlayerRange();

            Assert.IsTrue(interaction.PlayerInRange);

            player.transform.position = new Vector3(5f, 0f, 0f);
            interaction.RefreshPlayerRange();

            Assert.IsFalse(interaction.PlayerInRange);

            Object.DestroyImmediate(player);
        }

        [Test]
        public void TryOpenVault_SyncsPanelPhaseTrackerBeforeOpening()
        {
            var stalePanelPhase = new WavePhaseTracker();
            stalePanelPhase.SetPhase(WavePhase.InWave);
            panel.SetPhaseTracker(stalePanelPhase);

            interaction.SetPlayerInRange(true);
            phase.SetPhase(WavePhase.PreWave);
            vault.State.AddItem("potion_health", 1);

            Assert.IsTrue(interaction.TryOpenVault());
            Assert.IsTrue(vaultPanel.IsOpen);
            AssertTextExists("potion_health x1");
        }

        [Test]
        public void TryOpenVault_UsesCurrentRangeState_InsteadOfRepollingDuringOpen()
        {
            var target = root.AddComponent<CircleCollider2D>();
            target.isTrigger = true;
            target.radius = 2f;
            interaction.SetTouchTargetCollider(target);
            interaction.SetPlayerInRange(true);
            vault.State.AddItem("potion_health", 1);

            Assert.IsTrue(interaction.TryOpenVault());
            Assert.IsTrue(vaultPanel.IsOpen);
            AssertTextExists("potion_health x1");
        }

        [Test]
        public void TryOpenVault_UsesDiscoveredWaveRunnerPhase()
        {
            var runnerObject = new GameObject("EnemySpawnerRunner");
            var runner = runnerObject.AddComponent<EnemySpawnerRunner>();
            var stalePanelPhase = new WavePhaseTracker();
            stalePanelPhase.SetPhase(WavePhase.InWave);
            panel.SetPhaseTracker(stalePanelPhase);
            interaction.SetPhaseTracker(null);

            interaction.SetPlayerInRange(true);
            runner.PhaseTracker.SetPhase(WavePhase.PreWave);
            vault.State.AddItem("potion_health", 1);

            Assert.IsTrue(interaction.TryOpenVault());
            Assert.IsTrue(vaultPanel.IsOpen);
            AssertTextExists("potion_health x1");

            Object.DestroyImmediate(runnerObject);
        }

        [Test]
        public void TryOpenVault_DoesNotReplaceExplicitPhaseTracker_WithDiscoveredRunner()
        {
            var runnerObject = new GameObject("EnemySpawnerRunner");
            var runner = runnerObject.AddComponent<EnemySpawnerRunner>();
            runner.PhaseTracker.SetPhase(WavePhase.InWave);

            interaction.SetPlayerInRange(true);
            phase.SetPhase(WavePhase.PreWave);
            vault.State.AddItem("potion_health", 1);

            Assert.IsTrue(interaction.TryOpenVault());
            Assert.IsTrue(vaultPanel.IsOpen);
            AssertTextExists("potion_health x1");

            Object.DestroyImmediate(runnerObject);
        }

        [Test]
        public void TryOpenVault_ResolvesSiblingOutlinePresenter()
        {
            var parent = new GameObject("Vault");
            var outlineObject = new GameObject("Outline");
            outlineObject.transform.SetParent(parent.transform, false);
            var outlineRenderer = outlineObject.AddComponent<SpriteRenderer>();
            var outlinePresenter = outlineObject.AddComponent<VaultOutlinePresenter>();
            outlinePresenter.SetOutlineRenderer(outlineRenderer);

            var interactionObject = new GameObject("InteractionTrigger");
            interactionObject.transform.SetParent(parent.transform, false);
            var siblingInteraction = interactionObject.AddComponent<VaultWorldInteraction>();
            siblingInteraction.SetInventoryPanel(panel);
            siblingInteraction.SetPhaseTracker(phase);
            siblingInteraction.SetPlayerInRange(true);

            phase.SetPhase(WavePhase.PreWave);
            siblingInteraction.TryOpenVault();

            foreach (var edge in outlinePresenter.GetEdgeRenderersForTests())
            {
                Assert.IsTrue(edge.enabled);
                Assert.AreEqual(Color.white, edge.color);
            }

            Object.DestroyImmediate(parent);
        }

        [Test]
        public void TouchHoldInput_UsesPointerFallback_WhenTouchscreenIsUnavailable()
        {
            var source = File.ReadAllText("Assets/_Project/Scripts/_Project.Gameplay/Castle/VaultWorldInteraction.cs");

            StringAssert.Contains("Touchscreen.current", source);
            StringAssert.Contains("Pointer.current", source);
            StringAssert.Contains("TryToggleVaultFromHeldScreenPosition(screenPosition, Time.deltaTime)", source);
        }

        private void AssertTextExists(string expected)
        {
            var labels = root.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var label in labels)
            {
                if (label.text == expected)
                {
                    return;
                }
            }

            Assert.Fail($"Expected inventory panel text '{expected}'.");
        }
    }
}
