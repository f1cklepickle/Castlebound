using Castlebound.Gameplay.Combat;
using UnityEngine;

public sealed class PlayerAttackRuntime
{
    private int configuredSwingCount;

    public void Tick(
        float deltaTime,
        int baseAttackDamage,
        float baseAttackRate,
        bool isFireHeld,
        PlayerWeaponController weaponController,
        PlayerAttackLoop attackLoop,
        PlayerAttackAnimationDriver animationDriver,
        Animator animator,
        GameObject hitboxObject)
    {
        CombatEquipmentSnapshot nextSnapshot = CaptureSnapshot(
            baseAttackDamage,
            baseAttackRate,
            weaponController);
        attackLoop?.Tick(deltaTime, nextSnapshot.AttackRate, isFireHeld);

        if (attackLoop != null && attackLoop.StartedSwingCount != configuredSwingCount)
        {
            configuredSwingCount = attackLoop.StartedSwingCount;
            ConfigureDeliverySnapshot(nextSnapshot, weaponController, hitboxObject);
        }

        ApplyHitboxState(attackLoop, hitboxObject);
        ApplyPresentation(
            baseAttackDamage,
            baseAttackRate,
            weaponController,
            attackLoop,
            animationDriver,
            animator);
    }

    public void Reset(
        int baseAttackDamage,
        float baseAttackRate,
        PlayerWeaponController weaponController,
        PlayerAttackLoop attackLoop,
        PlayerAttackAnimationDriver animationDriver,
        Animator animator,
        GameObject hitboxObject)
    {
        attackLoop?.ResetLoopState();
        configuredSwingCount = 0;
        hitboxObject?.GetComponent<Hitbox>()?.Deactivate();
        ApplyPresentation(
            baseAttackDamage,
            baseAttackRate,
            weaponController,
            attackLoop,
            animationDriver,
            animator);
    }

    private static CombatEquipmentSnapshot CaptureSnapshot(
        int baseAttackDamage,
        float baseAttackRate,
        PlayerWeaponController weaponController)
    {
        var profile = weaponController != null ? weaponController.ActiveCombatProfile : null;
        var baseStats = new CombatBaseStats(baseAttackDamage, baseAttackRate, 0f, 0f);
        var capabilities = CombatEquipmentCapability.MeleeDelivery
            | CombatEquipmentCapability.HandSocket;

        if (CombatEquipmentResolver.TryResolve(baseStats, capabilities, profile, out var snapshot))
            return snapshot;

        return new CombatEquipmentSnapshot(
            null,
            Mathf.Max(0, baseAttackDamage),
            AttackRatePolicy.Normalize(baseAttackRate),
            0f,
            0f,
            CombatEquipmentCapability.None,
            null,
            null,
            0f,
            0f,
            0f);
    }

    private static void ConfigureDeliverySnapshot(
        CombatEquipmentSnapshot equipmentSnapshot,
        PlayerWeaponController weaponController,
        GameObject hitboxObject)
    {
        if (hitboxObject == null)
            return;

        var weaponStats = weaponController != null
            ? weaponController.CurrentWeaponStats
            : default;
        hitboxObject.GetComponent<Hitbox>()?.ConfigureSwing(equipmentSnapshot, weaponStats);
    }

    private static void ApplyHitboxState(PlayerAttackLoop attackLoop, GameObject hitboxObject)
    {
        if (hitboxObject == null)
            return;

        var hitbox = hitboxObject.GetComponent<Hitbox>();
        if (attackLoop != null && attackLoop.ShouldKeepHitboxActiveThisStep)
            hitbox?.Activate();
        else
            hitbox?.Deactivate();
    }

    private static void ApplyPresentation(
        int baseAttackDamage,
        float baseAttackRate,
        PlayerWeaponController weaponController,
        PlayerAttackLoop attackLoop,
        PlayerAttackAnimationDriver animationDriver,
        Animator animator)
    {
        if (animationDriver == null)
            return;

        float rate = attackLoop != null && attackLoop.IsSwingActive
            ? attackLoop.CurrentAttackRate
            : CaptureSnapshot(baseAttackDamage, baseAttackRate, weaponController).AttackRate;
        bool isSwingActive = attackLoop != null && attackLoop.IsPresentationActive;
        float progress = attackLoop != null ? attackLoop.NormalizedSwingProgress : 0f;
        animationDriver.ApplyLoopPresentation(
            animator,
            isSwingActive,
            progress,
            rate,
            baseAttackRate);
    }
}
