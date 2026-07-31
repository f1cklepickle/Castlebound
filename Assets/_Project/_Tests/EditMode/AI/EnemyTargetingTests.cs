using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEngine;

public class EnemyTargetingTests
{
    [Test]
    public void Refresh_ProducesSingleBarrierDecisionForMovementAndAttack()
    {
        GameObject enemy = new GameObject("Enemy");
        GameObject player = new GameObject("Player");
        GameObject barrier = new GameObject("Barrier");
        barrier.AddComponent<BarrierHealth>();

        try
        {
            EnemyTargeting targeting = enemy.AddComponent<EnemyTargeting>();
            targeting.Debug_Setup(player.transform, barrier.transform);

            targeting.Refresh(enemy.transform.position, playerInside: true, enemyInside: false);

            Assert.AreSame(barrier.transform, targeting.SteerTarget);
            Assert.AreSame(barrier.transform, targeting.AttackTarget);
            Assert.AreEqual(EnemyTargetType.Barrier, targeting.CurrentTargetType);
        }
        finally
        {
            Object.DestroyImmediate(enemy);
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(barrier);
        }
    }

    [Test]
    public void Refresh_ProducesSinglePlayerDecisionAfterEnemyEntersCastle()
    {
        GameObject enemy = new GameObject("Enemy");
        GameObject player = new GameObject("Player");
        GameObject barrier = new GameObject("Barrier");
        barrier.AddComponent<BarrierHealth>();

        try
        {
            EnemyTargeting targeting = enemy.AddComponent<EnemyTargeting>();
            targeting.Debug_Setup(player.transform, barrier.transform);

            targeting.Refresh(enemy.transform.position, playerInside: true, enemyInside: true);

            Assert.AreSame(player.transform, targeting.SteerTarget);
            Assert.AreSame(player.transform, targeting.AttackTarget);
            Assert.AreEqual(EnemyTargetType.Player, targeting.CurrentTargetType);
        }
        finally
        {
            Object.DestroyImmediate(enemy);
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(barrier);
        }
    }

    [Test]
    public void Refresh_BrokenBarrierIngress_DoesNotRetargetBarrierWhenRegionFlickersOutside()
    {
        GameObject enemy = new GameObject("Enemy");
        GameObject player = new GameObject("Player");
        GameObject barrier = new GameObject("Barrier");
        BarrierHealth barrierHealth = barrier.AddComponent<BarrierHealth>();
        barrierHealth.TakeDamage(barrierHealth.MaxHealth);

        try
        {
            EnemyTargeting targeting = enemy.AddComponent<EnemyTargeting>();
            targeting.Debug_Setup(player.transform, barrier.transform);

            targeting.Refresh(new Vector2(0.5f, 0f), playerInside: true, enemyInside: false);
            targeting.Refresh(new Vector2(0.7f, 0f), playerInside: true, enemyInside: false);

            Assert.AreSame(player.transform, targeting.SteerTarget,
                "An enemy committed through a broken opening must not steer back to the barrier when the region boundary flickers.");
            Assert.AreEqual(EnemyTargetType.Player, targeting.CurrentTargetType);
        }
        finally
        {
            Object.DestroyImmediate(enemy);
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(barrier);
        }
    }

    [Test]
    public void Refresh_BrokenBarrierIngressCommitment_ClearsWhenPlayerLeavesCastle()
    {
        GameObject enemy = new GameObject("Enemy");
        GameObject player = new GameObject("Player");
        GameObject barrier = new GameObject("Barrier");
        BarrierHealth barrierHealth = barrier.AddComponent<BarrierHealth>();
        barrierHealth.TakeDamage(barrierHealth.MaxHealth);

        try
        {
            EnemyTargeting targeting = enemy.AddComponent<EnemyTargeting>();
            targeting.Debug_Setup(player.transform, barrier.transform);

            targeting.Refresh(new Vector2(0.5f, 0f), playerInside: true, enemyInside: false);
            targeting.RequestRetarget();
            targeting.Refresh(new Vector2(2f, 0f), playerInside: false, enemyInside: false);
            targeting.RequestRetarget();
            targeting.Refresh(new Vector2(2f, 0f), playerInside: true, enemyInside: false);

            Assert.AreSame(barrier.transform, targeting.SteerTarget,
                "A later siege approach must be allowed to target its home barrier again.");
            Assert.AreEqual(EnemyTargetType.Barrier, targeting.CurrentTargetType);
        }
        finally
        {
            Object.DestroyImmediate(enemy);
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(barrier);
        }
    }

    [Test]
    public void Refresh_RepairedBarrierAndEnemyOutside_RetargetsHomeBarrier()
    {
        GameObject enemy = new GameObject("Enemy");
        GameObject player = new GameObject("Player");
        GameObject barrier = new GameObject("Barrier");
        BarrierHealth barrierHealth = barrier.AddComponent<BarrierHealth>();
        barrierHealth.TakeDamage(barrierHealth.MaxHealth);

        try
        {
            EnemyTargeting targeting = enemy.AddComponent<EnemyTargeting>();
            targeting.Debug_Setup(player.transform, barrier.transform);

            targeting.Refresh(new Vector2(0.5f, 0f), playerInside: true, enemyInside: false);
            Assert.AreEqual(EnemyTargetType.Player, targeting.CurrentTargetType);

            barrierHealth.Repair();
            targeting.RequestRetarget();
            targeting.Refresh(new Vector2(2f, 0f), playerInside: true, enemyInside: false);

            Assert.AreSame(barrier.transform, targeting.SteerTarget);
            Assert.AreSame(barrier.transform, targeting.AttackTarget);
            Assert.AreEqual(EnemyTargetType.Barrier, targeting.CurrentTargetType);
        }
        finally
        {
            Object.DestroyImmediate(enemy);
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(barrier);
        }
    }

    [Test]
    public void Refresh_CachesBarrierUntilEnemyLocalRetargetIsRequested()
    {
        GameObject enemy = new GameObject("Enemy");
        GameObject player = new GameObject("Player");
        GameObject firstBarrier = new GameObject("FirstBarrier");
        firstBarrier.transform.position = Vector2.zero;
        BarrierHealth firstHealth = firstBarrier.AddComponent<BarrierHealth>();
        GameObject secondBarrier = new GameObject("SecondBarrier");
        secondBarrier.transform.position = Vector2.right * 10f;
        BarrierHealth secondHealth = secondBarrier.AddComponent<BarrierHealth>();

        try
        {
            EnemyTargeting targeting = enemy.AddComponent<EnemyTargeting>();
            targeting.Debug_Setup(player.transform);
            targeting.Debug_SetBarrierCandidates(new[] { firstHealth, secondHealth });

            targeting.RequestRetarget();
            targeting.Refresh(Vector2.left, playerInside: true, enemyInside: false);
            int initialRevision = targeting.TargetRevision;
            Assert.AreSame(firstBarrier.transform, targeting.AttackTarget);

            targeting.Refresh(Vector2.right * 9f, playerInside: true, enemyInside: false);

            Assert.AreSame(firstBarrier.transform, targeting.AttackTarget,
                "A valid cached barrier must not trigger a nearest-barrier scan each refresh.");
            Assert.That(targeting.TargetRevision, Is.EqualTo(initialRevision));

            targeting.RequestRetarget();
            targeting.Refresh(Vector2.right * 9f, playerInside: true, enemyInside: false);

            Assert.AreSame(secondBarrier.transform, targeting.AttackTarget,
                "An explicit enemy-local retarget should select the nearest barrier from the current position.");
            Assert.That(targeting.TargetRevision, Is.EqualTo(initialRevision + 1));
        }
        finally
        {
            Object.DestroyImmediate(enemy);
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(firstBarrier);
            Object.DestroyImmediate(secondBarrier);
        }
    }

}
