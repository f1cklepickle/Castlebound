using NUnit.Framework;

public class EnemyBarrierAttackGateTests
{
    [Test]
        public void AllowsBarrierDamage_WhenEnemyOutside()
        {
            bool canDamage = EnemyAttack.CanDamageBarrier(
                enemyInside: false,
                playerInside: false);

        Assert.IsTrue(canDamage, "Enemy outside should be allowed to damage barriers.");
    }

    [Test]
    public void BlocksBarrierDamage_WhenEnemyInside_PlayerInside()
    {
        bool canDamage = EnemyAttack.CanDamageBarrier(
            enemyInside: true,
            playerInside: true);

        Assert.IsFalse(canDamage, "Enemy inside should not damage barriers when the player is inside.");
    }
}
