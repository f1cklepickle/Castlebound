using Castlebound.Gameplay.Combat;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Combat
{
    public class PlayerGuardArcPresenterTests
    {
        private GameObject player;
        private PlayerDefenseController defense;
        private LineRenderer lineRenderer;
        private PlayerGuardArcPresenter presenter;

        [SetUp]
        public void SetUp()
        {
            player = new GameObject("PlayerGuardArc");
            player.AddComponent<Health>();
            defense = player.AddComponent<PlayerDefenseController>();
            defense.Configure(0.15f, 0.15f, 120f, 0.6f);
            lineRenderer = player.AddComponent<LineRenderer>();
            presenter = player.AddComponent<PlayerGuardArcPresenter>();
            presenter.RefreshPresentation();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(player);
        }

        [Test]
        public void StateChanges_ShowGuardAndHideImmediatelyDuringRecovery()
        {
            Assert.IsFalse(lineRenderer.enabled);

            defense.SetDefensePressed(true);
            Assert.IsTrue(lineRenderer.enabled);
            Assert.That(lineRenderer.positionCount, Is.GreaterThan(2));
            defense.SetDefensePressed(false);
            Assert.IsFalse(lineRenderer.enabled);

            defense.Tick(0.15f);
            Assert.IsFalse(lineRenderer.enabled);
        }

        [Test]
        public void SuccessfulParry_UsesDistinctFlashColor()
        {
            var attacker = new GameObject("Attacker");
            try
            {
                player.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                attacker.transform.position = Vector2.right;
                defense.SetDefensePressed(true);

                defense.ReceiveHit(new PlayerHitRequest(
                    1,
                    attacker,
                    attacker.transform.position,
                    CombatDamageType.Melee));

                Color renderedColor = lineRenderer.startColor;
                Color expectedColor = presenter.ParrySuccessColor;
                const float colorChannelTolerance = (1f / 255f) + 0.0001f;
                Assert.That(renderedColor.r, Is.EqualTo(expectedColor.r).Within(colorChannelTolerance));
                Assert.That(renderedColor.g, Is.EqualTo(expectedColor.g).Within(colorChannelTolerance));
                Assert.That(renderedColor.b, Is.EqualTo(expectedColor.b).Within(colorChannelTolerance));
                Assert.That(renderedColor.a, Is.EqualTo(expectedColor.a).Within(colorChannelTolerance));
            }
            finally
            {
                Object.DestroyImmediate(attacker);
            }
        }
    }
}
