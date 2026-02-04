using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.AI;

public class CastleRegionTrackerEventsTests
{
    [Test]
    public void RaisesEvents_OnTriggerEnterAndExit_ForPlayerAndEnemy()
    {
        var regionGO = new GameObject("RegionTracker");
        var collider = regionGO.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        var tracker = regionGO.AddComponent<CastleRegionTracker>();

        int playerEntered = 0;
        int playerExited = 0;
        int enemyEntered = 0;
        int enemyExited = 0;

        tracker.OnPlayerEntered += () => playerEntered++;
        tracker.OnPlayerExited += () => playerExited++;
        tracker.OnEnemyEntered += _ => enemyEntered++;
        tracker.OnEnemyExited += _ => enemyExited++;

        var playerGO = new GameObject("Player");
        playerGO.tag = "Player";
        var playerCollider = playerGO.AddComponent<BoxCollider2D>();
        playerCollider.isTrigger = false;

        var enemyGO = new GameObject("Enemy");
        var enemyCollider = enemyGO.AddComponent<BoxCollider2D>();
        enemyCollider.isTrigger = false;
        enemyGO.AddComponent<EnemyController2D>();

        var enterMethod = typeof(CastleRegionTracker).GetMethod(
            "OnTriggerEnter2D",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var exitMethod = typeof(CastleRegionTracker).GetMethod(
            "OnTriggerExit2D",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        enterMethod.Invoke(tracker, new object[] { playerCollider });
        enterMethod.Invoke(tracker, new object[] { enemyCollider });

        Assert.That(playerEntered, Is.EqualTo(1), "Expected player enter event.");
        Assert.That(enemyEntered, Is.EqualTo(1), "Expected enemy enter event.");

        exitMethod.Invoke(tracker, new object[] { playerCollider });
        exitMethod.Invoke(tracker, new object[] { enemyCollider });

        Assert.That(playerExited, Is.EqualTo(1), "Expected player exit event.");
        Assert.That(enemyExited, Is.EqualTo(1), "Expected enemy exit event.");

        Object.DestroyImmediate(regionGO);
        Object.DestroyImmediate(playerGO);
        Object.DestroyImmediate(enemyGO);
    }
}
