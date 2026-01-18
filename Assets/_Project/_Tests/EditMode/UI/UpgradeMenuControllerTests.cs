using Castlebound.Gameplay.Spawning;
using Castlebound.Gameplay.UI;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.UI
{
    public class UpgradeMenuControllerTests
    {
        [Test]
        public void AutoOpen_OpensMenu_OnFirstPreWave_WhenEnabled()
        {
            var phase = new WavePhaseTracker();
            var menu = new GameObject("Menu");
            var controller = menu.AddComponent<UpgradeMenuController>();
            controller.SetPhaseTracker(phase);
            controller.SetAutoOpenOnFirstPreWave(true);

            Assert.IsFalse(controller.IsMenuOpen, "Menu should start closed.");

            phase.SetPhase(WavePhase.InWave);
            phase.SetPhase(WavePhase.PreWave);

            Assert.IsTrue(controller.IsMenuOpen, "Menu should auto-open on first pre-wave when enabled.");

            controller.CloseMenu();
            Assert.IsFalse(controller.IsMenuOpen, "Menu should close on request.");

            phase.SetPhase(WavePhase.InWave);
            phase.SetPhase(WavePhase.PreWave);

            Assert.IsTrue(controller.IsMenuOpen, "Menu should auto-open on every pre-wave when enabled.");

            Object.DestroyImmediate(menu);
        }

        [Test]
        public void AutoOpen_DoesNotOpen_WhenDisabled()
        {
            var phase = new WavePhaseTracker();
            var menu = new GameObject("Menu");
            var controller = menu.AddComponent<UpgradeMenuController>();
            controller.SetPhaseTracker(phase);
            controller.SetAutoOpenOnFirstPreWave(false);

            phase.SetPhase(WavePhase.InWave);
            phase.SetPhase(WavePhase.PreWave);

            Assert.IsFalse(controller.IsMenuOpen, "Menu should remain closed when auto-open is disabled.");

            Object.DestroyImmediate(menu);
        }

        [Test]
        public void CloseMenu_StartsWave()
        {
            var phase = new WavePhaseTracker();
            var menu = new GameObject("Menu");
            var controller = menu.AddComponent<UpgradeMenuController>();
            controller.SetPhaseTracker(phase);
            controller.SetAutoOpenOnFirstPreWave(true);

            phase.SetPhase(WavePhase.InWave);
            phase.SetPhase(WavePhase.PreWave);

            Assert.IsTrue(controller.IsMenuOpen, "Precondition: menu auto-opened.");

            controller.CloseMenu();

            Assert.IsFalse(controller.IsMenuOpen, "Menu should close.");
            Assert.That(phase.CurrentPhase, Is.EqualTo(WavePhase.InWave), "Closing the menu should start the wave.");

            Object.DestroyImmediate(menu);
        }

        [Test]
        public void Toggle_Ignored_WhenInWave()
        {
            var phase = new WavePhaseTracker();
            var menu = new GameObject("Menu");
            var controller = menu.AddComponent<UpgradeMenuController>();
            controller.SetPhaseTracker(phase);
            controller.SetAutoOpenOnFirstPreWave(false);

            phase.SetPhase(WavePhase.InWave);
            controller.ToggleMenu();

            Assert.IsFalse(controller.IsMenuOpen, "Menu should not open during active wave.");

            phase.SetPhase(WavePhase.PreWave);
            controller.ToggleMenu();

            Assert.IsTrue(controller.IsMenuOpen, "Menu should open during pre-wave when toggled.");

            Object.DestroyImmediate(menu);
        }

        [Test]
        public void CloseMenu_DoesNotStartWaveUntilExplicitStart()
        {
            var phase = new WavePhaseTracker();
            var menu = new GameObject("Menu");
            var controller = menu.AddComponent<UpgradeMenuController>();
            controller.SetPhaseTracker(phase);
            controller.SetAutoOpenOnFirstPreWave(true);

            phase.SetPhase(WavePhase.InWave);
            phase.SetPhase(WavePhase.PreWave);

            Assert.IsTrue(controller.IsMenuOpen, "Precondition: menu auto-opened.");

            controller.CloseMenu();

            Assert.That(phase.CurrentPhase, Is.EqualTo(WavePhase.InWave), "Closing the menu should start the wave.");

            Object.DestroyImmediate(menu);
        }

        [Test]
        public void MenuBlocksPlayerInput_WhenOpen()
        {
            var phase = new WavePhaseTracker();
            var menu = new GameObject("Menu");
            var player = new GameObject("Player");
            var controller = menu.AddComponent<UpgradeMenuController>();

            controller.SetPhaseTracker(phase);
            controller.SetAutoOpenOnFirstPreWave(true);
            controller.SetPlayerController(player.AddComponent<PlayerController>());

            phase.SetPhase(WavePhase.PreWave);
            controller.ToggleMenu();

            Assert.IsTrue(controller.IsMenuOpen, "Menu should be open in pre-wave.");

            Object.DestroyImmediate(menu);
            Object.DestroyImmediate(player);
        }
    }
}
