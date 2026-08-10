using System.Collections;
using System.Reflection;
using Castlebound.Gameplay.Combat;
using Castlebound.Gameplay.Input;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace Castlebound.Tests.PlayMode.Player
{
    public class PlayerDashPlayTests
    {
        [UnityTest]
        public IEnumerator DesktopAndGamepadDashBindings_ResolveRequiredRuntimeControls()
        {
            var keyboard = InputSystem.AddDevice<Keyboard>();
            var gamepad = InputSystem.AddDevice<Gamepad>();
            var controls = new PlayerControls();

            try
            {
                yield return null;
                Assert.That(controls.Player.Dash.controls,
                    Has.Some.Matches<InputControl>(control =>
                        control.device == keyboard && control.path.EndsWith("/leftCtrl")));
                Assert.That(controls.Player.Dash.controls,
                    Has.Some.Matches<InputControl>(control =>
                        control.device == gamepad && control.path.EndsWith("/leftStickPress")));
                Assert.That(controls.Player.Move.controls,
                    Has.Some.Matches<InputControl>(control =>
                        control.device == keyboard && control.path.EndsWith("/w")));
                Assert.That(controls.Player.Move.controls,
                    Has.Some.Matches<InputControl>(control =>
                        control.device == gamepad && control.path.EndsWith("/leftStick")));
            }
            finally
            {
                controls.Dispose();
                InputSystem.RemoveDevice(keyboard);
                InputSystem.RemoveDevice(gamepad);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator ForwardAndRear_UseSameSpeed_WhileRearEndsSoonerAndTravelsLess()
        {
            GameObject forward = CreatePlayer(Vector2.zero, 0, out PlayerController forwardController,
                out PlayerDashController forwardDash);
            GameObject rear = CreatePlayer(new Vector2(5f, 0f), 0, out PlayerController rearController,
                out PlayerDashController rearDash);

            try
            {
                SetMovementInput(forwardController, Vector2.up);
                SetMovementInput(rearController, Vector2.down);
                Vector2 forwardStart = forward.transform.position;
                Vector2 rearStart = rear.transform.position;

                Assert.IsTrue(forwardController.TryStartDash());
                Assert.IsTrue(rearController.TryStartDash());

                yield return new WaitForFixedUpdate();
                Vector2 forwardSampleStart = forward.transform.position;
                Vector2 rearSampleStart = rear.transform.position;
                yield return new WaitForFixedUpdate();

                float forwardStep = Vector2.Distance(forward.transform.position, forwardSampleStart);
                float rearStep = Vector2.Distance(rear.transform.position, rearSampleStart);
                Assert.That(forwardStep, Is.EqualTo(rearStep).Within(0.001f));

                float forwardDistance = -1f;
                float rearDistance = -1f;
                while (forwardDash.IsDashing || rearDash.IsDashing)
                {
                    yield return new WaitForFixedUpdate();
                    if (!forwardDash.IsDashing && forwardDistance < 0f)
                        forwardDistance = Vector2.Distance(forward.transform.position, forwardStart);
                    if (!rearDash.IsDashing && rearDistance < 0f)
                        rearDistance = Vector2.Distance(rear.transform.position, rearStart);
                }

                if (forwardDistance < 0f)
                    forwardDistance = Vector2.Distance(forward.transform.position, forwardStart);
                if (rearDistance < 0f)
                    rearDistance = Vector2.Distance(rear.transform.position, rearStart);
                Assert.That(forwardDistance, Is.EqualTo(forwardDash.FullDashDistance).Within(0.25f));
                Assert.That(rearDistance, Is.EqualTo(rearDash.RearDashDistance).Within(0.25f));
                Assert.That(rearDistance / forwardDistance, Is.EqualTo(0.6f).Within(0.08f));
            }
            finally
            {
                Object.Destroy(forward);
                Object.Destroy(rear);
            }
        }

        [UnityTest]
        public IEnumerator Dash_SnapshotsDirection_DoesNotSteerOrRotateFacing()
        {
            GameObject player = CreatePlayer(Vector2.zero, 0, out PlayerController controller,
                out PlayerDashController dash);
            try
            {
                SetMovementInput(controller, Vector2.down);
                Quaternion startingRotation = player.transform.rotation;
                Assert.IsTrue(controller.TryStartDash());
                Vector2 snapshot = dash.Direction;

                SetMovementInput(controller, Vector2.right);
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();

                Assert.That(dash.Direction, Is.EqualTo(snapshot));
                Assert.That(player.transform.position.x, Is.EqualTo(0f).Within(0.001f));
                Assert.That(player.transform.rotation, Is.EqualTo(startingRotation));
                Assert.That(controller.CurrentFacingDirection, Is.EqualTo(Vector2.up));
            }
            finally
            {
                Object.Destroy(player);
            }
        }

        [UnityTest]
        public IEnumerator BlockedDash_ContinuesTimer_SlidesAndRecoversNormalMovement()
        {
            int wallsMask = 1 << LayerMask.NameToLayer("Walls");
            GameObject player = CreatePlayer(Vector2.zero, wallsMask, out PlayerController controller,
                out PlayerDashController dash);
            GameObject wall = new GameObject("Wall");
            wall.layer = LayerMask.NameToLayer("Walls");
            wall.transform.position = new Vector2(0.75f, 0f);
            wall.AddComponent<BoxCollider2D>().size = new Vector2(0.5f, 4f);

            try
            {
                SetMovementInput(controller, new Vector2(1f, 1f));
                Assert.IsTrue(controller.TryStartDash());

                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();

                Assert.IsTrue(dash.IsDashing, "Collision must not cancel the active dash state.");
                Assert.That(player.transform.position.x, Is.LessThanOrEqualTo(0.27f));
                Assert.That(player.transform.position.y, Is.GreaterThan(0.1f),
                    "The unblocked axis should retain the mover's existing sliding behavior.");

                while (dash.IsDashing)
                    yield return new WaitForFixedUpdate();
                float blockedX = player.transform.position.x;
                SetMovementInput(controller, Vector2.left);
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();

                Assert.IsFalse(dash.IsDashing);
                Assert.That(player.transform.position.x, Is.LessThan(blockedX - 0.01f),
                    "Normal movement should resume without stale dash velocity.");
            }
            finally
            {
                Object.Destroy(wall);
                Object.Destroy(player);
            }
        }

        [UnityTest]
        public IEnumerator WallBarrierAndVault_AllClampWithoutCancellingDashState()
        {
            int wallsLayer = LayerMask.NameToLayer("Walls");
            int environmentLayer = LayerMask.NameToLayer("Environment");
            int solidMask = (1 << wallsLayer) | (1 << environmentLayer);
            string[] names = { "Wall", "Barrier", "Vault" };
            int[] layers = { wallsLayer, wallsLayer, environmentLayer };
            var players = new GameObject[names.Length];
            var blockers = new GameObject[names.Length];
            var dashes = new PlayerDashController[names.Length];

            try
            {
                for (int i = 0; i < names.Length; i++)
                {
                    float y = i * 2f;
                    players[i] = CreatePlayer(new Vector2(0f, y), solidMask,
                        out PlayerController controller, out dashes[i]);
                    blockers[i] = new GameObject(names[i]);
                    blockers[i].layer = layers[i];
                    blockers[i].transform.position = new Vector2(0.75f, y);
                    blockers[i].AddComponent<BoxCollider2D>().size = Vector2.one * 0.5f;
                    SetMovementInput(controller, Vector2.right);
                    Assert.IsTrue(controller.TryStartDash());
                }

                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();

                for (int i = 0; i < names.Length; i++)
                {
                    Assert.IsTrue(dashes[i].IsDashing, $"{names[i]} collision cancelled dash state.");
                    Assert.That(players[i].transform.position.x, Is.LessThanOrEqualTo(0.27f),
                        $"Dash passed through representative {names[i]} geometry.");
                }

                while (dashes[0].IsDashing)
                    yield return new WaitForFixedUpdate();

                for (int i = 0; i < names.Length; i++)
                    Assert.IsFalse(dashes[i].IsDashing);
            }
            finally
            {
                for (int i = 0; i < names.Length; i++)
                {
                    if (blockers[i] != null)
                        Object.Destroy(blockers[i]);
                    if (players[i] != null)
                        Object.Destroy(players[i]);
                }
            }
        }

        [UnityTest]
        public IEnumerator RearMovementCanFinishBeforeDamageInvulnerabilityExpires()
        {
            GameObject player = CreatePlayer(Vector2.zero, 0, out PlayerController controller,
                out PlayerDashController dash);
            Health health = player.GetComponent<Health>();
            try
            {
                SetMovementInput(controller, Vector2.down);
                Assert.IsTrue(controller.TryStartDash());

                while (dash.IsDashing)
                    yield return new WaitForFixedUpdate();

                Assert.IsTrue(dash.IsInvulnerable);
                Assert.That(health.ApplyDamage(3), Is.Zero);

                while (dash.IsInvulnerable)
                    yield return new WaitForFixedUpdate();

                Assert.That(health.ApplyDamage(3), Is.EqualTo(3));
            }
            finally
            {
                Object.Destroy(player);
            }
        }

        [UnityTest]
        public IEnumerator ActiveAttackIsCancelled_AndDefenseIsRejectedDuringDash()
        {
            GameObject player = CreatePlayer(Vector2.zero, 0, out PlayerController controller,
                out PlayerDashController dash);
            var fire = player.GetComponent<PlayerFireInputController>();
            var attack = player.GetComponent<PlayerAttackLoop>();
            var defense = player.GetComponent<PlayerDefenseController>();
            try
            {
                fire.Configure(null, () => true);
                fire.OnFirePressedStateChanged(true);
                attack.Tick(0.01f, 1f, true);

                Assert.IsTrue(controller.TryStartDash());
                defense.SetDefensePressed(true);
                yield return new WaitForFixedUpdate();

                Assert.IsFalse(fire.IsFireHeld);
                Assert.IsFalse(attack.IsSwingActive);
                Assert.That(defense.State, Is.EqualTo(PlayerDefenseState.Idle));
                Assert.IsTrue(dash.IsDashing);
            }
            finally
            {
                Object.Destroy(player);
            }
        }

        [UnityTest]
        public IEnumerator MobileRelease_UsesVirtualGamepadDashActionAndCurrentReleaseDirection()
        {
            var inputRoot = new GameObject("MobileDashInput");
            inputRoot.SetActive(false);
            var movementZone = inputRoot.AddComponent<TouchMovementZone>();
            var driver = inputRoot.AddComponent<MobileInputDriver>();
            var dashRoot = new GameObject("DashOwner");
            var dash = dashRoot.AddComponent<PlayerDashController>();
            dash.Configure(20f, 0.5f, 0.6f, 1f, 0.4f, 0.9f);
            SetField(driver, "movementZone", movementZone);
            SetField(driver, "dashController", dash);
            SetField(driver, "enableInEditor", true);

            var controls = new PlayerControls();
            bool dashPerformed = false;
            Vector2 performedDirection = Vector2.zero;
            controls.Player.Dash.performed += _ =>
            {
                dashPerformed = true;
                performedDirection = controls.Player.Move.ReadValue<Vector2>();
            };
            controls.Player.Enable();

            try
            {
                inputRoot.SetActive(true);
                movementZone.MaxRadius = 100f;
                movementZone.SimulatePointerDown(Vector2.zero);
                movementZone.SimulateDrag(new Vector2(95f, 0f));
                movementZone.SimulatePointerUp();

                yield return null;
                yield return null;

                Assert.IsTrue(dashPerformed);
                Assert.That(performedDirection.x, Is.GreaterThanOrEqualTo(0.9f));

                dashPerformed = false;
                movementZone.SimulatePointerDown(Vector2.zero);
                movementZone.SimulateDrag(new Vector2(100f, 0f));
                movementZone.SimulateDrag(new Vector2(40f, 0f));
                movementZone.SimulatePointerUp();
                yield return null;
                yield return null;

                Assert.IsFalse(dashPerformed,
                    "Returning below the threshold before release must not retain historical arming.");
            }
            finally
            {
                controls.Dispose();
                Object.Destroy(inputRoot);
                Object.Destroy(dashRoot);
            }
        }

        [UnityTest]
        public IEnumerator DashDealsNoDamage_AndDoesNotAddEnemyDisplacementBehavior()
        {
            int enemiesMask = 1 << LayerMask.NameToLayer("Enemies");
            GameObject player = CreatePlayer(Vector2.zero, enemiesMask, out PlayerController controller,
                out PlayerDashController dash);
            GameObject enemy = new GameObject("Enemy");
            enemy.tag = "Enemy";
            enemy.layer = LayerMask.NameToLayer("Enemies");
            enemy.transform.position = new Vector2(0f, 0.8f);
            enemy.AddComponent<BoxCollider2D>().size = Vector2.one * 0.5f;
            Health enemyHealth = enemy.AddComponent<Health>();
            enemyHealth.ConfigureMaxHealth(10, refill: true);
            Vector2 enemyStart = enemy.transform.position;

            try
            {
                SetMovementInput(controller, Vector2.up);
                Assert.IsTrue(controller.TryStartDash());
                while (dash.IsDashing)
                    yield return new WaitForFixedUpdate();

                Assert.That(enemyHealth.Current, Is.EqualTo(10));
                Assert.That((Vector2)enemy.transform.position, Is.EqualTo(enemyStart));
            }
            finally
            {
                Object.Destroy(enemy);
                Object.Destroy(player);
            }
        }

        private static GameObject CreatePlayer(
            Vector2 position,
            int solidMask,
            out PlayerController controller,
            out PlayerDashController dash)
        {
            var player = new GameObject("Player");
            player.tag = "Player";
            player.layer = LayerMask.NameToLayer("Player");
            player.transform.position = position;
            var body = player.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            player.AddComponent<BoxCollider2D>().size = Vector2.one * 0.5f;
            var mover = player.AddComponent<PlayerCollisionMove2D>();
            SetField(mover, "solidMask", (LayerMask)solidMask);
            dash = player.AddComponent<PlayerDashController>();
            dash.Configure(10f, 0.2f, 0.6f, 0.4f, 0.25f, 0.9f);
            player.AddComponent<PlayerFireInputController>();
            player.AddComponent<PlayerAttackLoop>();
            player.AddComponent<PlayerDefenseController>().Configure(0.15f, 0.15f, 120f, 0.6f);
            controller = player.AddComponent<PlayerController>();
            player.GetComponent<Health>().ConfigureMaxHealth(10, refill: true);
            return player;
        }

        private static void SetMovementInput(PlayerController controller, Vector2 value)
        {
            SetField(controller, "movementInput", value);
        }

        private static void SetField(object instance, string fieldName, object value)
        {
            FieldInfo field = instance.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field, $"Expected field {fieldName} on {instance.GetType().Name}.");
            field.SetValue(instance, value);
        }
    }
}
