using Castlebound.Gameplay.Balance;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Castlebound.Tests.Balance
{
    public class PlayerBalanceApplierTests
    {
        [Test]
        public void Apply_ConfiguresPlayerBaselinesFromBalanceTable()
        {
            var station = ScriptableObject.CreateInstance<GameBalanceStation>();
            var table = ScriptableObject.CreateInstance<PlayerBalanceTable>();
            var player = new GameObject("Player");

            try
            {
                player.AddComponent<Rigidbody2D>();
                player.AddComponent<BoxCollider2D>();
                var health = player.AddComponent<Health>();
                var mover = player.AddComponent<PlayerCollisionMove2D>();
                var controller = player.AddComponent<PlayerController>();
                var applier = player.AddComponent<PlayerBalanceApplier>();

                table.BaseMaxHealth = 150;
                table.BaseMoveSpeed = 9.5f;
                table.BaseRepairRange = 3.25f;
                table.BaseRepairCooldownSeconds = 1.5f;
                station.Player = table;
                applier.BalanceStation = station;
                applier.PlayerController = controller;

                Assert.IsTrue(applier.Apply());

                Assert.That(health.Max, Is.EqualTo(150));
                Assert.That(health.Current, Is.EqualTo(150));
                Assert.That(mover.MoveSpeed, Is.EqualTo(9.5f).Within(0.001f));
                Assert.That(controller.RepairRange, Is.EqualTo(3.25f).Within(0.001f));
                Assert.That(controller.RepairCooldownSeconds, Is.EqualTo(1.5f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(player);
                Object.DestroyImmediate(table);
                Object.DestroyImmediate(station);
            }
        }

        [Test]
        public void Apply_WhenTableMissing_LeavesAuthoredValues()
        {
            var station = ScriptableObject.CreateInstance<GameBalanceStation>();
            var player = new GameObject("Player");

            try
            {
                player.AddComponent<Rigidbody2D>();
                player.AddComponent<BoxCollider2D>();
                var health = player.AddComponent<Health>();
                var mover = player.AddComponent<PlayerCollisionMove2D>();
                var controller = player.AddComponent<PlayerController>();
                var applier = player.AddComponent<PlayerBalanceApplier>();

                health.ConfigureMaxHealth(200, true);
                mover.MoveSpeed = 12f;
                controller.RepairRange = 2f;
                controller.RepairCooldownSeconds = 1f;
                applier.BalanceStation = station;
                applier.PlayerController = controller;

                Assert.IsFalse(applier.Apply());

                Assert.That(health.Max, Is.EqualTo(200));
                Assert.That(mover.MoveSpeed, Is.EqualTo(12f).Within(0.001f));
                Assert.That(controller.RepairRange, Is.EqualTo(2f).Within(0.001f));
                Assert.That(controller.RepairCooldownSeconds, Is.EqualTo(1f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(player);
                Object.DestroyImmediate(station);
            }
        }
    }
}
