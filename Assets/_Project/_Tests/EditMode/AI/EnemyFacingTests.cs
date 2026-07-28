using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.AI
{
    public class EnemyFacingTests
    {
        [Test]
        public void IsDirectionAligned_AcceptsDirectionInsideThreshold()
        {
            Assert.IsTrue(EnemyFacing.IsDirectionAligned(
                Vector2.right,
                new Vector2(1f, 0.1f),
                15f));
        }

        [Test]
        public void IsDirectionAligned_RejectsDirectionOutsideThreshold()
        {
            Assert.IsFalse(EnemyFacing.IsDirectionAligned(
                Vector2.right,
                Vector2.up,
                45f));
        }

        [Test]
        public void TurnToward_AdvancesByAtMostConfiguredTurnSpeed()
        {
            Vector2 result = EnemyFacing.TurnToward(
                Vector2.right,
                Vector2.up,
                turnSpeedDegreesPerSecond: 90f,
                deltaTime: 0.25f);

            Assert.That(Vector2.Angle(Vector2.right, result), Is.EqualTo(22.5f).Within(0.01f));
            Assert.That(result.magnitude, Is.EqualTo(1f).Within(0.001f));
        }

        [Test]
        public void TurnToward_AntiparallelDirection_TurnsWithin2DPlane()
        {
            Vector2 result = EnemyFacing.TurnToward(
                Vector2.down,
                Vector2.up,
                turnSpeedDegreesPerSecond: 120f,
                deltaTime: 0.25f);

            Assert.That(Vector2.Angle(Vector2.down, result), Is.EqualTo(30f).Within(0.01f),
                "A south-facing enemy must begin turning toward a directly north target.");
            Assert.That(result.magnitude, Is.EqualTo(1f).Within(0.001f));
        }

        [Test]
        public void MissingOrZeroDirection_IsNotAttackAligned()
        {
            Assert.IsFalse(EnemyFacing.IsDirectionAligned(Vector2.zero, Vector2.right, 45f));
            Assert.IsFalse(EnemyFacing.IsDirectionAligned(Vector2.right, Vector2.zero, 45f));
        }

        [Test]
        public void VisualRotation_TreatsSouthAsAuthoredSpriteForward()
        {
            Assert.That(EnemyFacing.GetVisualRotationDegrees(Vector2.down), Is.EqualTo(0f).Within(0.001f));
            Assert.That(EnemyFacing.GetVisualRotationDegrees(Vector2.right), Is.EqualTo(90f).Within(0.001f));
        }

        [Test]
        public void InitializeAimDirection_AppliesNormalizedSpawnDirectionImmediately()
        {
            var enemy = new GameObject("Enemy");
            try
            {
                var facing = enemy.AddComponent<EnemyFacing>();

                facing.InitializeAimDirection(new Vector2(2f, 0f));

                Assert.That(facing.AimDirection, Is.EqualTo(Vector2.right));
            }
            finally
            {
                Object.DestroyImmediate(enemy);
            }
        }

    }
}
