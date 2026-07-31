using System.Collections;
using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Castlebound.Tests.Gate
{
    public class BarrierRepairEnemyRetargetPlayTests
    {
        [UnityTest]
        public IEnumerator Repair_WhenEnemyIsExpelled_ReconcilesRegionAndRetargetsBarrier()
        {
            var regionObject = new GameObject("CastleRegion");
            var region = regionObject.AddComponent<CastleRegionTracker>();
            region.Debug_ForceInstanceForTests();
            region.Debug_SetPlayerInsideForTests(true);

            var playerObject = new GameObject("Player");
            playerObject.tag = "Player";

            var barrierObject = new GameObject("Barrier");
            var barrierCollider = barrierObject.AddComponent<BoxCollider2D>();
            barrierCollider.size = new Vector2(2f, 2f);
            barrierObject.AddComponent<SpriteRenderer>();

            var anchorObject = new GameObject("ApproachAnchor");
            anchorObject.transform.position = new Vector2(2f, 0f);
            var hold = barrierObject.AddComponent<EnemyBarrierHoldBehavior>();
            hold.Debug_SetAnchor(anchorObject.transform);

            var barrierHealth = barrierObject.AddComponent<BarrierHealth>();
            barrierHealth.TakeDamage(barrierHealth.MaxHealth);

            var enemyObject = new GameObject("Enemy");
            enemyObject.tag = "Enemy";
            enemyObject.layer = LayerMask.NameToLayer("Enemies");
            var body = enemyObject.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            var enemyCollider = enemyObject.AddComponent<CircleCollider2D>();
            enemyCollider.radius = 0.4f;
            var controller = enemyObject.AddComponent<EnemyController2D>();
            controller.Debug_SetupRefs(playerObject.transform, barrierObject.transform);
            var regionState = enemyObject.AddComponent<EnemyRegionState>();
            regionState.Debug_EnsureBound();

            region.Debug_SetEnemyInsideForTests(controller, true);
            enemyObject.transform.position = new Vector2(1.2f, 0f);
            Physics2D.SyncTransforms();

            Assert.IsTrue(regionState.EnemyInside, "Precondition: enemy should be recorded inside.");
            Assert.AreSame(
                playerObject.transform,
                controller.Debug_SelectTarget(playerInside: true, enemyInside: true),
                "Precondition: an inside enemy should target the player.");

            barrierHealth.Repair();
            yield return new WaitForFixedUpdate();

            Assert.IsFalse(regionState.EnemyInside, "An enemy expelled by repair must be reconciled outside.");
            Assert.AreSame(
                barrierObject.transform,
                controller.Target,
                "The expelled enemy should reacquire its repaired home barrier.");

            Object.Destroy(enemyObject);
            Object.Destroy(barrierObject);
            Object.Destroy(anchorObject);
            Object.Destroy(playerObject);
            Object.Destroy(regionObject);
        }

        [UnityTest]
        public IEnumerator RepairExpulsion_WhenEnemyWasNeverRegisteredInside_StillRetargetsBarrier()
        {
            var regionObject = new GameObject("CastleRegion");
            var region = regionObject.AddComponent<CastleRegionTracker>();
            region.Debug_ForceInstanceForTests();
            region.Debug_SetPlayerInsideForTests(true);

            var playerObject = new GameObject("Player");
            playerObject.tag = "Player";

            var barrierObject = new GameObject("Barrier");
            var barrierHealth = barrierObject.AddComponent<BarrierHealth>();
            barrierHealth.TakeDamage(barrierHealth.MaxHealth);

            var enemyObject = new GameObject("Enemy");
            enemyObject.tag = "Enemy";
            var body = enemyObject.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            var controller = enemyObject.AddComponent<EnemyController2D>();
            controller.Debug_SetupRefs(playerObject.transform, barrierObject.transform);
            var regionState = enemyObject.AddComponent<EnemyRegionState>();
            regionState.Debug_EnsureBound();

            Assert.IsFalse(region.EnemyInside(controller),
                "Precondition: the partially advanced enemy was never registered inside.");
            Assert.AreSame(
                playerObject.transform,
                controller.Debug_SelectTarget(playerInside: true, enemyInside: false),
                "Precondition: the enemy committed through the nearby broken barrier toward the player.");

            barrierHealth.Repair();
            region.ReconcileEnemyOutsideAfterBarrierRepair(controller);
            yield return new WaitForFixedUpdate();

            Assert.IsFalse(regionState.EnemyInside,
                "Repair reconciliation must keep an unregistered expelled enemy outside.");
            Assert.AreSame(barrierObject.transform, controller.Target,
                "The expelled outside enemy must target the repaired barrier instead of retaining the player.");

            Object.Destroy(enemyObject);
            Object.Destroy(barrierObject);
            Object.Destroy(playerObject);
            Object.Destroy(regionObject);
        }
    }
}
