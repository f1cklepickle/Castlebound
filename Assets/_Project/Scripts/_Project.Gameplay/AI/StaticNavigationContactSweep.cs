using System;
using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    // Exact straight swept-circle volume, including starting overlaps. Never skip an entire
    // collider merely because the body initially touches one of its faces.
    internal sealed class StaticNavigationContactSweep : IDisposable
    {
        internal const float ContactTolerance = 0.001f;
        private readonly StaticNavigationWorld2D world;
        private readonly Collider2D[] overlaps;
        private readonly GameObject probeObject;
        private readonly CapsuleCollider2D probe;
#if UNITY_EDITOR
        internal bool DebugCaptureEnabled { get; set; }
        internal string DebugLastDecision { get; private set; }
#endif

        internal StaticNavigationContactSweep(StaticNavigationWorld2D world, int queryCapacity)
        {
            this.world = world;
            overlaps = new Collider2D[queryCapacity];
            probeObject = new GameObject("Navigation contact query") { hideFlags = HideFlags.HideAndDontSave };
            probe = probeObject.AddComponent<CapsuleCollider2D>();
            probe.enabled = false;
            probe.isTrigger = true;
            probe.excludeLayers = ~0;
            probe.layerOverridePriority = int.MaxValue;
            probe.direction = CapsuleDirection2D.Vertical;
        }

        internal StaticNavigationSampleState Sample(Vector2 start, Vector2 end, Func<bool> spendQuery = null)
        {
            Record("begin");
            if (spendQuery != null && !spendQuery()) return StaticNavigationSampleState.Unknown;
            Vector2 delta = end - start;
            float length = delta.magnitude;
            if (length < 0.000001f)
            {
                Record("zero-length point check");
                return world.SamplePoint(start);
            }
            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg - 90f;
            Vector2 middle = (start + end) * 0.5f;
            float broadRadius = world.ClearanceRadius + ContactTolerance;
            int count = Physics2D.OverlapCapsule(middle,
                new Vector2(2f * broadRadius, 2f * broadRadius + length), CapsuleDirection2D.Vertical,
                angle, world.ObstacleFilter, overlaps);
            if (count == overlaps.Length)
            {
                Record("overlap buffer saturated");
                return StaticNavigationSampleState.Unknown;
            }

            bool touchingMargin = false;
            for (int i = 0; i < count; i++)
            {
                var obstacle = overlaps[i];
                if (!world.IsObstacle(obstacle)) continue;
                Vector2 outward = start - obstacle.ClosestPoint(start);
                float skin = PolygonContactSkin(obstacle);
                float separation = outward.magnitude + skin - world.BodyRadius;
                // ClosestPoint includes polygon contact skin. Remove that same skin from both
                // signed clearance checks, without reducing the body's physical radius.
                // A point inside the collider has no usable outward normal; never depenetrate it.
                if (outward.sqrMagnitude == 0f || separation < -ContactTolerance)
                {
                    Record("start penetration", obstacle, separation, skin: skin);
                    return StaticNavigationSampleState.Blocked;
                }
                if (separation > world.ClearanceMargin + ContactTolerance) continue;
                touchingMargin = true;
                float inwardDot = Vector2.Dot(delta, outward.normalized);
                if (inwardDot < -0.000001f)
                {
                    Record("inward direction", obstacle, separation, skin: skin, inwardDot: inwardDot);
                    return StaticNavigationSampleState.Blocked;
                }
            }

            // Disabled outside this synchronous query; no simulation step or actor movement occurs.
            probeObject.transform.SetPositionAndRotation(middle, Quaternion.Euler(0f, 0f, angle));
            float radius = touchingMargin ? world.BodyRadius : world.ClearanceRadius;
            probe.size = new Vector2(2f * radius, 2f * radius + length);
            probe.enabled = true;
            try
            {
                Physics2D.SyncTransforms();
                for (int i = 0; i < count; i++)
                {
                    var obstacle = overlaps[i];
                    if (!world.IsObstacle(obstacle)) continue;
                    if (spendQuery != null && !spendQuery()) return StaticNavigationSampleState.Unknown;
                    ColliderDistance2D swept = Physics2D.Distance(probe, obstacle);
                    if (!swept.isValid)
                    {
                        Record("invalid collider distance", obstacle);
                        return StaticNavigationSampleState.Unknown;
                    }
                    // ClosestPoint AND Distance include polygon skin. Comparing the returned
                    // witness to ClosestPoint cannot measure it: the witness is already on it.
                    float skin = PolygonContactSkin(obstacle);
                    float geometricDistance = swept.distance + skin;
                    float separation = (start - obstacle.ClosestPoint(start)).magnitude + skin - world.BodyRadius;
                    if (!touchingMargin && geometricDistance <= 0f)
                    {
                        Record("normal-margin sweep blocked", obstacle, separation, swept.distance, geometricDistance, skin);
                        return StaticNavigationSampleState.Blocked;
                    }
                    // The capsule is the WHOLE connector, not a sequence of sparse point tests.
                    // Tiny initial numerical overlap cannot grow while traversing the connector.
                    if (geometricDistance < -ContactTolerance)
                    {
                        Record("sweep penetration", obstacle, separation, swept.distance, geometricDistance, skin);
                        return StaticNavigationSampleState.Blocked;
                    }
                    if (geometricDistance < Mathf.Min(0f, separation) - ContactTolerance * 0.1f)
                    {
                        Record("sweep deepens contact", obstacle, separation, swept.distance, geometricDistance, skin);
                        return StaticNavigationSampleState.Blocked;
                    }
                }
                Record("clear");
                return StaticNavigationSampleState.Clear;
            }
            finally { probe.enabled = false; }
        }

        internal static float PolygonContactSkin(Collider2D obstacle)
        {
            // Only standard, unrounded polygon geometry gets the implicit contact-offset
            // correction. Circles/capsules and authored edge radii are physical geometry.
            // Unsupported/rounded shapes stay conservative rather than eroding their boundary.
            // Physics contact settings must remain unchanged while the sampled world is in use.
            if (obstacle is BoxCollider2D box)
                return box.edgeRadius == 0f ? Physics2D.defaultContactOffset : 0f;
            if (obstacle is PolygonCollider2D || obstacle is UnityEngine.Tilemaps.TilemapCollider2D)
                return Physics2D.defaultContactOffset;
            if (obstacle is CompositeCollider2D composite &&
                composite.geometryType == CompositeCollider2D.GeometryType.Polygons && composite.edgeRadius == 0f)
                return Physics2D.defaultContactOffset;
            return 0f;
        }

        // Opt-in EditMode diagnostics only. No logging and no capture in normal use or player builds.
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void Record(string reason, Collider2D obstacle = null, float startGap = float.NaN,
            float rawSweep = float.NaN, float correctedSweep = float.NaN, float skin = float.NaN,
            float inwardDot = float.NaN)
        {
#if UNITY_EDITOR
            if (!DebugCaptureEnabled) return;
            DebugLastDecision = $"{reason}; obstacle={obstacle?.name}; type={obstacle?.GetType().Name}; " +
                $"bodyRadius={world.BodyRadius:R}; margin={world.ClearanceMargin:R}; startGap={startGap:R}; " +
                $"rawSweep={rawSweep:R}; correctedSweep={correctedSweep:R}; obstacleSkin={skin:R}; " +
                $"inwardDot={inwardDot:R}; contactOffset={Physics2D.defaultContactOffset:R}";
#endif
        }

        public void Dispose()
        {
            if (probeObject == null) return;
            probe.enabled = false;
            if (Application.isPlaying) UnityEngine.Object.Destroy(probeObject);
            else UnityEngine.Object.DestroyImmediate(probeObject);
        }
    }
}
