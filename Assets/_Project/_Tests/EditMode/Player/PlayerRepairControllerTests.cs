using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Castlebound.Tests.Player
{
    public class PlayerRepairControllerTests
    {
        private GameObject _playerGo;
        private PlayerController _controller;
        private GameObject _barrierGo;

        [TearDown]
        public void TearDown()
        {
            if (_barrierGo != null)
            {
                Object.DestroyImmediate(_barrierGo);
            }

            if (_playerGo != null)
            {
                Object.DestroyImmediate(_playerGo);
            }
        }

        [Test]
        public void HasRepairableBarrierInRange_ReturnsTrue_ForDamagedUnbrokenBarrier()
        {
            CreatePlayer();
            var barrier = CreateBarrier(new Vector2(1f, 0f), maxHealth: 10, currentHealth: 6);

            var result = _controller.HasRepairableBarrierInRange();

            Assert.IsTrue(result);
            Assert.IsTrue(barrier.CanRepair);
        }

        [Test]
        public void TryRepair_RepairsDamagedBarrier_AndStartsCooldown()
        {
            CreatePlayer();
            var barrier = CreateBarrier(new Vector2(1f, 0f), maxHealth: 10, currentHealth: 6);

            var repaired = _controller.TryRepair();

            Assert.IsTrue(repaired);
            Assert.That(barrier.CurrentHealth, Is.EqualTo(10));
            Assert.That(_controller.RepairCooldownRemaining, Is.EqualTo(_controller.RepairCooldownSeconds).Within(0.001f));
        }

        [Test]
        public void TryRepair_WhenBarrierIsFull_DoesNotStartCooldown()
        {
            CreatePlayer();
            CreateBarrier(new Vector2(1f, 0f), maxHealth: 10, currentHealth: 10);

            var repaired = _controller.TryRepair();

            Assert.IsFalse(repaired);
            Assert.That(_controller.RepairCooldownRemaining, Is.EqualTo(0f));
        }

        [Test]
        public void TryRepair_BlocksUntilCooldownExpires()
        {
            CreatePlayer();
            var barrier = CreateBarrier(new Vector2(1f, 0f), maxHealth: 10, currentHealth: 6);

            Assert.IsTrue(_controller.TryRepair());
            barrier.TakeDamage(2);

            Assert.IsFalse(_controller.TryRepair(), "Repair should be blocked while cooldown is active.");

            _controller.TickRepairCooldown(_controller.RepairCooldownSeconds);

            Assert.IsTrue(_controller.TryRepair(), "Repair should be allowed after cooldown expires.");
            Assert.That(barrier.CurrentHealth, Is.EqualTo(10));
        }

        private void CreatePlayer()
        {
            _playerGo = new GameObject("Player");
            _playerGo.transform.position = Vector3.zero;
            _controller = _playerGo.AddComponent<PlayerController>();
            _controller.RepairRange = 2f;
            _controller.RepairBarrierMask = 1 << 0;
            _controller.RepairCooldownSeconds = 1f;
        }

        private BarrierHealth CreateBarrier(Vector2 position, int maxHealth, int currentHealth)
        {
            _barrierGo = new GameObject("Barrier");
            _barrierGo.layer = 0;
            _barrierGo.transform.position = position;
            _barrierGo.AddComponent<BoxCollider2D>();
            var health = _barrierGo.AddComponent<BarrierHealth>();
            health.MaxHealth = maxHealth;
            health.CurrentHealth = currentHealth;
            Physics2D.SyncTransforms();
            return health;
        }
    }
}
