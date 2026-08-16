using System;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Player
{
    public class PlayerSweepSlideMathTests
    {
        [Test]
        public void GetWorldCenterOffset_AppliesScaleAndRotation()
        {
            Vector2 result = PlayerSweepSlideMath.GetWorldCenterOffset(
                new Vector2(0.25f, -0.5f),
                new Vector3(2f, 3f, 1f),
                90f);

            Assert.That(result.x, Is.EqualTo(1.5f).Within(0.0001f));
            Assert.That(result.y, Is.EqualTo(0.5f).Within(0.0001f));
        }

        [Test]
        public void GetWorldRadius_UsesLargestAbsoluteScaleAxis()
        {
            float radius = PlayerSweepSlideMath.GetWorldRadius(
                0.4f,
                new Vector3(-2f, 3f, 1f));

            Assert.That(radius, Is.EqualTo(1.2f).Within(0.0001f));
        }

        [Test]
        public void ClipAgainstNormals_RemovesOnlyInwardMotion()
        {
            var normals = new Vector2[2];
            int count = 0;
            PlayerSweepSlideMath.AddSortedUniqueNormal(normals, ref count, Vector2.left);

            Vector2 clipped = PlayerSweepSlideMath.ClipAgainstNormals(
                new Vector2(2f, 3f),
                normals,
                count);

            Assert.That(clipped.x, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(clipped.y, Is.EqualTo(3f).Within(0.0001f));
        }

        [Test]
        public void ClipAgainstNormals_InsideCornerIsDeterministicAcrossInputOrder()
        {
            Vector2 first = ResolveCorner(Vector2.left, Vector2.down);
            Vector2 reversed = ResolveCorner(Vector2.down, Vector2.left);

            Assert.That(first.sqrMagnitude, Is.LessThan(0.000001f));
            Assert.That(reversed, Is.EqualTo(first));
        }

        [Test]
        public void GetAllowedTravelDistance_DiagonalContactReservesSkinAlongNormal()
        {
            var normals = new Vector2[1];
            int count = 0;
            PlayerSweepSlideMath.AddSortedUniqueNormal(normals, ref count, Vector2.left);
            Vector2 direction = Vector2.one.normalized;
            float contactDistance = 0.25f / direction.x;

            float travel = PlayerSweepSlideMath.GetAllowedTravelDistance(
                contactDistance,
                1f,
                0.02f,
                direction,
                normals,
                count);

            Assert.That(travel * direction.x, Is.EqualTo(0.23f).Within(0.0001f));
        }

        [Test]
        public void RepeatedGeometryAndClipping_PerformsWithoutManagedAllocations()
        {
            var normals = new Vector2[4];
            RunRepeatedMath(normals, 8);
            long before = GC.GetAllocatedBytesForCurrentThread();

            RunRepeatedMath(normals, 1024);

            long after = GC.GetAllocatedBytesForCurrentThread();
            Assert.That(after - before, Is.Zero);
        }

        private static Vector2 ResolveCorner(Vector2 firstNormal, Vector2 secondNormal)
        {
            var normals = new Vector2[2];
            int count = 0;
            PlayerSweepSlideMath.AddSortedUniqueNormal(normals, ref count, firstNormal);
            PlayerSweepSlideMath.AddSortedUniqueNormal(normals, ref count, secondNormal);
            return PlayerSweepSlideMath.ClipAgainstNormals(Vector2.one, normals, count);
        }

        private static void RunRepeatedMath(Vector2[] normals, int iterations)
        {
            for (int i = 0; i < iterations; i++)
            {
                int count = 0;
                PlayerSweepSlideMath.AddSortedUniqueNormal(normals, ref count, Vector2.left);
                PlayerSweepSlideMath.AddSortedUniqueNormal(normals, ref count, Vector2.down);
                PlayerSweepSlideMath.ClipAgainstNormals(Vector2.one, normals, count);
                PlayerSweepSlideMath.GetAllowedTravelDistance(
                    1f,
                    1f,
                    0.02f,
                    Vector2.one.normalized,
                    normals,
                    count);
                PlayerSweepSlideMath.GetWorldCenterOffset(
                    new Vector2(0.1f, -0.2f),
                    new Vector3(1.5f, 0.75f, 1f),
                    i % 360);
                PlayerSweepSlideMath.GetWorldRadius(0.5f, new Vector3(1.5f, 0.75f, 1f));
            }
        }
    }
}
