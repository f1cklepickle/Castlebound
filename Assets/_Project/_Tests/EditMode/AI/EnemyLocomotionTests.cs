using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.AI;

public class EnemyLocomotionTests
{
    [Test]
    public void RequestChase_LeavesHoldAndRecordsRequestUntilCleared()
    {
        GameObject enemy = new GameObject("Enemy");
        try
        {
            EnemyLocomotion locomotion = enemy.AddComponent<EnemyLocomotion>();
            locomotion.SetMovementState(EnemyController2D.State.HOLD);

            locomotion.RequestChase();

            Assert.AreEqual(EnemyController2D.State.CHASE, locomotion.CurrentState);
            Assert.IsTrue(locomotion.IsChaseRequested);
            Assert.IsFalse(locomotion.IsInHoldRange);

            locomotion.ClearChaseRequest();
            Assert.IsFalse(locomotion.IsChaseRequested);
        }
        finally
        {
            Object.DestroyImmediate(enemy);
        }
    }

    [Test]
    public void HoldState_ExposesAttackEligibility()
    {
        GameObject enemy = new GameObject("Enemy");
        try
        {
            EnemyLocomotion locomotion = enemy.AddComponent<EnemyLocomotion>();
            locomotion.SetMovementState(EnemyController2D.State.HOLD);

            Assert.IsTrue(locomotion.IsInHoldRange);
        }
        finally
        {
            Object.DestroyImmediate(enemy);
        }
    }

    [Test]
    public void ComputeBaseMovement_EntersHoldAtPlayerRadius()
    {
        GameObject enemy = new GameObject("Enemy");
        GameObject player = new GameObject("Player");
        player.transform.position = new Vector2(0.5f, 0f);

        try
        {
            EnemyLocomotion locomotion = enemy.AddComponent<EnemyLocomotion>();

            locomotion.ComputeBaseMovement(
                Vector2.zero, player.transform, null,
                0.5f, 1f, 0.5f, 0.3f, 3f, 0.8f, 2.5f, 8, 0.01f,
                0f, 0f, out _, out _);

            Assert.AreEqual(EnemyController2D.State.HOLD, locomotion.CurrentState);
            Assert.IsTrue(locomotion.IsInHoldRange);
        }
        finally
        {
            Object.DestroyImmediate(enemy);
            Object.DestroyImmediate(player);
        }
    }

    [Test]
    public void ComputeBaseMovement_RangedPolicyStopsOrbitWhileHolding()
    {
        GameObject enemy = new GameObject("RangedEnemy");
        GameObject player = new GameObject("Player");
        player.transform.position = new Vector2(0.5f, 0f);

        try
        {
            EnemyLocomotion locomotion = enemy.AddComponent<EnemyLocomotion>();
            EnemyRangedEngagement rangedEngagement = enemy.AddComponent<EnemyRangedEngagement>();
            locomotion.Debug_SetHoldMovementPolicy(rangedEngagement);

            locomotion.ComputeBaseMovement(
                Vector2.zero, player.transform, null,
                0.5f, 5f, 0.25f, 0.3f, 8f, 2.8f, 2f, 8, 0.01f,
                0.2f, 1f, out Vector2 radial, out Vector2 tangent);

            Assert.AreEqual(EnemyController2D.State.HOLD, locomotion.CurrentState);
            Assert.AreEqual(Vector2.zero, radial);
            Assert.AreEqual(Vector2.zero, tangent);
        }
        finally
        {
            Object.DestroyImmediate(enemy);
            Object.DestroyImmediate(player);
        }
    }

    [Test]
    public void ComputeBaseMovement_RangedPolicyApproachesOutsideMaximumDistance()
    {
        GameObject enemy = new GameObject("RangedEnemy");
        GameObject player = new GameObject("Player");
        player.transform.position = new Vector2(10f, 0f);

        try
        {
            EnemyLocomotion locomotion = enemy.AddComponent<EnemyLocomotion>();
            EnemyRangedEngagement rangedEngagement = enemy.AddComponent<EnemyRangedEngagement>();
            locomotion.Debug_SetHoldMovementPolicy(rangedEngagement);

            locomotion.ComputeBaseMovement(
                Vector2.zero, player.transform, null,
                10f, 5f, 0.25f, 0.3f, 8f, 2.8f, 2f, 8, 0.01f,
                0.2f, 1f, out Vector2 radial, out Vector2 tangent);
            locomotion.ApplyHoldMovementPolicy(
                new EnemyHoldMovementContext(
                    Vector2.right,
                    Vector2.up,
                    hasNeighbors: true,
                    stableBias: Vector2.down,
                    speed: 8f),
                ref radial,
                ref tangent);

            Assert.AreEqual(EnemyController2D.State.CHASE, locomotion.CurrentState);
            Assert.That(radial.x, Is.GreaterThan(0f));
            Assert.AreEqual(0f, radial.y);
            Assert.AreEqual(Vector2.zero, tangent);
        }
        finally
        {
            Object.DestroyImmediate(enemy);
            Object.DestroyImmediate(player);
        }
    }

    [Test]
    public void ComputeBaseMovement_RangedPolicyReseatsInsideReleaseMargin()
    {
        GameObject enemy = new GameObject("RangedEnemy");
        GameObject player = new GameObject("Player");
        player.transform.position = new Vector2(5.1f, 0f);

        try
        {
            EnemyLocomotion locomotion = enemy.AddComponent<EnemyLocomotion>();
            EnemyRangedEngagement rangedEngagement = enemy.AddComponent<EnemyRangedEngagement>();
            locomotion.Debug_SetHoldMovementPolicy(rangedEngagement);
            locomotion.SetMovementState(EnemyController2D.State.HOLD);

            locomotion.ComputeBaseMovement(
                Vector2.zero, player.transform, null,
                5.1f, 5f, 0.25f, 0.3f, 8f, 2.8f, 2f, 8, 0.01f,
                0.2f, 1f, out Vector2 radial, out Vector2 tangent);

            Assert.AreEqual(EnemyController2D.State.HOLD, locomotion.CurrentState);
            Assert.That(radial.x, Is.GreaterThan(0f));
            Assert.AreEqual(0f, radial.y);
            Assert.AreEqual(Vector2.zero, tangent);
        }
        finally
        {
            Object.DestroyImmediate(enemy);
            Object.DestroyImmediate(player);
        }
    }

    [Test]
    public void ApplyHoldMovementPolicy_CrowdedRangedEnemySpreadsWithoutRetreating()
    {
        GameObject enemy = new GameObject("RangedEnemy");

        try
        {
            EnemyLocomotion locomotion = enemy.AddComponent<EnemyLocomotion>();
            enemy.AddComponent<EnemyApproachSpread>();
            EnemyRangedEngagement rangedEngagement = enemy.AddComponent<EnemyRangedEngagement>();
            locomotion.Debug_SetHoldMovementPolicy(rangedEngagement);
            locomotion.SetMovementState(EnemyController2D.State.HOLD);
            Vector2 radial = Vector2.zero;
            Vector2 tangent = Vector2.zero;

            locomotion.ApplyHoldMovementPolicy(
                new EnemyHoldMovementContext(
                    Vector2.right,
                    Vector2.up,
                    hasNeighbors: true,
                    stableBias: Vector2.down,
                    speed: 8f),
                ref radial,
                ref tangent);

            Assert.AreEqual(Vector2.zero, radial);
            Assert.That(tangent.x, Is.EqualTo(0f).Within(0.001f));
            Assert.That(tangent.y, Is.GreaterThan(0f));
            Assert.That(tangent.magnitude, Is.LessThanOrEqualTo(1.001f));
        }
        finally
        {
            Object.DestroyImmediate(enemy);
        }
    }

    [Test]
    public void ComputeBaseMovement_DefaultMeleePolicyPreservesHoldOrbit()
    {
        GameObject enemy = new GameObject("MeleeEnemy");
        GameObject player = new GameObject("Player");
        player.transform.position = new Vector2(0.5f, 0f);

        try
        {
            EnemyLocomotion locomotion = enemy.AddComponent<EnemyLocomotion>();

            locomotion.ComputeBaseMovement(
                Vector2.zero, player.transform, null,
                0.5f, 1f, 0.25f, 0.3f, 3f, 0.8f, 2.5f, 8, 0.01f,
                0.2f, 1f, out Vector2 radial, out Vector2 tangent);

            Assert.AreEqual(EnemyController2D.State.HOLD, locomotion.CurrentState);
            Assert.AreEqual(Vector2.zero, radial);
            Assert.That(tangent.sqrMagnitude, Is.GreaterThan(0f));
        }
        finally
        {
            Object.DestroyImmediate(enemy);
            Object.DestroyImmediate(player);
        }
    }

    [Test]
    public void ExecuteMovement_DoesNotMoveRootedEnemy()
    {
        GameObject enemy = new GameObject("Enemy");
        try
        {
            Rigidbody2D body = enemy.AddComponent<Rigidbody2D>();
            EnemyRootReceiver root = enemy.AddComponent<EnemyRootReceiver>();
            EnemyLocomotion locomotion = enemy.AddComponent<EnemyLocomotion>();
            root.RootForSeconds(1f);

            bool movementApplied = locomotion.ExecuteMovement(body, Vector2.right * 3f, Vector2.zero, 0.02f);

            Assert.AreEqual(Vector2.zero, body.position);
            Assert.IsFalse(movementApplied);
        }
        finally
        {
            Object.DestroyImmediate(enemy);
        }
    }

    [Test]
    public void ExecuteMovement_ReportsAppliedMovementForPresentation()
    {
        GameObject enemy = new GameObject("Enemy");
        try
        {
            Rigidbody2D body = enemy.AddComponent<Rigidbody2D>();
            EnemyLocomotion locomotion = enemy.AddComponent<EnemyLocomotion>();

            bool movementApplied = locomotion.ExecuteMovement(
                body,
                Vector2.right * 3f,
                Vector2.zero,
                0.02f);

            Assert.IsTrue(movementApplied);
        }
        finally
        {
            Object.DestroyImmediate(enemy);
        }
    }
}
