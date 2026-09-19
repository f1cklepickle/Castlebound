using System.Collections.Generic;
using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.AI
{
    public class StaticNavigationWorld2DTests
    {
        private readonly List<GameObject> objects = new List<GameObject>();
        private readonly Vector2 origin = new Vector2(10000f, 10000f);
        private StaticNavigationWorld2D world;

        [SetUp]
        public void SetUp() => world = new StaticNavigationWorld2D(0.87684506f);

        [TearDown]
        public void TearDown()
        {
            for (int i = objects.Count - 1; i >= 0; i--) Object.DestroyImmediate(objects[i]);
            objects.Clear();
            Physics2D.SyncTransforms();
        }

        [TestCase("Walls")]
        [TestCase("Barriers")]
        [TestCase("Gates")]
        [TestCase("Environment")]
        [TestCase("Default")]
        public void EnabledWorldSolid_BlocksPointAndSweep(string layer)
        {
            Box(layer);
            Physics2D.SyncTransforms();
            Assert.IsFalse(world.IsPointClear(origin));
            Assert.IsFalse(world.IsSegmentClear(origin + Vector2.left * 3f, origin + Vector2.right * 3f));
        }

        [Test]
        public void VaultSizedBox_BlocksWithoutEditorStaticFlag()
        {
            var box = Box("Walls");
            box.size = new Vector2(1.1f, 0.9f);
            Assert.IsFalse(box.gameObject.isStatic);
            Physics2D.SyncTransforms();
            Assert.IsFalse(world.IsSegmentClear(origin + Vector2.down * 3f, origin + Vector2.up * 3f));
        }

        [TestCase("trigger")]
        [TestCase("disabled")]
        [TestCase("inactive")]
        public void NonSolidOrInactiveCollider_IsIgnored(string kind)
        {
            var box = Box("Walls");
            if (kind == "trigger") box.isTrigger = true;
            if (kind == "disabled") box.enabled = false;
            if (kind == "inactive") box.gameObject.SetActive(false);
            Physics2D.SyncTransforms();
            Assert.IsTrue(world.IsPointClear(origin));
            Assert.IsTrue(world.IsSegmentClear(origin + Vector2.left * 3f, origin + Vector2.right * 3f));
        }

        [Test]
        public void BarrierDisableAndRepair_AreObservedByLiveQueries()
        {
            var box = Box("Barriers");
            box.gameObject.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            Physics2D.SyncTransforms();
            Assert.IsFalse(world.IsPointClear(origin));
            box.enabled = false;
            Physics2D.SyncTransforms();
            Assert.IsTrue(world.IsPointClear(origin));
            box.enabled = true;
            Physics2D.SyncTransforms();
            Assert.IsFalse(world.IsPointClear(origin));
        }

        [Test]
        public void ActualBodyRadiusAndMargin_BlockWhereSeparationSensorWouldFit()
        {
            Box("Walls");
            Physics2D.SyncTransforms();
            Vector2 near = origin + Vector2.right * 1.3f;
            Assert.IsTrue(new StaticNavigationWorld2D(0.2f, 0f).IsPointClear(near));
            Assert.IsFalse(world.IsPointClear(near));
            Vector2 edge = origin + Vector2.right * 1.5f;
            Assert.IsTrue(new StaticNavigationWorld2D(0.87684506f, 0f).IsPointClear(edge));
            Assert.IsFalse(new StaticNavigationWorld2D(0.87684506f, 0.2f).IsPointClear(edge));
        }

        [Test]
        public void ColliderOffsetRotationAndScale_AreReadFromWorldGeometry()
        {
            var box = Box("Walls");
            box.offset = new Vector2(2f, 0f);
            box.transform.localScale = new Vector3(2f, 1f, 1f);
            box.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            Physics2D.SyncTransforms();
            Assert.IsTrue(world.IsPointClear(origin));
            Assert.IsFalse(world.IsPointClear(origin + Vector2.up * 4f));
            Assert.IsFalse(world.IsSegmentClear(origin + new Vector2(-3f, 4f), origin + new Vector2(3f, 4f)));
        }

        [TestCase("Enemies", "Untagged")]
        [TestCase("Player", "Untagged")]
        [TestCase("Default", "Enemy")]
        [TestCase("Default", "Player")]
        public void ActorIncludingDefaultLayerChild_IsExcluded(string layer, string tag)
        {
            var parent = new GameObject("Actor");
            objects.Add(parent);
            parent.layer = LayerMask.NameToLayer(layer);
            parent.tag = tag;
            var box = Box("Default");
            box.transform.SetParent(parent.transform, true);
            Physics2D.SyncTransforms();
            Assert.IsTrue(world.IsPointClear(origin));
            Assert.IsTrue(world.IsSegmentClear(origin + Vector2.left * 3f, origin + Vector2.right * 3f));
        }

        [TestCase(RigidbodyType2D.Dynamic)]
        [TestCase(RigidbodyType2D.Kinematic)]
        public void MovingBodyTypes_AreExcludedEvenWhileStationary(RigidbodyType2D type)
        {
            Box("Default").gameObject.AddComponent<Rigidbody2D>().bodyType = type;
            Physics2D.SyncTransforms();
            Assert.IsTrue(world.IsPointClear(origin));
        }

        [Test]
        public void IgnoredHitDoesNotHideLaterWall_AndZeroLengthChecksOverlap()
        {
            Box("Walls").isTrigger = true;
            Box("Walls").transform.position = origin + Vector2.right * 4f;
            Physics2D.SyncTransforms();
            Assert.IsTrue(world.IsSegmentClear(origin, origin));
            Assert.IsFalse(world.IsSegmentClear(origin, origin + Vector2.right * 7f));
            Assert.IsFalse(world.IsSegmentClear(origin + Vector2.right * 4f, origin + Vector2.right * 4f));
        }

        [Test]
        public void NonAllocOverlapSaturation_IsUnknownUntilRetryCanResolveIt()
        {
            var box = Box("Walls");
            Physics2D.SyncTransforms();
            var smallBuffer = new StaticNavigationWorld2D(0.87684506f, 0.02f, 1);
            Assert.That(smallBuffer.SamplePoint(origin), Is.EqualTo(StaticNavigationSampleState.Unknown));
            Assert.IsFalse(smallBuffer.IsPointClear(origin), "Legacy bool queries must fail closed on saturation.");
            box.enabled = false;
            Physics2D.SyncTransforms();
            Assert.That(smallBuffer.SamplePoint(origin), Is.EqualTo(StaticNavigationSampleState.Clear));
            Assert.That(smallBuffer.DebugPhysicsQueryCount, Is.EqualTo(3));
        }

        [Test]
        public void NonAllocSweepSaturation_IsUnknown_AndDoesNotRepeatEndpointQueries()
        {
            Box("Walls");
            Box("Walls").transform.position = origin + Vector2.right;
            Physics2D.SyncTransforms();
            var smallBuffer = new StaticNavigationWorld2D(0.87684506f, 0.02f, 2);
            Vector2 start = origin + Vector2.left * 3f, end = origin + Vector2.right * 4f;
            Assert.That(smallBuffer.SamplePoint(start), Is.EqualTo(StaticNavigationSampleState.Clear));
            Assert.That(smallBuffer.SamplePoint(end), Is.EqualTo(StaticNavigationSampleState.Clear));
            int before = smallBuffer.DebugPhysicsQueryCount;
            Assert.That(smallBuffer.SampleEdgeWithClearEndpoints(start, end), Is.EqualTo(StaticNavigationSampleState.Unknown));
            Assert.That(smallBuffer.DebugPhysicsQueryCount - before, Is.EqualTo(1));
        }

        private BoxCollider2D Box(string layer)
        {
            var item = new GameObject("Navigation test solid");
            objects.Add(item);
            item.layer = LayerMask.NameToLayer(layer);
            item.transform.position = origin;
            return item.AddComponent<BoxCollider2D>();
        }
    }
}
