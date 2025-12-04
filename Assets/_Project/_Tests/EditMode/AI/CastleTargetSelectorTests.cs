using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Castlebound.Gameplay.AI;

namespace Castlebound.Tests.AI
{
    /// <summary>
    /// Castle gate targeting rules covered here:
    /// - Player outside: always player (regardless of gates).
    /// - Player inside & enemy outside: nearest intact gate; fall back to player when none/broken.
    /// - Player and enemy inside: player.
    /// - Enemy inside, player outside: player.
    /// - Multiple gates: choose nearest.
    /// </summary>
    public class CastleTargetSelectorTests
    {
        // Player outside castle: always target player even if gates exist.
        [Test]
        public void ReturnsPlayer_WhenPlayerOutside_RegardlessOfGates()
        {
            var player = new GameObject("Player").transform;
            var gate = new GameObject("Gate").transform;

            var enemyPosition = Vector2.zero;
            bool enemyInside = false;
            bool playerInside = false;

            var gates = new List<Transform> { gate };

            var result = CastleTargetSelector.ChooseTarget(
                enemyPosition,
                enemyInside,
                playerInside,
                player,
                gates);

            Assert.AreSame(player, result);

            Object.DestroyImmediate(player.gameObject);
            Object.DestroyImmediate(gate.gameObject);
        }

        // Player inside, enemy outside, gate present: target the gate (siege behavior).
        [Test]
        public void ReturnsGate_WhenPlayerInside_EnemyOutside_GatePresent()
        {
            var player = new GameObject("Player").transform;
            var gate = new GameObject("Gate").transform;

            var enemyPosition = new Vector2(-5f, 0f);
            bool enemyInside = false;
            bool playerInside = true;

            var gates = new List<Transform> { gate };

            var result = CastleTargetSelector.ChooseTarget(
                enemyPosition,
                enemyInside,
                playerInside,
                player,
                gates);

            Assert.AreSame(gate, result);

            Object.DestroyImmediate(player.gameObject);
            Object.DestroyImmediate(gate.gameObject);
        }

        // Player inside, enemy outside, no gates: chase the player (gate broken/missing).
        [Test]
        public void ReturnsPlayer_WhenPlayerInside_EnemyOutside_NoGates()
        {
            var player = new GameObject("Player").transform;

            var enemyPosition = new Vector2(-5f, 0f);
            bool enemyInside = false;
            bool playerInside = true;

            var gates = new List<Transform>(); // no gates

            var result = CastleTargetSelector.ChooseTarget(
                enemyPosition,
                enemyInside,
                playerInside,
                player,
                gates);

            Assert.AreSame(player, result);

            Object.DestroyImmediate(player.gameObject);
        }

        // Player and enemy both inside castle: always chase player, even if gates exist/rebuild.
        [Test]
        public void ReturnsPlayer_WhenPlayerAndEnemyInside_EvenIfGatesExist()
        {
            var player = new GameObject("Player").transform;
            var gate = new GameObject("Gate").transform;

            var enemyPosition = new Vector2(0f, 0f);
            bool enemyInside = true;
            bool playerInside = true;

            var gates = new List<Transform> { gate };

            var result = CastleTargetSelector.ChooseTarget(
                enemyPosition,
                enemyInside,
                playerInside,
                player,
                gates);

            Assert.AreSame(player, result);

            Object.DestroyImmediate(player.gameObject);
            Object.DestroyImmediate(gate.gameObject);
        }

        // Rebuild scenario: no gates -> target player, then gate appears -> target gate.
        [Test]
        public void ReturnsGate_AfterGateReappears_WhilePlayerInside_EnemyOutside()
        {
            var player = new GameObject("Player").transform;
            var gate = new GameObject("Gate").transform;

            var enemyPosition = new Vector2(-5f, 0f);
            bool enemyInside = false;
            bool playerInside = true;

            // First: no gates (broken/missing) -> should chase player.
            var noGates = new List<Transform>();

            var firstResult = CastleTargetSelector.ChooseTarget(
                enemyPosition,
                enemyInside,
                playerInside,
                player,
                noGates);

            Assert.AreSame(player, firstResult);

            // Then: gate "rebuilt" (gate list now contains an entry) -> should target gate.
            var withGate = new List<Transform> { gate };

            var secondResult = CastleTargetSelector.ChooseTarget(
                enemyPosition,
                enemyInside,
                playerInside,
                player,
                withGate);

            Assert.AreSame(gate, secondResult);

            Object.DestroyImmediate(player.gameObject);
            Object.DestroyImmediate(gate.gameObject);
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
        public void ReturnsPlayer_WhenBarrierBroken()
        {
            // Arrange
            var enemy = new GameObject("Enemy").transform;
            var player = new GameObject("Player").transform;
            var gate = new GameObject("BrokenGate").transform;

            var barrierHealth = gate.gameObject.AddComponent<BarrierHealth>();
            barrierHealth.TakeDamage(barrierHealth.MaxHealth);

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
            Assert.AreSame(player, result, "When the barrier is broken, selector should target the player.");

            // Cleanup
            Object.DestroyImmediate(enemy.gameObject);
            Object.DestroyImmediate(player.gameObject);
            Object.DestroyImmediate(gate.gameObject);
        }
    }
}
