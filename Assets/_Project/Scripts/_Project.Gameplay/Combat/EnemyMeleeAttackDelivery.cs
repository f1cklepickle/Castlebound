using Castlebound.Gameplay.AI;
using Castlebound.Gameplay.Combat;
using UnityEngine;

public class EnemyMeleeAttackDelivery : MonoBehaviour, IEnemyAttackDelivery
{
    [SerializeField, Min(0)] private int damage = 1;
    [SerializeField] private FeedbackEventChannel enemyHitBarrierFeedbackChannel;
    [SerializeField] private EnemyStaggerReceiver staggerReceiver;

    private EnemyRegionState regionState;
    private EnemyRootReceiver rootReceiver;

    public EnemyAttackRole AttackRole => EnemyAttackRole.Melee;
    public int Damage { get => damage; set => damage = Mathf.Max(0, value); }

    public bool CanDeliver(
        Transform lockedTarget,
        EnemyEquipmentDefinition equipmentDefinitionSnapshot,
        CombatEquipmentSnapshot combatEquipmentSnapshot)
    {
        return lockedTarget != null &&
               !IsActionLocked() &&
               (equipmentDefinitionSnapshot == null || equipmentDefinitionSnapshot.IsCompatibleWith(AttackRole)) &&
               ResolveDamageable(lockedTarget) != null;
    }

    public bool TryDeliver(
        Transform lockedTarget,
        EnemyEquipmentDefinition equipmentDefinitionSnapshot,
        CombatEquipmentSnapshot combatEquipmentSnapshot)
    {
        if (!CanDeliver(lockedTarget, equipmentDefinitionSnapshot, combatEquipmentSnapshot))
        {
            return false;
        }

        return TryDealDamage(ResolveDamageable(lockedTarget), combatEquipmentSnapshot.Damage);
    }

    public bool CanDeliver(Transform lockedTarget, EnemyEquipmentDefinition equipmentSnapshot)
    {
        return CanDeliver(lockedTarget, equipmentSnapshot, default);
    }

    public bool TryDeliver(Transform lockedTarget, EnemyEquipmentDefinition equipmentSnapshot)
    {
        if (!CanDeliver(lockedTarget, equipmentSnapshot, default))
            return false;
        return TryDealDamage(ResolveDamageable(lockedTarget));
    }

    public bool TryDealDamage(IDamageable target)
    {
        return TryDealDamage(target, damage);
    }

    private bool TryDealDamage(IDamageable target, int resolvedDamage)
    {
        if (target == null || resolvedDamage <= 0 || IsRooted() || IsActionLocked())
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

            target.TakeDamage(resolvedDamage);
            enemyHitBarrierFeedbackChannel?.Raise(new FeedbackCue(
                FeedbackCueType.EnemyHitBarrier,
                barrier.transform.position,
                barrier.gameObject.GetInstanceID()));
            return true;
        }

        if (TryResolvePlayerHitReceiver(target, out var playerHitReceiver))
        {
            PlayerHitResult result = playerHitReceiver.ReceiveHit(new PlayerHitRequest(
                resolvedDamage,
                gameObject,
                transform.position,
                CombatDamageType.Melee));
            if (result.Outcome == PlayerHitOutcome.Parried)
                ((IEnemyStaggerReceiver)staggerReceiver)?.TryStagger();
            return true;
        }

        target.TakeDamage(resolvedDamage);
        return true;
    }

    private static bool TryResolvePlayerHitReceiver(
        IDamageable target,
        out IPlayerHitReceiver playerHitReceiver)
    {
        playerHitReceiver = null;
        if (!(target is Component targetComponent))
            return false;

        playerHitReceiver = targetComponent.GetComponent<IPlayerHitReceiver>();
        if (playerHitReceiver != null)
            return true;

        playerHitReceiver = targetComponent.GetComponentInParent<IPlayerHitReceiver>();
        if (playerHitReceiver != null)
            return true;

        playerHitReceiver = targetComponent.GetComponentInChildren<IPlayerHitReceiver>();
        return playerHitReceiver != null;
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

    private bool IsActionLocked()
    {
        return staggerReceiver != null && staggerReceiver.IsActionLocked;
    }
}
