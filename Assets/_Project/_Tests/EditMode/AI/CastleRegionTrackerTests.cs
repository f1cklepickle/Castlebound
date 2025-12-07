using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.AI;

public class CastleRegionTrackerTests
{
    [Test]
    public void RegistersPlayerAndEnemy_OnTriggerEnter_AndClears_OnExit()
    {
        var regionGO = new GameObject("RegionTracker");
        var collider = regionGO.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        var tracker = regionGO.AddComponent<CastleRegionTracker>();

        var playerGO = new GameObject("Player");
        playerGO.tag = "Player";
        var playerCollider = playerGO.AddComponent<BoxCollider2D>();
        playerCollider.isTrigger = false;

        var enemyGO = new GameObject("Enemy");
        var enemyCollider = enemyGO.AddComponent<BoxCollider2D>();
        enemyCollider.isTrigger = false;
        var enemyController = enemyGO.AddComponent<EnemyController2D>();

        // Simulate entering region using reflection (OnTriggerEnter2D is private).
        var enterMethod = typeof(CastleRegionTracker).GetMethod("OnTriggerEnter2D", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        enterMethod.Invoke(tracker, new object[] { playerCollider });
        enterMethod.Invoke(tracker, new object[] { enemyCollider });

        Assert.IsTrue(tracker.PlayerInside, "Player should be marked inside after entering trigger.");
        Assert.IsTrue(tracker.EnemyInside(enemyController), "Enemy should be tracked inside after entering trigger.");

        // Simulate exiting region.
        var exitMethod = typeof(CastleRegionTracker).GetMethod("OnTriggerExit2D", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        exitMethod.Invoke(tracker, new object[] { playerCollider });
        exitMethod.Invoke(tracker, new object[] { enemyCollider });

        Assert.IsFalse(tracker.PlayerInside, "Player should be marked outside after exiting trigger.");
        Assert.IsFalse(tracker.EnemyInside(enemyController), "Enemy should be removed after exiting trigger.");

        Object.DestroyImmediate(regionGO);
        Object.DestroyImmediate(playerGO);
        Object.DestroyImmediate(enemyGO);
    }
}
