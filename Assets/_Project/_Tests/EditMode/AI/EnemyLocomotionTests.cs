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
                1f, 0.5f, 0.3f, 3f, 0.8f, 2.5f, 8, 0.01f,
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
    public void ExecuteMovement_DoesNotMoveRootedEnemy()
    {
        GameObject enemy = new GameObject("Enemy");
        try
        {
            Rigidbody2D body = enemy.AddComponent<Rigidbody2D>();
            EnemyRootReceiver root = enemy.AddComponent<EnemyRootReceiver>();
            EnemyLocomotion locomotion = enemy.AddComponent<EnemyLocomotion>();
            root.RootForSeconds(1f);

            locomotion.ExecuteMovement(body, Vector2.right * 3f, Vector2.zero, 0.02f);

            Assert.AreEqual(Vector2.zero, body.position);
        }
        finally
        {
            Object.DestroyImmediate(enemy);
        }
    }
}
