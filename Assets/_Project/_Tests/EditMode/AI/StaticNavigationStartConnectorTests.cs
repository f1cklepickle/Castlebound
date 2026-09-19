using System.Collections.Generic;
using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.AI
{
    public class StaticNavigationStartConnectorTests
    {
        private const float MeleeRadius = 0.87684506f;
        private readonly List<GameObject> objects = new List<GameObject>();
        private Grid authority;
        private StaticNavigationWorld2D world;
        private StaticNavigationWorldCache cache;
        private StaticNavigationStartConnector connector;
        private static Vector2 World(float x, float y) => new Vector2(100f + x, 100f + y);

        [SetUp] public void SetUp()
        {
            authority = new GameObject("WorldGrid", typeof(Grid)).GetComponent<Grid>();
            authority.transform.position = World(0f, 0f);
            Configure(MeleeRadius);
        }
        [TearDown] public void TearDown()
        {
            connector.Dispose();
            foreach (var item in objects) Object.DestroyImmediate(item);
            objects.Clear();
            Object.DestroyImmediate(authority.gameObject);
        }

        [TestCase(0f)]
        [TestCase(-0.0003f)]
        public void NumericalWallContact_CanEscapeOutward_WithoutShrinkingBody(float gap)
        {
            Box(-1f, 0f, 2f, 10f);
            var start = World(MeleeRadius + gap, 0.125f);
            Assert.That(world.SamplePoint(start), Is.EqualTo(StaticNavigationSampleState.Blocked));
            Assert.That(Check(start, new Vector2Int(4, 0)), Is.EqualTo(StaticNavigationSampleState.Clear), connector.DebugLastDecision);
        }

        [Test]
        public void TangentialEscapePastWallEnd_IsAllowed_WhenDestinationHasNormalMargin()
        {
            Box(0.875f - MeleeRadius - 1f, -0.875f, 2f, 2f);
            var start = World(0.875f, 0.125f);
            Assert.That(Check(start, new Vector2Int(3, 1)), Is.EqualTo(StaticNavigationSampleState.Clear), connector.DebugLastDecision);
            Assert.That(cache.GetCellState(new Vector2Int(3, 1)), Is.EqualTo(StaticNavigationSampleState.Clear));
        }

        [Test]
        public void InwardMotionIsRejected_EvenIfItEndsBeyondFiniteWallCorner()
        {
            Configure(0.25f);
            // Right face .75, top .25; body begins touching at (1,.25).
            Box(-0.25f, -0.75f, 2f, 2f);
            // A .125 inward, .375 upward connector has a clear endpoint yet points into the face.
            var start = World(1f, 0.25f);
            var destination = new Vector2Int(3, 2);
            Assert.That(world.SamplePoint(cache.CellToWorld(destination)), Is.EqualTo(StaticNavigationSampleState.Clear));
            Assert.That(Check(start, destination), Is.EqualTo(StaticNavigationSampleState.Blocked));
        }

        [TestCase(0.01f)]
        [TestCase(0.003f)]
        public void RealPenetrationIsRejected_InsteadOfDepenetrating(float depth)
        {
            Box(-1f, 0f, 2f, 10f);
            Assert.That(Check(World(MeleeRadius - depth, 0.125f), new Vector2Int(4, 0)),
                Is.EqualTo(StaticNavigationSampleState.Blocked));
        }

        [Test]
        public void DestinationMustHaveNormalClearance_UnknownAndMarginBlockedNeverSucceed()
        {
            // Cell (.875,.125) fits the physical body but not the extra .02 margin.
            Box(-1.01f, 0f, 2f, 10f);
            var start = World(MeleeRadius - 0.01f, 0.125f);
            Assert.That(connector.CheckConnection(start, new Vector2Int(4, 0)), Is.EqualTo(StaticNavigationSampleState.Unknown));
            Assert.That(Check(start, new Vector2Int(3, 0)), Is.EqualTo(StaticNavigationSampleState.Blocked));
        }

        [Test]
        public void SweepRejectsCrossingAnotherFaceOfSameConcaveCollider()
        {
            Configure(0.1f);
            var item = new GameObject("Concave wall", typeof(PolygonCollider2D));
            objects.Add(item);
            item.transform.position = World(0f, 0f);
            item.GetComponent<PolygonCollider2D>().points = new[] {
                new Vector2(-0.2f, -0.5f), new Vector2(0.4f, -0.5f), new Vector2(0.4f, 0.5f),
                new Vector2(0.35f, 0.5f), new Vector2(0.35f, -0.3f), new Vector2(0.025f, -0.3f),
                new Vector2(0.025f, 0.5f), new Vector2(-0.2f, 0.5f) };
            Physics2D.SyncTransforms();
            var destination = new Vector2Int(2, 0);
            Assert.That(world.SamplePoint(cache.CellToWorld(destination)), Is.EqualTo(StaticNavigationSampleState.Clear));
            Assert.That(Check(World(0.125f, 0.125f), destination), Is.EqualTo(StaticNavigationSampleState.Blocked));
        }

        [TestCase(0.22f, 0.02f)]
        [TestCase(0.225f, 0.01f)]
        public void FullBodySweepRejectsCornerClip_EvenWithClearEndpoints(float centerY, float height)
        {
            Configure(0.1f);
            Box(0.375f, centerY, 0.02f, height);
            var start = World(0.125f, 0.125f);
            Assert.That(world.SamplePoint(start), Is.EqualTo(StaticNavigationSampleState.Clear));
            Assert.That(world.SamplePoint(cache.CellToWorld(new Vector2Int(2, 0))), Is.EqualTo(StaticNavigationSampleState.Clear));
            Assert.That(Check(start, new Vector2Int(2, 0)), Is.EqualTo(StaticNavigationSampleState.Blocked));
        }

        [Test]
        public void HalfUnitLimit_IsInclusive_AndLongerConnectorsAreRejectedBeforeSampling()
        {
            Assert.That(Check(World(0.125f, 0.125f), new Vector2Int(2, 0)), Is.EqualTo(StaticNavigationSampleState.Clear));
            int before = cache.DebugSamplingQueries;
            Assert.That(connector.CheckConnection(World(0.124f, 0.125f), new Vector2Int(2, 0)),
                Is.EqualTo(StaticNavigationSampleState.Blocked));
            Assert.That(cache.DebugSamplingQueries, Is.EqualTo(before));
        }

        [Test]
        public void CandidateChoiceIsDeterministic_ColdOrWarm_WithoutClearingContactedCell()
        {
            Box(-1f, 0f, 2f, 10f);
            var start = World(MeleeRadius, 0.125f);
            Vector2Int cold = default;
            StaticNavigationSampleState state = StaticNavigationSampleState.Unknown;
            for (int i = 0; i < 64 && state == StaticNavigationSampleState.Unknown; i++)
            { state = connector.TryConnect(start, out cold); cache.SamplePending(128); }
            Assert.That(state, Is.EqualTo(StaticNavigationSampleState.Clear), connector.DebugCandidateTrace);
            Assert.That(cold, Is.EqualTo(new Vector2Int(4, 0)));
            Assert.That(connector.TryConnect(start, out var warm), Is.EqualTo(StaticNavigationSampleState.Clear));
            Assert.That(warm, Is.EqualTo(cold));
            Assert.That(cache.GetCellState(new Vector2Int(3, 0)), Is.EqualTo(StaticNavigationSampleState.Blocked));
        }

        [Test]
        public void EqualDistanceCandidates_UseRowThenColumnOrder()
        {
            var start = World(0.25f, 0.25f);
            StaticNavigationSampleState state = StaticNavigationSampleState.Unknown;
            Vector2Int cell = default;
            for (int i = 0; i < 32 && state == StaticNavigationSampleState.Unknown; i++)
            { state = connector.TryConnect(start, out cell); cache.SamplePending(128); }
            Assert.That(state, Is.EqualTo(StaticNavigationSampleState.Clear));
            Assert.That(cell, Is.EqualTo(Vector2Int.zero));
        }

        [Test]
        public void EmbeddedStartReturnsBlocked_WhenNoSafeCandidateExists()
        {
            Box(-1f, 0f, 2f, 10f);
            StaticNavigationSampleState state = StaticNavigationSampleState.Unknown;
            for (int i = 0; i < 64 && state == StaticNavigationSampleState.Unknown; i++)
            {
                state = connector.TryConnect(World(MeleeRadius - 0.02f, 0.125f), out _);
                cache.SamplePending(128);
            }
            Assert.That(state, Is.EqualTo(StaticNavigationSampleState.Blocked));
        }

        [Test]
        public void SaturatedContactQuery_ReturnsUnknown_NotClear()
        {
            Configure(MeleeRadius, 1);
            Box(-1f, 0f, 2f, 10f);
            Assert.That(Check(World(MeleeRadius, 0.125f), new Vector2Int(4, 0)), Is.EqualTo(StaticNavigationSampleState.Unknown));
        }

        [Test]
        public void TriggerAndDynamicGeometryRemainExcluded()
        {
            Configure(0.1f);
            Box(0.375f, 0.125f, 0.1f, 1f).isTrigger = true;
            var dynamic = Box(0.375f, 0.125f, 0.1f, 1f);
            dynamic.gameObject.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            Physics2D.SyncTransforms();
            Assert.That(Check(World(0.125f, 0.125f), new Vector2Int(2, 0)), Is.EqualTo(StaticNavigationSampleState.Clear));
        }

        [Test]
        public void SameInflatedRadius_WithDifferentPhysicalBody_CannotReplaceAuthoritativeProfile()
        {
            var other = new StaticNavigationWorld2D(MeleeRadius - 0.01f, 0.03f);
            Assert.That(other.ClearanceRadius, Is.EqualTo(world.ClearanceRadius).Within(0.000001f));
            Assert.Throws<System.ArgumentException>(() => new StaticNavigationStartConnector(cache, other));
        }

        [TestCase(0f, StaticNavigationSampleState.Clear)]
        [TestCase(-0.003f, StaticNavigationSampleState.Blocked)]
        public void CircleContact_PreservesPhysicalRadiusWithoutPolygonSkin(float gap, StaticNavigationSampleState expected)
        {
            var item = new GameObject("Round world solid", typeof(CircleCollider2D));
            objects.Add(item);
            item.transform.position = World(-0.5f, 0.125f);
            item.GetComponent<CircleCollider2D>().radius = 0.5f;
            Physics2D.SyncTransforms();
            Assert.That(Check(World(MeleeRadius + gap, 0.125f), new Vector2Int(4, 0)),
                Is.EqualTo(expected), connector.DebugLastDecision);
        }

        [TestCase(0.005f)]
        [TestCase(0.02f)]
        public void PolygonSkinCorrection_TracksConfiguredOffset_WithoutAllowingPhysicalPenetration(float offset)
        {
            float previous = Physics2D.defaultContactOffset;
            try
            {
                Physics2D.defaultContactOffset = offset;
                Box(-1f, 0f, 2f, 10f);
                Assert.That(Check(World(MeleeRadius, 0.125f), new Vector2Int(4, 0)),
                    Is.EqualTo(StaticNavigationSampleState.Clear), connector.DebugLastDecision);
                Assert.That(Check(World(MeleeRadius - 0.003f, 0.125f), new Vector2Int(4, 0)),
                    Is.EqualTo(StaticNavigationSampleState.Blocked), connector.DebugLastDecision);
            }
            finally { Physics2D.defaultContactOffset = previous; }
        }

        [Test]
        public void AuthoredBoxEdgeRadius_IsNotRemovedAsContactSkin()
        {
            var box = Box(-1f, 0f, 2f, 10f);
            box.edgeRadius = 0.05f;
            Physics2D.SyncTransforms();
            Assert.That(Check(World(MeleeRadius + 0.05f - 0.003f, 0.125f), new Vector2Int(4, 0)),
                Is.EqualTo(StaticNavigationSampleState.Blocked), connector.DebugLastDecision);
        }

        [Test]
        public void PolygonWallContact_UsesTheSameCorrectionAsBoxWallContact()
        {
            var item = new GameObject("Polygon world solid", typeof(PolygonCollider2D));
            objects.Add(item);
            item.transform.position = World(0f, 0f);
            item.GetComponent<PolygonCollider2D>().points = new[] {
                new Vector2(-2f, -5f), new Vector2(0f, -5f), new Vector2(0f, 5f), new Vector2(-2f, 5f) };
            Physics2D.SyncTransforms();
            Assert.That(Check(World(MeleeRadius, 0.125f), new Vector2Int(4, 0)),
                Is.EqualTo(StaticNavigationSampleState.Clear), connector.DebugLastDecision);
        }

        private void Configure(float radius, int queryCapacity = 64)
        {
            connector?.Dispose();
            world = new StaticNavigationWorld2D(radius);
            cache = new StaticNavigationWorldCache(new StaticNavigationLayout(authority,
                new BoundsInt(-16, -16, 0, 32, 32, 1)), world);
            connector = new StaticNavigationStartConnector(cache, world, queryCapacity);
            connector.DebugCaptureEnabled = true;
        }
        private BoxCollider2D Box(float x, float y, float width, float height)
        {
            var item = new GameObject("World solid", typeof(BoxCollider2D));
            objects.Add(item);
            item.transform.position = World(x, y);
            var box = item.GetComponent<BoxCollider2D>();
            box.size = new Vector2(width, height);
            Physics2D.SyncTransforms();
            return box;
        }
        private StaticNavigationSampleState Check(Vector2 start, Vector2Int destination)
        {
            cache.GetCellState(destination); cache.SamplePending(128);
            return connector.CheckConnection(start, destination);
        }
    }
}
