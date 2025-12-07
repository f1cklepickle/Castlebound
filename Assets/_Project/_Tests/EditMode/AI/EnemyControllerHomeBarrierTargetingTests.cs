using NUnit.Framework;
using UnityEngine;

public class EnemyControllerHomeBarrierTargetingTests
{
    [Test]
    public void RetainsHomeBarrierOutside_WhenBroken_ThenTargetsPlayerInside()
    {
        var barrier = new GameObject("BarrierHome");
        barrier.transform.position = new Vector2(-2f, 0f);
        var barrierHealth = barrier.AddComponent<BarrierHealth>();
        barrierHealth.TakeDamage(barrierHealth.MaxHealth); // broken

        var player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = Vector2.zero;

        var enemyGO = new GameObject("Enemy");
        enemyGO.transform.position = new Vector2(-8f, 0f);
        enemyGO.AddComponent<Rigidbody2D>();
        var controller = enemyGO.AddComponent<EnemyController2D>();
        controller.Debug_SetupRefs(player.transform, barrier.transform);

        // Outside, player inside: steering target should be the home barrier even though broken.
        var targetOutside = controller.Debug_SteerTarget(playerInside: true, enemyInside: false);
        Assert.AreSame(barrier.transform, targetOutside, "While outside, enemy should keep targeting its home barrier even if it is broken.");

        // Once inside, steering target switches to player.
        var targetInside = controller.Debug_SteerTarget(playerInside: true, enemyInside: true);
        Assert.AreSame(player.transform, targetInside, "After passing the barrier and being inside, enemy should target the player.");

        Object.DestroyImmediate(enemyGO);
        Object.DestroyImmediate(player);
        Object.DestroyImmediate(barrier);
    }
}
