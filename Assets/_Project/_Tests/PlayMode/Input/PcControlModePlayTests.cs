using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;
using UnityEngine.TestTools;
using Castlebound.Gameplay.Combat;
using Castlebound.Gameplay.Input;
using Castlebound.Gameplay.Spawning;
using Castlebound.Gameplay.UI;
using UnityEngine.UI;

namespace Castlebound.Tests.Input
{
    public class PcControlModePlayTests
    {
        private GameObject player;
        private GameObject attacker;
        private GameObject cameraObject;
        private GameObject eventSystemObject;
        private Keyboard keyboard;
        private Mouse mouse;
        private PlayerControls inputActions;
        private PlayerInput playerInput;
        private PcControlModeTestInputModule pointerInputModule;
        private InputSettings.BackgroundBehavior originalBackgroundBehavior;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            originalBackgroundBehavior = InputSystem.settings.backgroundBehavior;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;

            keyboard = InputSystem.AddDevice<Keyboard>();
            mouse = InputSystem.AddDevice<Mouse>();
            player = new GameObject("Player");
            player.SetActive(false);
            player.tag = "Player";
            var body = player.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            player.AddComponent<BoxCollider2D>();
            player.AddComponent<PlayerCollisionMove2D>();
            var relativeFacing = player.AddComponent<PlayerRelativeMouseFacingController>();
            relativeFacing.Configure(1f);
            player.AddComponent<PcControlModeController>();
            player.AddComponent<PlayerFireInputController>();
            player.AddComponent<PlayerAimInputResolver>();
            player.AddComponent<PlayerAttackLoop>();
            player.AddComponent<PlayerDashController>();
            player.AddComponent<Health>().ConfigureMaxHealth(10, true);
            player.AddComponent<PlayerDefenseController>().Configure(0.15f, 0.15f, 120f, 0.6f);
            player.AddComponent<PlayerController>();
            inputActions = new PlayerControls();
            playerInput = player.AddComponent<PlayerInput>();
            playerInput.actions = inputActions.asset;
            playerInput.notificationBehavior = PlayerNotifications.SendMessages;
            playerInput.defaultActionMap = "Player";
            player.SetActive(true);
            playerInput.ActivateInput();
            InputUser.PerformPairingWithDevice(keyboard, playerInput.user);
            InputUser.PerformPairingWithDevice(mouse, playerInput.user);

            Assert.That(playerInput.currentActionMap, Is.Not.Null);
            Assert.That(playerInput.currentActionMap.name, Is.EqualTo("Player"));
            Assert.That(playerInput.currentActionMap.enabled, Is.True);
            Assert.That(playerInput.currentActionMap.FindAction("Fire", true).enabled, Is.True);
            Assert.That(playerInput.currentActionMap.FindAction("Defend", true).enabled, Is.True);
            Assert.That(IsPairedWithPlayer(mouse), Is.True);
            Assert.That(IsPairedWithPlayer(keyboard), Is.True);

            attacker = new GameObject("Attacker");
            cameraObject = new GameObject("Main Camera", typeof(Camera));
            cameraObject.tag = "MainCamera";
            cameraObject.GetComponent<Camera>().orthographic = true;
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.SetActive(false);
            eventSystemObject.AddComponent<EventSystem>();
            pointerInputModule = eventSystemObject.AddComponent<PcControlModeTestInputModule>();
            eventSystemObject.SetActive(true);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (keyboard != null && keyboard.added)
                InputSystem.RemoveDevice(keyboard);
            if (mouse != null && mouse.added)
                InputSystem.RemoveDevice(mouse);
            Object.Destroy(player);
            Object.Destroy(attacker);
            Object.Destroy(cameraObject);
            Object.Destroy(eventSystemObject);
            inputActions?.Dispose();
            InputSystem.settings.backgroundBehavior = originalBackgroundBehavior;
            yield return null;
        }

        [UnityTest]
        public IEnumerator RelativeFacingAndWorldMovement_RemainIndependent()
        {
            var relativeFacing = player.GetComponent<PlayerRelativeMouseFacingController>();
            var controller = player.GetComponent<PlayerController>();
            relativeFacing.QueueMouseDelta(new Vector2(90f, 600f));
            SetField(controller, "movementInput", Vector2.up);

            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            Assert.That(Vector2.Distance(controller.CurrentFacingDirection, Vector2.right), Is.LessThan(0.001f));
            Assert.That(
                Quaternion.Angle(player.transform.rotation, Quaternion.Euler(0f, 0f, -90f)),
                Is.LessThan(0.001f),
                "PC facing presentation must not lag behind its logical combat direction.");
            Assert.That(player.transform.position.y, Is.GreaterThan(0.01f));
            Assert.That(player.transform.position.x, Is.EqualTo(0f).Within(0.001f),
                "World-up movement must not become facing-relative movement.");
        }

        [UnityTest]
        public IEnumerator WorldRelativeDash_UsesMovementDirectionAndExistingFacingClassification()
        {
            var relativeFacing = player.GetComponent<PlayerRelativeMouseFacingController>();
            var controller = player.GetComponent<PlayerController>();
            var dash = player.GetComponent<PlayerDashController>();
            relativeFacing.QueueMouseDelta(new Vector2(90f, 0f));
            yield return new WaitForFixedUpdate();

            AssertDash(Vector2.up, Vector2.up, PlayerDashDirection.Full);
            AssertDash(Vector2.down, Vector2.down, PlayerDashDirection.Rear);
            AssertDash(Vector2.left, Vector2.left, PlayerDashDirection.Rear);
            AssertDash(Vector2.right, Vector2.right, PlayerDashDirection.Full);
            AssertDash(new Vector2(-1f, -1f), new Vector2(-1f, -1f).normalized, PlayerDashDirection.Rear);
            AssertDash(Vector2.zero, Vector2.left, PlayerDashDirection.Rear);

            void AssertDash(
                Vector2 localInput,
                Vector2 expectedDirection,
                PlayerDashDirection expectedClass)
            {
                dash.ResetState();
                SetField(controller, "movementInput", localInput);
                Assert.IsTrue(controller.TryStartDash());
                Assert.That(Vector2.Distance(dash.Direction, expectedDirection), Is.LessThan(0.001f));
                Assert.That(dash.DirectionClass, Is.EqualTo(expectedClass));
            }
        }

        [UnityTest]
        public IEnumerator DefenseAndNeutralDash_ConsumeTheProductionFacingContract()
        {
            var relativeFacing = player.GetComponent<PlayerRelativeMouseFacingController>();
            var controller = player.GetComponent<PlayerController>();
            var defense = player.GetComponent<PlayerDefenseController>();
            var dash = player.GetComponent<PlayerDashController>();
            relativeFacing.QueueMouseDelta(new Vector2(90f, 0f));
            yield return new WaitForFixedUpdate();

            attacker.transform.position = Vector2.right;
            defense.SetDefensePressed(true);
            PlayerHitResult result = defense.ReceiveHit(new PlayerHitRequest(
                1,
                attacker,
                attacker.transform.position,
                CombatDamageType.Melee));

            Assert.That(result.Outcome, Is.EqualTo(PlayerHitOutcome.Parried));
            defense.SetDefensePressed(false);
            defense.Tick(1f);
            Assert.IsTrue(controller.TryStartDash());
            Assert.That(Vector2.Distance(dash.Direction, Vector2.left), Is.LessThan(0.001f),
                "Neutral backward dodge must continue using the established current-facing contract.");
        }

        [UnityTest]
        public IEnumerator InputLock_UnlocksCursorAndResumeRestoresGameplayCursor()
        {
            var controller = player.GetComponent<PlayerController>();
            var cursor = player.GetComponent<PcControlModeController>();
            cursor.RefreshCursorState();
            yield return null;

            Assert.That(cursor.RequestedLockMode, Is.EqualTo(CursorLockMode.Locked));
            Assert.IsFalse(cursor.RequestedCursorVisible);

            controller.SetInputLocked(true);
            Assert.That(cursor.RequestedLockMode, Is.EqualTo(CursorLockMode.None));
            Assert.IsTrue(cursor.RequestedCursorVisible);

            controller.SetInputLocked(false);
            Assert.That(cursor.RequestedLockMode, Is.EqualTo(CursorLockMode.Locked));
            Assert.IsFalse(cursor.RequestedCursorVisible);
        }

        [UnityTest]
        public IEnumerator PointerModeTransition_DiscardsQueuedDeltaAndPreservesFacing()
        {
            var controller = player.GetComponent<PlayerController>();
            var relativeFacing = player.GetComponent<PlayerRelativeMouseFacingController>();
            var mode = player.GetComponent<PcControlModeController>();
            relativeFacing.QueueMouseDelta(new Vector2(90f, 0f));

            mode.RequestPointerMode(player);
            mode.ReleasePointerMode(player);
            yield return new WaitForFixedUpdate();

            Assert.That(Vector2.Distance(controller.CurrentFacingDirection, Vector2.up), Is.LessThan(0.001f));
            Assert.That(mode.RequestedLockMode, Is.EqualTo(CursorLockMode.Locked));
            Assert.IsFalse(mode.RequestedCursorVisible);
        }

        [UnityTest]
        public IEnumerator LockedGameplay_MouseActionsDriveAttackAndDefensePressAndRelease()
        {
            var controller = player.GetComponent<PlayerController>();
            var fire = player.GetComponent<PlayerFireInputController>();
            var defense = player.GetComponent<PlayerDefenseController>();
            controller.SetInputLocked(true);
            controller.SetInputLocked(false);
            yield return null;

            yield return SetMouseButton(MouseButton.Left, true);
            Assert.IsTrue(fire.IsFireHeld);

            yield return SetMouseButton(MouseButton.Left, false);
            yield return new WaitForFixedUpdate();
            Assert.IsFalse(fire.IsFireHeld);

            yield return SetMouseButton(MouseButton.Right, true);
            Assert.IsTrue(defense.IsGuarding);

            yield return SetMouseButton(MouseButton.Right, false);
            yield return new WaitForFixedUpdate();
            Assert.IsFalse(defense.IsGuarding);
        }

        [UnityTest]
        public IEnumerator PointerModeOffUi_MouseCombatUsesActionCallbacksAndAbsoluteAim()
        {
            var mode = player.GetComponent<PcControlModeController>();
            var controller = player.GetComponent<PlayerController>();
            var fire = player.GetComponent<PlayerFireInputController>();
            var defense = player.GetComponent<PlayerDefenseController>();
            pointerInputModule.PointerOverUi = false;
            mode.RequestPointerMode(player);
            yield return null;

            Vector2 aimPosition = Camera.main.WorldToScreenPoint(Vector3.right * 4f);
            yield return SetMouseButton(MouseButton.Left, true, aimPosition);
            yield return new WaitForFixedUpdate();
            Assert.IsTrue(fire.IsFireHeld);
            Assert.That(Vector2.Distance(controller.CurrentFacingDirection, Vector2.right), Is.LessThan(0.05f));

            yield return SetMouseButton(MouseButton.Left, false, aimPosition);
            yield return new WaitForFixedUpdate();
            Assert.IsFalse(fire.IsFireHeld);

            yield return SetMouseButton(MouseButton.Right, true, aimPosition);
            yield return new WaitForFixedUpdate();
            Assert.IsTrue(defense.IsGuarding);
            Assert.That(Vector2.Distance(controller.CurrentFacingDirection, Vector2.right), Is.LessThan(0.05f));

            yield return SetMouseButton(MouseButton.Right, false, aimPosition);
            yield return new WaitForFixedUpdate();
            Assert.IsFalse(defense.IsGuarding);
        }

        [UnityTest]
        public IEnumerator PointerOverUi_SuppressesMouseCombatThroughEventSystem()
        {
            var mode = player.GetComponent<PcControlModeController>();
            var fire = player.GetComponent<PlayerFireInputController>();
            var defense = player.GetComponent<PlayerDefenseController>();
            mode.RequestPointerMode(player);
            pointerInputModule.PointerOverUi = true;
            yield return null;
            Assert.IsTrue(EventSystem.current.IsPointerOverGameObject());

            yield return SetMouseButton(MouseButton.Left, true);
            Assert.IsFalse(fire.IsFireHeld);
            yield return SetMouseButton(MouseButton.Left, false);

            yield return SetMouseButton(MouseButton.Right, true);
            Assert.IsFalse(defense.IsGuarding);
            yield return SetMouseButton(MouseButton.Right, false);
        }

        [UnityTest]
        public IEnumerator ClosingPointerMode_UiClickDoesNotLeakAndNextClickWorks()
        {
            var mode = player.GetComponent<PcControlModeController>();
            var fire = player.GetComponent<PlayerFireInputController>();
            mode.RequestPointerMode(player);
            pointerInputModule.PointerOverUi = true;
            yield return null;

            yield return SetMouseButton(MouseButton.Left, true);
            Assert.IsFalse(fire.IsFireHeld);
            mode.ReleasePointerMode(player);
            Assert.IsFalse(fire.IsFireHeld);

            yield return SetMouseButton(MouseButton.Left, false);
            pointerInputModule.PointerOverUi = false;
            yield return null;
            yield return SetMouseButton(MouseButton.Left, true);
            Assert.IsTrue(fire.IsFireHeld);
            yield return SetMouseButton(MouseButton.Left, false);
        }

        [UnityTest]
        public IEnumerator PointerModeMovementFallback_UsesSmoothFacingPresentation()
        {
            var mode = player.GetComponent<PcControlModeController>();
            var controller = player.GetComponent<PlayerController>();
            mode.RequestPointerMode(player);
            SetField(controller, "movementInput", Vector2.right);

            yield return new WaitForFixedUpdate();

            Assert.IsFalse(mode.ShouldSnapFacingPresentation);
            Assert.That(controller.CurrentFacingDirection, Is.EqualTo(Vector2.right));
            Assert.That(
                Quaternion.Angle(player.transform.rotation, Quaternion.Euler(0f, 0f, -90f)),
                Is.GreaterThan(0.1f));
        }

        [UnityTest]
        public IEnumerator UpgradeMenuOpen_LeavesOffUiMouseCombatAvailable()
        {
            var mode = player.GetComponent<PcControlModeController>();
            var fire = player.GetComponent<PlayerFireInputController>();
            var menuObject = new GameObject("UpgradeMenu");
            var menuRoot = new GameObject("UpgradeMenuRoot", typeof(RectTransform));
            menuRoot.SetActive(false);
            var menu = menuObject.AddComponent<UpgradeMenuController>();
            var phase = new WavePhaseTracker();
            SetField(menu, "menuRoot", menuRoot.GetComponent<RectTransform>());
            SetField(menu, "pcControlModeController", mode);
            menu.SetPhaseTracker(phase);
            menu.SetAutoOpenOnFirstPreWave(false);
            phase.SetPhase(WavePhase.PreWave);

            menu.ToggleMenu();
            pointerInputModule.PointerOverUi = false;
            yield return null;

            Assert.IsTrue(menu.IsMenuOpen);
            Assert.IsTrue(mode.IsPointerModeActive);
            yield return SetMouseButton(MouseButton.Left, true);
            Assert.IsTrue(fire.IsFireHeld,
                "Opening a pointer-mode menu must not globally lock off-UI combat input.");

            yield return SetMouseButton(MouseButton.Left, false);
            Object.Destroy(menuObject);
            Object.Destroy(menuRoot);
        }

        [UnityTest]
        public IEnumerator PointerModeHitTest_IgnoresMobileOverlayButDetectsDesktopPanel()
        {
            var mode = player.GetComponent<PcControlModeController>();
            var canvasObject = new GameObject("PointerCanvas", typeof(Canvas), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var touchSurface = CreateRaycastSurface("MobileTouchSurface", canvasObject.transform);
            touchSurface.AddComponent<TouchMovementZone>();
            mode.RequestPointerMode(player);

            yield return SetMouseButton(MouseButton.Left, false, new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));

            Assert.IsFalse(mode.IsPointerOverUi,
                "Desktop combat must ignore the full-screen mobile touch overlay.");

            CreateRaycastSurface("DesktopPanel", canvasObject.transform);
            yield return null;

            Assert.IsTrue(mode.IsPointerOverUi,
                "A visible desktop panel under the pointer must own mouse input.");

            Object.Destroy(canvasObject);
        }

        private static GameObject CreateRaycastSurface(string name, Transform parent)
        {
            var surface = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            surface.transform.SetParent(parent, false);
            var rect = surface.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            surface.GetComponent<Image>().raycastTarget = true;
            return surface;
        }

        private IEnumerator SetMouseButton(MouseButton button, bool pressed, Vector2? position = null)
        {
            Vector2 screenPosition = position ?? new Vector2(100f, 100f);
            MouseState state = new MouseState { position = screenPosition };
            if (pressed)
                state = state.WithButton(button);

            InputState.Change(mouse, state, InputUpdateType.Dynamic);

            bool buttonIsPressed = button == MouseButton.Left
                ? mouse.leftButton.isPressed
                : mouse.rightButton.isPressed;
            Assert.That(buttonIsPressed, Is.EqualTo(pressed),
                "InputState.Change did not apply the expected synthetic mouse button state.");

            yield return null;
        }

        private bool IsPairedWithPlayer(InputDevice device)
        {
            foreach (InputDevice pairedDevice in playerInput.devices)
            {
                if (pairedDevice == device)
                    return true;
            }

            return false;
        }

        private static void SetField(object instance, string fieldName, object value)
        {
            FieldInfo field = instance.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            field.SetValue(instance, value);
        }
    }

    public class PcControlModeTestInputModule : BaseInputModule
    {
        public bool PointerOverUi { get; set; }

        public override void Process()
        {
        }

        public override bool IsPointerOverGameObject(int pointerId)
        {
            return PointerOverUi;
        }
    }
}
