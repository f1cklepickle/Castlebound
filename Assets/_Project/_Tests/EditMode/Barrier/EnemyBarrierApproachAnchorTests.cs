using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.AI;

namespace Castlebound.Tests.Barrier
{
    public class EnemyBarrierApproachAnchorTests
    {
        [Test]
        public void EntersHold_WhenApproachingFromSide_UsesAnchorPosition()
        {
            // Arrange: barrier with an approach anchor offset in front (to the west).
            var barrierGO = new GameObject("Barrier");
            var anchor = new GameObject("Anchor").transform;
            anchor.SetParent(barrierGO.transform);
            anchor.localPosition = new Vector2(-1.5f, 0f);

            var barrierHealth = barrierGO.AddComponent<BarrierHealth>();
            var holdBehavior = barrierGO.AddComponent<EnemyBarrierHoldBehavior>();
            holdBehavior.Debug_SetAnchor(anchor);
            holdBehavior.Debug_SetHoldRadius(1f);

            var enemyGO = new GameObject("Enemy");
            // Place enemy so it's inside anchor radius (1.0) but outside the barrier center radius.
            enemyGO.transform.position = new Vector2(-2.4f, 0f);

            // Act: check if hold is allowed using anchor position via the hold behavior.
            bool canHold = holdBehavior.Debug_CanHold(enemyGO.transform.position);

            // Assert: enemy should be allowed to hold because anchor is closer than barrier center.
            Assert.IsTrue(canHold, "Enemy approaching from the side should enter hold using the approach anchor position.");

            Object.DestroyImmediate(enemyGO);
            Object.DestroyImmediate(barrierGO);
        }
    }
}
