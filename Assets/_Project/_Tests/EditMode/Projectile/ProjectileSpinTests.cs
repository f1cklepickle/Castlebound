using Castlebound.Gameplay.Projectile;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Projectile
{
    public class ProjectileSpinTests
    {
        [Test]
        public void Advance_RotatesByConfiguredDegreesPerSecond()
        {
            var projectile = new GameObject("SpinningProjectile");
            try
            {
                var spin = projectile.AddComponent<ProjectileSpin>();
                spin.DegreesPerSecond = 360f;

                spin.Advance(0.25f);

                Assert.That(projectile.transform.eulerAngles.z, Is.EqualTo(90f).Within(0.01f));
            }
            finally
            {
                Object.DestroyImmediate(projectile);
            }
        }

        [Test]
        public void Advance_WithNegativeDeltaTime_DoesNotRotate()
        {
            var projectile = new GameObject("SpinningProjectile");
            try
            {
                var spin = projectile.AddComponent<ProjectileSpin>();
                spin.DegreesPerSecond = 360f;

                spin.Advance(-1f);

                Assert.That(projectile.transform.eulerAngles.z, Is.EqualTo(0f).Within(0.01f));
            }
            finally
            {
                Object.DestroyImmediate(projectile);
            }
        }
    }
}
