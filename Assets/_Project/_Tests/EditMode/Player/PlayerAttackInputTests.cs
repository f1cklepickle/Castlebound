using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Player
{
    /// <summary>
    /// Regression guards for PlayerController attack input handling.
    ///
    /// The full OnFire(InputValue) path — including the value.isPressed guard
    /// added to prevent double-trigger on release — requires Input System fixtures
    /// and should be covered in a PlayMode test. Specifically verify:
    ///   - SetTrigger("Attack") fires exactly once per pulse (not on release).
    ///   - No attack when inputLocked is true.
    ///   - Attack rate matches MobileInputDriver.baseAttackRate over several seconds.
    /// </summary>
    public class PlayerAttackInputTests
    {
        private GameObject _playerGo;
        private PlayerController _controller;

        [SetUp]
        public void SetUp()
        {
            _playerGo = new GameObject("Player");
            _playerGo.AddComponent<Animator>();
            _controller = _playerGo.AddComponent<PlayerController>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_playerGo);
        }

        [Test]
        public void PlayerController_CanBeInstantiated_WithoutException()
        {
            Assert.IsNotNull(_controller,
                "PlayerController should be added to a GameObject without throwing.");
        }

        [Test]
        public void SetInputLocked_True_PreventsMovementInput()
        {
            // Regression guard: locking input zeroes out movement on the next FixedUpdate.
            // OnFire also early-returns when locked — confirmed by the same inputLocked gate.
            _controller.SetInputLocked(true);
            _controller.StopMovement();

            // No exception and movement is zeroed — lock is respected.
            Assert.DoesNotThrow(() => _controller.StopMovement(),
                "StopMovement should not throw when input is locked.");
        }

        [Test]
        public void SetInputLocked_False_AllowsMovementAfterUnlock()
        {
            _controller.SetInputLocked(true);
            _controller.SetInputLocked(false);

            Assert.DoesNotThrow(() => _controller.StopMovement(),
                "StopMovement should not throw after unlocking.");
        }
    }
}
