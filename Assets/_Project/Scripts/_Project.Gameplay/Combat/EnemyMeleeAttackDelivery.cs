using Castlebound.Gameplay.AI;
using UnityEngine;

public class EnemyMeleeAttackDelivery : MonoBehaviour, IEnemyAttackDelivery
{
    [SerializeField, Min(0)] private int damage = 1;
    [SerializeField] private FeedbackEventChannel enemyHitBarrierFeedbackChannel;

    private EnemyRegionState regionState;
    private EnemyRootReceiver rootReceiver;

    public EnemyAttackRole AttackRole => EnemyAttackRole.Melee;
    public int Damage { get => damage; set => damage = Mathf.Max(0, value); }

    public bool CanDeliver(Transform lockedTarget, EnemyEquipmentDefinition equipmentSnapshot)
    {
        return lockedTarget != null &&
               (equipmentSnapshot == null || equipmentSnapshot.IsCompatibleWith(AttackRole)) &&
               ResolveDamageable(lockedTarget) != null;
    }

    public bool TryDeliver(Transform lockedTarget, EnemyEquipmentDefinition equipmentSnapshot)
    {
        if (!CanDeliver(lockedTarget, equipmentSnapshot))
        {
            return false;
        }

        return TryDealDamage(ResolveDamageable(lockedTarget));
    }

    public bool TryDealDamage(IDamageable target)
    {
        if (target == null || damage <= 0 || IsRooted())
        {
            return false;
        }

        if (target is BarrierHealth barrier)
        {
            GetRegionState(out bool enemyInside, out bool playerInside);
            if (!EnemyAttack.CanDamageBarrier(enemyInside, playerInside))
            {
                return false;
            }

            target.TakeDamage(damage);
            enemyHitBarrierFeedbackChannel?.Raise(new FeedbackCue(
                FeedbackCueType.EnemyHitBarrier,
                barrier.transform.position,
                barrier.gameObject.GetInstanceID()));
            return true;
        }

        target.TakeDamage(damage);
        return true;
    }

    private static IDamageable ResolveDamageable(Transform lockedTarget)
    {
        if (lockedTarget == null)
        {
            return null;
        }

        var damageable = lockedTarget.GetComponent<IDamageable>();
        if (damageable != null)
        {
            return damageable;
        }

        damageable = lockedTarget.GetComponentInParent<IDamageable>();
        return damageable ?? lockedTarget.GetComponentInChildren<IDamageable>();
    }

    private void GetRegionState(out bool enemyInside, out bool playerInside)
    {
        if (regionState == null)
        {
            regionState = GetComponent<EnemyRegionState>();
        }

        enemyInside = regionState != null && regionState.EnemyInside;
        playerInside = regionState != null && regionState.PlayerInside;
    }

    private bool IsRooted()
    {
        if (rootReceiver == null)
        {
            rootReceiver = GetComponent<EnemyRootReceiver>();
        }

        return rootReceiver != null && rootReceiver.IsRooted;
    }
}
