using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Castlebound.Gameplay.AI;
using System.Reflection;

public class EnemyRegionStateTests
{
    [Test]
    public void UpdatesState_FromTrackerEvents()
    {
        ClearSceneTrackers();
        var regionGO = new GameObject("RegionTracker");
        var collider = regionGO.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        var tracker = regionGO.AddComponent<CastleRegionTracker>();
        tracker.Debug_ForceInstanceForTests();

        var enemy = new GameObject("Enemy");
        enemy.AddComponent<Rigidbody2D>();
        var enemyCollider = enemy.AddComponent<BoxCollider2D>();
        enemyCollider.isTrigger = false;
        enemy.AddComponent<EnemyController2D>();
        var state = enemy.AddComponent<EnemyRegionState>();
        state.Debug_EnsureBound();

        var player = new GameObject("Player");
        player.tag = "Player";
        var playerCollider = player.AddComponent<BoxCollider2D>();
        playerCollider.isTrigger = false;

        var enterMethod = typeof(CastleRegionTracker).GetMethod(
            "OnTriggerEnter2D",
            BindingFlags.Instance | BindingFlags.NonPublic);
        var exitMethod = typeof(CastleRegionTracker).GetMethod(
            "OnTriggerExit2D",
            BindingFlags.Instance | BindingFlags.NonPublic);

        enterMethod.Invoke(tracker, new object[] { enemyCollider });
        enterMethod.Invoke(tracker, new object[] { playerCollider });

        Assert.IsTrue(state.EnemyInside, "Enemy should be marked inside after tracker enter.");
        Assert.IsTrue(state.PlayerInside, "Player should be marked inside after tracker enter.");

        exitMethod.Invoke(tracker, new object[] { enemyCollider });
        exitMethod.Invoke(tracker, new object[] { playerCollider });

        Assert.IsFalse(state.EnemyInside, "Enemy should be cleared on tracker exit.");
        Assert.IsFalse(state.PlayerInside, "Player should be cleared on tracker exit.");

        Object.DestroyImmediate(regionGO);
        Object.DestroyImmediate(enemy);
        Object.DestroyImmediate(player);
    }

    [Test]
    public void DefaultsOutside_WhenTrackerMissing()
    {
        ClearSceneTrackers();
        SetRegionTrackerInstance(null);
        EnemyRegionState.Debug_ResetMissingRegionWarning();

        LogAssert.Expect(LogType.Warning, "[EnemyRegionState] CastleRegionTracker.Instance is missing; treating enemy/player as outside.");

        var enemy = new GameObject("Enemy");
        enemy.AddComponent<Rigidbody2D>();
        enemy.AddComponent<EnemyController2D>();
        var state = enemy.AddComponent<EnemyRegionState>();
        state.Debug_EnsureBound();

        Assert.IsFalse(state.EnemyInside, "Missing tracker should default enemyInside to false.");
        Assert.IsFalse(state.PlayerInside, "Missing tracker should default playerInside to false.");

        Object.DestroyImmediate(enemy);
    }

    private static void SetRegionTrackerInstance(CastleRegionTracker value)
    {
        var backingField = typeof(CastleRegionTracker).GetField("<Instance>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic);
        if (backingField != null)
        {
            backingField.SetValue(null, value);
        }
    }

    private static void ClearSceneTrackers()
    {
        foreach (var tracker in Object.FindObjectsOfType<CastleRegionTracker>())
        {
            Object.DestroyImmediate(tracker.gameObject);
        }

        SetRegionTrackerInstance(null);
    }
}
