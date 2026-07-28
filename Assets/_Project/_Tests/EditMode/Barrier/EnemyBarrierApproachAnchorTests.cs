using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.AI;

namespace Castlebound.Tests.Barrier
{
    public class EnemyBarrierApproachAnchorTests
    {
        [Test]
        public void DistanceToAnchor_UsesConfiguredApproachPosition()
        {
            // Arrange: barrier with an approach anchor offset in front (to the west).
            var barrierGO = new GameObject("Barrier");
            var anchor = new GameObject("Anchor").transform;
            anchor.SetParent(barrierGO.transform);
            anchor.localPosition = new Vector2(-1.5f, 0f);

            var holdBehavior = barrierGO.AddComponent<EnemyBarrierHoldBehavior>();
            holdBehavior.Debug_SetAnchor(anchor);

            var enemyGO = new GameObject("Enemy");
            // Place enemy so it's inside anchor radius (1.0) but outside the barrier center radius.
            enemyGO.transform.position = new Vector2(-2.4f, 0f);

            float distance = holdBehavior.DistanceToAnchor(enemyGO.transform.position);

            Assert.That(distance, Is.EqualTo(0.9f).Within(0.001f),
                "Barrier approach geometry should retain its configured anchor independently of engagement tuning.");

            Object.DestroyImmediate(enemyGO);
            Object.DestroyImmediate(barrierGO);
        }
    }
}
