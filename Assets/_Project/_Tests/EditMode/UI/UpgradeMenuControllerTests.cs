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
        public void CloseMenu_DoesNotStartWave()
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
            Assert.That(phase.CurrentPhase, Is.EqualTo(WavePhase.PreWave), "Closing the menu should not start the wave.");

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
        public void NextWaveHudButton_StartsWave()
        {
            var phase = new WavePhaseTracker();
            var hud = new GameObject("NextWaveHud");
            var button = hud.AddComponent<NextWaveHudButton>();
            button.SetPhaseTracker(phase);

            button.StartNextWave();

            Assert.That(phase.CurrentPhase, Is.EqualTo(WavePhase.InWave), "The explicit HUD button should start the wave.");

            Object.DestroyImmediate(hud);
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

        [Test]
        public void HideMenuForPlacement_DoesNotStartWave_AndCanReopen()
        {
            var phase = new WavePhaseTracker();
            var menu = new GameObject("Menu");
            var root = new GameObject("MenuRoot", typeof(RectTransform));
            var controller = menu.AddComponent<UpgradeMenuController>();

            SetPrivateField(controller, "menuRoot", root.GetComponent<RectTransform>());
            controller.SetPhaseTracker(phase);
            controller.SetAutoOpenOnFirstPreWave(false);

            phase.SetPhase(WavePhase.PreWave);
            controller.ToggleMenu();
            Assert.IsTrue(controller.IsMenuOpen, "Precondition: menu should open during pre-wave.");

            controller.HideMenuForPlacement();

            Assert.IsFalse(controller.IsMenuOpen);
            Assert.That(phase.CurrentPhase, Is.EqualTo(WavePhase.PreWave), "Hiding for placement should not start the wave.");

            controller.ReopenMenuAfterPlacement();

            Assert.IsTrue(controller.IsMenuOpen);
            Assert.That(phase.CurrentPhase, Is.EqualTo(WavePhase.PreWave));

            Object.DestroyImmediate(menu);
            Object.DestroyImmediate(root);
        }

        private static void SetPrivateField<T>(object target, string fieldName, T value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(field, $"Expected private field '{fieldName}' to exist.");
            field.SetValue(target, value);
        }
    }
}
