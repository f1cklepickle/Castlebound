using UnityEngine;

public static class PlayerAttackRateCalculator
{
    public const float MinAttackRate = 0.1f;

    public static float ComputeEffectiveRate(float baseAttackRate, float weaponAttackSpeed)
    {
        var safeBaseRate = Mathf.Max(baseAttackRate, MinAttackRate);
        var safeWeaponSpeed = weaponAttackSpeed > 0f ? weaponAttackSpeed : 1f;
        return Mathf.Max(safeBaseRate * safeWeaponSpeed, MinAttackRate);
    }
}
