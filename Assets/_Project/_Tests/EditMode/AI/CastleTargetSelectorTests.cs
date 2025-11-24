using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.AI;

namespace Castlebound.Tests.AI
{
    /// <summary>
    /// Castle gate targeting rules:
    /// - If both player and enemy are outside, target player.
    /// - If player is inside and enemy is outside, target nearest gate.
    /// - If both are inside, target player.
    /// - If no gates exist, fall back to player.
    /// </summary>
    public class CastleTargetSelectorTests
    {
        [Test]
        public void SelectsPlayer_IfBothOutside()
        {
            // Arrange
            var enemy = new GameObject("Enemy").transform;
            var player = new GameObject("Player").transform;

            bool enemyInside = false;
            bool playerInside = false;

            // Act
            var result = Castlebound.Gameplay.AI.CastleTargetSelector.ChooseTarget(
                enemy.position,
                enemyInside,
                playerInside,
                player,
                System.Array.Empty<Transform>());

            // Assert
            Assert.AreSame(player, result, "When both are outside, selector should choose player.");

            // Cleanup
            Object.DestroyImmediate(enemy.gameObject);
            Object.DestroyImmediate(player.gameObject);
        }

        [Test]
        public void SelectsGate_WhenPlayerInside_EnemyOutside()
        {
            // Arrange
            var enemy = new GameObject("Enemy").transform;
            var player = new GameObject("Player").transform;

            var gateNear = new GameObject("GateNear").transform;
            var gateFar = new GameObject("GateFar").transform;

            enemy.position = new Vector2(0f, 0f);
            gateNear.position = new Vector2(5f, 0f);
            gateFar.position = new Vector2(20f, 0f);

            bool enemyInside = false;
            bool playerInside = true;

            var gates = new[] { gateNear, gateFar };

            // Act
            var result = Castlebound.Gameplay.AI.CastleTargetSelector.ChooseTarget(
                enemy.position,
                enemyInside,
                playerInside,
                player,
                gates);

            // Assert
            Assert.AreSame(gateNear, result, "When player is inside and enemy is outside, selector should choose the nearest gate.");

            // Cleanup
            Object.DestroyImmediate(enemy.gameObject);
            Object.DestroyImmediate(player.gameObject);
            Object.DestroyImmediate(gateNear.gameObject);
            Object.DestroyImmediate(gateFar.gameObject);
        }

        [Test]
        public void SelectsPlayer_IfEnemyAndPlayerInside()
        {
            // Arrange
            var enemy = new GameObject("Enemy").transform;
            var player = new GameObject("Player").transform;

            bool enemyInside = true;
            bool playerInside = true;

            // Act
            var result = CastleTargetSelector.ChooseTarget(
                enemy.position,
                enemyInside,
                playerInside,
                player,
                System.Array.Empty<Transform>());

            // Assert
            Assert.AreSame(player, result, "When enemy and player are inside, selector should choose player.");

            // Cleanup
            Object.DestroyImmediate(enemy.gameObject);
            Object.DestroyImmediate(player.gameObject);
        }

        [Test]
        public void SelectsPlayer_IfEnemyInside_PlayerOutside()
        {
            // Arrange
            var enemy = new GameObject("Enemy").transform;
            var player = new GameObject("Player").transform;

            bool enemyInside = true;
            bool playerInside = false;

            // Act
            var result = CastleTargetSelector.ChooseTarget(
                enemy.position,
                enemyInside,
                playerInside,
                player,
                System.Array.Empty<Transform>());

            // Assert
            Assert.AreSame(player, result, "When enemy is inside and player is outside, selector should still choose player.");

            // Cleanup
            Object.DestroyImmediate(enemy.gameObject);
            Object.DestroyImmediate(player.gameObject);
        }

        [Test]
        public void SelectsNearestGate_WhenMultipleExist()
        {
            // Arrange
            var enemy = new GameObject("Enemy").transform;
            var player = new GameObject("Player").transform;

            var gateNear = new GameObject("GateNear").transform;
            var gateFar = new GameObject("GateFar").transform;

            enemy.position = new Vector2(0f, 0f);
            gateNear.position = new Vector2(3f, 1f);
            gateFar.position = new Vector2(10f, 0f);

            bool enemyInside = false;
            bool playerInside = true;

            var gates = new[] { gateFar, gateNear }; // Intentionally out of order.

            // Act
            var result = CastleTargetSelector.ChooseTarget(
                enemy.position,
                enemyInside,
                playerInside,
                player,
                gates);

            // Assert
            Assert.AreSame(gateNear, result, "Selector should choose the closest gate, not just the first in the list.");

            // Cleanup
            Object.DestroyImmediate(enemy.gameObject);
            Object.DestroyImmediate(player.gameObject);
            Object.DestroyImmediate(gateNear.gameObject);
            Object.DestroyImmediate(gateFar.gameObject);
        }

        [Test]
        public void DefaultsToPlayer_WhenNoGates()
        {
            // Arrange
            var enemy = new GameObject("Enemy").transform;
            var player = new GameObject("Player").transform;

            bool enemyInside = false;
            bool playerInside = true;

            // Act
            var result = CastleTargetSelector.ChooseTarget(
                enemy.position,
                enemyInside,
                playerInside,
                player,
                System.Array.Empty<Transform>());

            // Assert
            Assert.AreSame(player, result, "When player is inside but no gates are available, selector should fall back to player.");

            // Cleanup
            Object.DestroyImmediate(enemy.gameObject);
            Object.DestroyImmediate(player.gameObject);
        }

        [Test]
        public void ReturnsGate_WhenBroken()
        {
            // Arrange
            var enemy = new GameObject("Enemy").transform;
            var player = new GameObject("Player").transform;
            var gate = new GameObject("BrokenGate").transform;

            enemy.position = new Vector2(0f, 0f);
            gate.position = new Vector2(4f, 0f);

            bool enemyInside = false;
            bool playerInside = false;

            var gates = new[] { gate };

            // Act
            var result = CastleTargetSelector.ChooseTarget(
                enemy.position,
                enemyInside,
                playerInside,
                player,
                gates);

            // Assert
            Assert.AreSame(gate, result, "Broken gate should still be selected.");

            // Cleanup
            Object.DestroyImmediate(enemy.gameObject);
            Object.DestroyImmediate(player.gameObject);
            Object.DestroyImmediate(gate.gameObject);
        }
    }
}
