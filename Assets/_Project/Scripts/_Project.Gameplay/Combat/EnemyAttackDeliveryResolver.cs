using UnityEngine;

public static class EnemyAttackDeliveryResolver
{
    public static IEnemyAttackDelivery Resolve(GameObject owner, ref MonoBehaviour deliverySource)
    {
        if (deliverySource is IEnemyAttackDelivery configuredDelivery)
            return configuredDelivery;

        var behaviours = owner.GetComponents<MonoBehaviour>();
        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is IEnemyAttackDelivery delivery)
            {
                deliverySource = behaviours[i];
                return delivery;
            }
        }

        var meleeDelivery = GetOrCreateMelee(owner);
        deliverySource = meleeDelivery;
        return meleeDelivery;
    }

    public static EnemyMeleeAttackDelivery GetOrCreateMelee(GameObject owner)
    {
        var meleeDelivery = owner.GetComponent<EnemyMeleeAttackDelivery>();
        return meleeDelivery != null
            ? meleeDelivery
            : owner.AddComponent<EnemyMeleeAttackDelivery>();
    }

    public static EnemyMeleeAttackDelivery ResolveMeleeForStats(
        GameObject owner,
        MonoBehaviour deliverySource)
    {
        if (deliverySource is IEnemyAttackDelivery &&
            !(deliverySource is EnemyMeleeAttackDelivery))
        {
            return null;
        }

        return GetOrCreateMelee(owner);
    }
}
