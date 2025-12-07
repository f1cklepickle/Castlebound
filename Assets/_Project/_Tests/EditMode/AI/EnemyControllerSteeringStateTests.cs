using NUnit.Framework;
using UnityEngine;

public class EnemyControllerSteeringStateTests
{
    [Test]
    public void SteersToHomeBarrier_Outside_AndPlayer_Inside()
    {
        var barrier = new GameObject("BarrierHome");
        barrier.transform.position = new Vector2(-2f, 0f);
        barrier.AddComponent<BarrierHealth>(); // intact barrier

        var player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = Vector2.zero;

        var enemyGO = new GameObject("Enemy");
        enemyGO.transform.position = new Vector2(-8f, 0f);
        enemyGO.AddComponent<Rigidbody2D>();
        var controller = enemyGO.AddComponent<EnemyController2D>();
        controller.Debug_SetupRefs(player.transform, barrier.transform);

        // Outside, player inside -> steer to home barrier.
        var outsideTarget = controller.Debug_SteerTarget(playerInside: true, enemyInside: false);
        Assert.AreSame(barrier.transform, outsideTarget, "Outside with player inside should steer to home barrier.");

        // Inside -> steer to player.
        var insideTarget = controller.Debug_SteerTarget(playerInside: true, enemyInside: true);
        Assert.AreSame(player.transform, insideTarget, "Once inside, steering should switch to player.");

        Object.DestroyImmediate(enemyGO);
        Object.DestroyImmediate(player);
        Object.DestroyImmediate(barrier);
    }
}
