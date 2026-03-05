using UnityEngine;

public sealed class PlayerAttackCooldownGate
{
    private float _nextAllowedAttackTime;

    public bool TryConsume(float currentTime, float attacksPerSecond)
    {
        var safeRate = Mathf.Max(attacksPerSecond, PlayerAttackRateCalculator.MinAttackRate);

        if (currentTime < _nextAllowedAttackTime)
            return false;

        _nextAllowedAttackTime = currentTime + (1f / safeRate);
        return true;
    }

    public void Reset()
    {
        _nextAllowedAttackTime = 0f;
    }
}
