using UnityEngine;
using System.Collections.Generic;

public class BarrierGateRoot : MonoBehaviour
{
    [SerializeField] private Transform gatePoint;

    // Global registry (no per-frame Find calls)
    private static readonly List<BarrierGateRoot> all = new List<BarrierGateRoot>(16);
    public static System.Collections.Generic.IReadOnlyList<BarrierGateRoot> All => all;

    // Cached children
    private readonly List<BarrierSegment> segments = new List<BarrierSegment>(8);

    public Transform GatePoint => gatePoint ? gatePoint : transform;
    public bool IsOpen { get; private set; }

    void OnEnable()
    {
        if (!all.Contains(this)) all.Add(this);

        // Cache child BarrierSegments once on enable
        segments.Clear();
        GetComponentsInChildren(true, segments);

        // Subscribe to segment break events
        for (int i = 0; i < segments.Count; i++)
        {
            var s = segments[i];
            if (s) s.onBroken += HandleSegmentBroken;
        }

        // Initialize state
        IsOpen = false;
        for (int i = 0; i < segments.Count; i++)
        {
            var s = segments[i];
            if (s != null && s.IsBroken) { IsOpen = true; break; }
        }

        GateManager.NotifyGateChanged(this);
    }

    void OnDisable()
    {
        for (int i = 0; i < segments.Count; i++)
        {
            var s = segments[i];
            if (s) s.onBroken -= HandleSegmentBroken;
        }
        all.Remove(this);
        GateManager.NotifyGateChanged(this);
    }

    private void HandleSegmentBroken(BarrierSegment _)
    {
        if (!IsOpen)
        {
            IsOpen = true;
            GateManager.NotifyGateChanged(this); // event-driven retarget
        }
    }

    // Return nearest UNBROKEN segment (squared distance to avoid sqrt)
    public BarrierSegment NearestUnbroken(Vector2 from)
    {
        BarrierSegment best = null;
        float bestD = float.PositiveInfinity;

        for (int i = 0; i < segments.Count; i++)
        {
            var s = segments[i];
            if (s == null || s.IsBroken) continue;

            float d = ((Vector2)s.transform.position - from).sqrMagnitude;
            if (d < bestD) { bestD = d; best = s; }
        }
        return best;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = IsOpen ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.4f);
        if (GatePoint)
        {
            Gizmos.DrawSphere(GatePoint.position, 0.08f);
            Gizmos.DrawLine(transform.position, GatePoint.position);
        }
        
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.3f,
            IsOpen ? "Gate: OPEN" : "Gate: CLOSED");
#endif
    }
#endif
}
