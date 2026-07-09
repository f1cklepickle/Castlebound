using Castlebound.Gameplay.Castle;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Castle
{
    public class VaultOutlinePresenterTests
    {
        private GameObject root;
        private SpriteRenderer renderer;
        private VaultOutlinePresenter presenter;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("VaultOutline");
            renderer = root.AddComponent<SpriteRenderer>();
            presenter = root.AddComponent<VaultOutlinePresenter>();
            presenter.SetOutlineRenderer(renderer);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root);
        }

        [Test]
        public void Apply_HidesOutline_OutOfRange()
        {
            presenter.Apply(VaultInteractionVisualState.Hidden);

            Assert.IsFalse(renderer.enabled);
        }

        [Test]
        public void Apply_UsesWhiteWhenAccessible_AndRedWhenBlocked()
        {
            presenter.Apply(VaultInteractionVisualState.Accessible);

            Assert.IsTrue(renderer.enabled);
            Assert.AreEqual(Color.white, renderer.color);

            presenter.Apply(VaultInteractionVisualState.Blocked);

            Assert.IsTrue(renderer.enabled);
            Assert.AreEqual(Color.red, renderer.color);
        }

        [Test]
        public void Apply_UsesSingleBorderSprite_InsteadOfScaledDuplicate()
        {
            presenter.Apply(VaultInteractionVisualState.Accessible);

            var edges = presenter.GetEdgeRenderersForTests();

            Assert.That(edges.Length, Is.EqualTo(1));
            Assert.That(edges[0], Is.SameAs(renderer));
            Assert.That(renderer.transform.localScale, Is.EqualTo(Vector3.one));
            Assert.That(renderer.transform.localPosition, Is.EqualTo(Vector3.zero));
        }
    }
}
