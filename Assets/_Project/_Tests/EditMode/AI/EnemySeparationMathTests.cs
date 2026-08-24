using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.AI
{
    public class EnemySeparationMathTests
    {
        [Test]
        public void ClampInwardDisplacement_RemovesOnlyMinimumFootprintViolation()
        {
            Vector2 result = EnemySeparationMath.ClampInwardDisplacement(
                new Vector2(0.42f, 0f),
                0.4f,
                new Vector2(0.05f, 0.03f),
                0.001f);

            float expectedInward = 0.42f - Mathf.Sqrt(0.4f * 0.4f - 0.03f * 0.03f);
            Assert.That(result.x, Is.EqualTo(expectedInward).Within(0.000001f));
            Assert.That(result.y, Is.EqualTo(0.03f).Within(0.000001f));
        }

        [Test]
        public void ClampInwardDisplacement_PreservesOutwardMovement()
        {
            Vector2 proposed = new Vector2(-0.05f, 0.03f);
            Vector2 result = EnemySeparationMath.ClampInwardDisplacement(
                new Vector2(0.4f, 0f),
                0.4f,
                proposed,
                0.001f);

            Assert.That(result, Is.EqualTo(proposed));
        }

        [Test]
        public void ClampInwardDisplacement_DoesNotPermitCrossingThroughFootprint()
        {
            Vector2 result = EnemySeparationMath.ClampInwardDisplacement(
                new Vector2(0.4f, 0f),
                0.4f,
                Vector2.right,
                0.001f);

            Assert.That(result, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void ResolveAxis_ExactCoincidence_UsesStableFallback()
        {
            Assert.That(
                EnemySeparationMath.ResolveAxis(Vector2.zero, 0.001f),
                Is.EqualTo(Vector2.right));
        }

        [Test]
        public void AllocateCorrections_OneSideBlocked_MovesOnlyAvailableSide()
        {
            EnemySeparationMath.AllocateCorrections(
                0.1f,
                0f,
                0.1f,
                out float blockedCorrection,
                out float availableCorrection);

            Assert.That(blockedCorrection, Is.Zero);
            Assert.That(availableCorrection, Is.EqualTo(0.1f).Within(0.000001f));
        }

        [Test]
        public void AllocateCorrections_OpposingStep_SplitsFullPenetration()
        {
            EnemySeparationMath.AllocateCorrections(
                0.08f,
                0.05f,
                0.05f,
                out float firstCorrection,
                out float secondCorrection);

            Assert.That(firstCorrection, Is.EqualTo(0.04f).Within(0.000001f));
            Assert.That(secondCorrection, Is.EqualTo(0.04f).Within(0.000001f));
            Assert.That(firstCorrection + secondCorrection,
                Is.EqualTo(0.08f).Within(0.000001f));
        }

        [Test]
        public void AllocateCorrections_BothSidesBlocked_DoesNotCreateMovement()
        {
            EnemySeparationMath.AllocateCorrections(
                0.1f,
                0f,
                0f,
                out float firstCorrection,
                out float secondCorrection);

            Assert.That(firstCorrection, Is.Zero);
            Assert.That(secondCorrection, Is.Zero);
        }
    }
}
