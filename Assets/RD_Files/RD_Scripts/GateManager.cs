using UnityEngine;
using System;

public static class GateManager
{
    public static event Action onGatesChanged;

    // Fast check without LINQ
    public static bool AnyGateOpen()
    {
        var list = BarrierGateRoot.All;
        for (int i = 0; i < list.Count; i++)
            if (list[i].IsOpen) return true;
        return false;
    }

    public static BarrierSegment NearestClosedBarrier(Vector2 from)
    {
        BarrierSegment best = null;
        float bestD = float.PositiveInfinity;

        var gates = BarrierGateRoot.All;
        for (int i = 0; i < gates.Count; i++)
        {
            var seg = gates[i].NearestUnbroken(from);
            if (!seg) continue;

            float d = ((Vector2)seg.transform.position - from).sqrMagnitude;
            if (d < bestD) { bestD = d; best = seg; }
        }
        return best;
    }

    // Called by gates on enable/disable/state-change
    public static void NotifyGateChanged(BarrierGateRoot _)
    {
        onGatesChanged?.Invoke();
    }
}

