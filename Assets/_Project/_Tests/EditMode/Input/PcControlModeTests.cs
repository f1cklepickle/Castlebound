using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Castlebound.Gameplay.Input;

namespace Castlebound.Tests.Input
{
    public class PcControlModeTests
    {
        private GameObject root;
        private PlayerRelativeMouseFacingController relativeFacing;
        private Mouse mouse;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("PcControlMode");
            mouse = InputSystem.AddDevice<Mouse>();
            relativeFacing = root.AddComponent<PlayerRelativeMouseFacingController>();
            relativeFacing.Configure(1f);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root);
            if (mouse != null && mouse.added)
                InputSystem.RemoveDevice(mouse);
        }

        [Test]
        public void ResolveFacing_HorizontalDeltaRotatesFromCurrentFacing()
        {
            Vector2 facing = relativeFacing.ResolveFacing(Vector2.up, new Vector2(10f, 0f));

            Vector2 expected = Quaternion.Euler(0f, 0f, -10f) * Vector2.up;
            Assert.That(Vector2.Distance(facing, expected), Is.LessThan(0.0001f));
        }

        [Test]
        public void ResolveFacing_NoMouseDeltaPreservesAccumulatedFacing()
        {
            Vector2 turned = relativeFacing.ResolveFacing(Vector2.up, new Vector2(45f, 0f));

            Vector2 preserved = relativeFacing.ResolveFacing(Vector2.left, Vector2.zero);

            Assert.That(Vector2.Distance(preserved, turned), Is.LessThan(0.0001f));
        }

        [Test]
        public void ResolveFacing_VerticalDeltaDoesNotChangeFacing()
        {
            Vector2 facing = relativeFacing.ResolveFacing(Vector2.up, new Vector2(0f, 500f));

            Assert.That(Vector2.Distance(facing, Vector2.up), Is.LessThan(0.0001f));
        }

        [Test]
        public void ResolveFacing_FastHorizontalDeltaCanTurnOneHundredEightyDegrees()
        {
            Vector2 facing = relativeFacing.ResolveFacing(Vector2.up, new Vector2(180f, 0f));

            Assert.That(Vector2.Distance(facing, Vector2.down), Is.LessThan(0.0001f));
        }

        [Test]
        public void Configure_ClampsSensitivityToSafeProductionRange()
        {
            relativeFacing.Configure(100f);

            Assert.That(relativeFacing.MouseTurnSensitivity, Is.EqualTo(2f));
        }

        [Test]
        public void PlayerController_RoutesWorldRelativeInputDirectlyToMovementAndDash()
        {
            const string controllerPath =
                "Assets/_Project/Scripts/_Project.Gameplay/Player/PlayerController.cs";
            string source = System.IO.File.ReadAllText(controllerPath);

            StringAssert.Contains(
                "movementOrchestrator.Tick(mover, movementInput, movementSpeedMultiplier)",
                source);
            StringAssert.Contains(
                "dashController.TryStart(movementInput, CurrentFacingDirection, defenseAllowsDash)",
                source);
            StringAssert.DoesNotContain("ResolveMovementInput", source);
            StringAssert.DoesNotContain("bodyRelative", source.ToLowerInvariant());
        }

        [Test]
        public void CursorPolicy_LocksOnlyDuringPcGameplay()
        {
            Assert.That(
                PcControlModeController.ResolveLockMode(true, false),
                Is.EqualTo(CursorLockMode.Locked));
            Assert.IsFalse(PcControlModeController.ResolveCursorVisible(true, false));

            Assert.That(
                PcControlModeController.ResolveLockMode(true, true),
                Is.EqualTo(CursorLockMode.None));
            Assert.IsTrue(PcControlModeController.ResolveCursorVisible(true, true));
            Assert.IsTrue(PcControlModeController.ResolveCursorVisible(false, false));
        }

        [Test]
        public void FacingPresentation_SnapsOnlyDuringLockedRelativeMouseGameplay()
        {
            var mode = root.AddComponent<PcControlModeController>();

            Assert.IsTrue(mode.ShouldSnapFacingPresentation);

            mode.RequestPointerMode(root);

            Assert.IsFalse(mode.ShouldSnapFacingPresentation);
        }

        [Test]
        public void PointerModeMovementFallback_AdvancesThroughFacingOrchestratorWithoutSnapping()
        {
            var player = new GameObject("PlayerFacing");
            try
            {
                var orchestrator = new PlayerFacingOrchestrator();
                orchestrator.Tick(player.transform, Vector2.right, 0.02f, false);

                float targetAngle = Quaternion.Angle(
                    player.transform.rotation,
                    Quaternion.Euler(0f, 0f, -90f));
                Assert.That(targetAngle, Is.GreaterThan(0.1f));
                Assert.That(Quaternion.Angle(player.transform.rotation, Quaternion.identity), Is.GreaterThan(0.1f));
            }
            finally
            {
                Object.DestroyImmediate(player);
            }
        }

        [Test]
        public void PointerMode_UsesAbsoluteCombatAimThenMovementFallbackThenRetention()
        {
            var mode = root.AddComponent<PcControlModeController>();
            mode.RequestPointerMode(root);

            Assert.IsTrue(mode.TryResolveFacing(
                Vector2.up, Vector2.left, Vector2.right, true, false, out Vector2 attackFacing));
            Assert.That(attackFacing, Is.EqualTo(Vector2.right));

            Assert.IsTrue(mode.TryResolveFacing(
                attackFacing, Vector2.left, Vector2.down, false, false, out Vector2 movementFacing));
            Assert.That(movementFacing, Is.EqualTo(Vector2.left));

            Assert.IsTrue(mode.TryResolveFacing(
                movementFacing, Vector2.zero, Vector2.down, false, false, out Vector2 retainedFacing));
            Assert.That(retainedFacing, Is.EqualTo(Vector2.left));
        }

        [Test]
        public void PointerOverUi_SuppressesCombatAimAndUsesMovementFallback()
        {
            Vector2 facing = PcControlModeController.ResolvePointerModeFacing(
                Vector2.up,
                Vector2.left,
                Vector2.right,
                true,
                true);

            Assert.That(facing, Is.EqualTo(Vector2.left));
        }

        [Test]
        public void PointerMode_AttackAndDefenseUseTheSameAbsoluteFacingContract()
        {
            Vector2 attackFacing = PcControlModeController.ResolvePointerModeFacing(
                Vector2.up, Vector2.zero, Vector2.right, true, false);
            Vector2 defenseFacing = PcControlModeController.ResolvePointerModeFacing(
                Vector2.up, Vector2.zero, Vector2.right, true, false);

            Assert.That(attackFacing, Is.EqualTo(defenseFacing));
        }

        [Test]
        public void GamepadLook_KeepsExistingFacingPolicyAuthoritative()
        {
            var gamepad = InputSystem.AddDevice<Gamepad>();
            try
            {
                var mode = root.AddComponent<PcControlModeController>();
                mode.NotifyLookInput(Vector2.right);

                Assert.IsFalse(mode.TryResolveFacing(
                    Vector2.up, Vector2.left, Vector2.right, true, false, out _));
            }
            finally
            {
                InputSystem.RemoveDevice(gamepad);
            }
        }

        [Test]
        public void PointerUiLifecycles_RequestTheCentralControlModeOwner()
        {
            string combinedSource = string.Join("\n",
                System.IO.File.ReadAllText("Assets/_Project/Scripts/_Project.Gameplay/UI/InventoryPanelController.cs"),
                System.IO.File.ReadAllText("Assets/_Project/Scripts/_Project.Gameplay/UI/UpgradeMenuController.cs"),
                System.IO.File.ReadAllText("Assets/_Project/Scripts/_Project.Gameplay/UI/VaultPanelController.cs"),
                System.IO.File.ReadAllText("Assets/_Project/Scripts/_Project.Gameplay/World/Placement/WorldPlaceablePlacementController.cs"));

            StringAssert.Contains("RequestPointerMode(this)", combinedSource);
            StringAssert.Contains("ReleasePointerMode(this)", combinedSource);
            StringAssert.DoesNotContain("Cursor.lockState", combinedSource);
            StringAssert.DoesNotContain("Cursor.visible", combinedSource);
        }

        [Test]
        public void DesktopUiHitTesting_IgnoresMobileTouchSurfacesButNotNormalUi()
        {
            var touchSurface = new GameObject("TouchSurface");
            var normalUi = new GameObject("NormalUi");
            try
            {
                touchSurface.AddComponent<TouchMovementZone>();

                Assert.IsTrue(PcControlModeController.IsDesktopTouchSurface(touchSurface));
                Assert.IsFalse(PcControlModeController.IsDesktopTouchSurface(normalUi));
            }
            finally
            {
                Object.DestroyImmediate(touchSurface);
                Object.DestroyImmediate(normalUi);
            }
        }

        [Test]
        public void PrefabUsesProductionComponentsWithoutPrototypeScaffolding()
        {
            const string prefabPath = "Assets/_Project/Prefabs/Player.prefab";
            const string scenePath = "Assets/_Project/Scenes/MainPrototype.unity";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            Assert.NotNull(prefab);
            var authoredRelativeFacing = prefab.GetComponent<PlayerRelativeMouseFacingController>();
            var playerController = prefab.GetComponent<PlayerController>();
            Assert.NotNull(authoredRelativeFacing);
            Assert.NotNull(prefab.GetComponent<PcControlModeController>());
            Assert.NotNull(playerController);

            var serializedController = new SerializedObject(playerController);
            var hitbox = serializedController.FindProperty("hitboxObject").objectReferenceValue as GameObject;
            Assert.NotNull(hitbox);
            Assert.IsTrue(hitbox.transform.IsChildOf(prefab.transform),
                "Melee attack geometry must continue inheriting the player-facing transform.");

            string prefabText = System.IO.File.ReadAllText(prefabPath);
            string scene = System.IO.File.ReadAllText(scenePath);
            StringAssert.DoesNotContain("prototypeEnabled", prefabText + scene);
            StringAssert.DoesNotContain("bodyRelative", prefabText + scene);
        }
    }
}
