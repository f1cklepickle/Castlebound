using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Castlebound.Gameplay.AI;

namespace Castlebound.Tests.PlayMode.AI
{
    public class EnemyRegionStatePlayTests
    {
        [UnityTest]
        public IEnumerator UpdatesInsideState_WhenCrossingRegionTrigger()
        {
            var regionGO = new GameObject("CastleRegion");
            var regionCollider = regionGO.AddComponent<BoxCollider2D>();
            regionCollider.isTrigger = true;
            regionCollider.size = new Vector2(4f, 4f);
            regionGO.AddComponent<CastleRegionTracker>();

            var player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = Vector3.zero;

            var enemy = new GameObject("Enemy");
            var rb = enemy.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            enemy.AddComponent<BoxCollider2D>();
            enemy.AddComponent<EnemyController2D>();
            var state = enemy.AddComponent<EnemyRegionState>();

            enemy.transform.position = new Vector2(10f, 0f);
            yield return new WaitForFixedUpdate();

            Assert.IsFalse(state.EnemyInside, "Enemy should start outside the region.");

            enemy.transform.position = Vector2.zero;
            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.IsTrue(state.EnemyInside, "Enemy should be marked inside after entering region.");

            enemy.transform.position = new Vector2(10f, 0f);
            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.IsFalse(state.EnemyInside, "Enemy should be marked outside after exiting region.");

            Object.DestroyImmediate(regionGO);
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(enemy);
        }
    }
}
