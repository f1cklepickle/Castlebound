using Castlebound.Gameplay.Castle;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Castle
{
    public class BarrierPrefabVisualContractTests
    {
        private const string BarrierPrefabPath = "Assets/_Project/Prefabs/Barrier_Gate.prefab";
        private const float ColliderTolerance = 0.05f;

        [Test]
        public void BarrierPrefab_HasVisualBinder_WithAllSideSpritesAssigned()
        {
            var prefab = PrefabTestUtil.Load(BarrierPrefabPath);
            try
            {
                var binder = prefab.GetComponent<BarrierVisualBinder>();
                Assert.NotNull(binder, "Barrier prefab must include BarrierVisualBinder.");

                AssertSpriteAssigned(binder, "northSprite");
                AssertSpriteAssigned(binder, "eastSprite");
                AssertSpriteAssigned(binder, "southSprite");
                AssertSpriteAssigned(binder, "westSprite");
            }
            finally
            {
                PrefabTestUtil.Unload(prefab);
            }
        }

        [Test]
        public void BarrierPrefab_SideSprites_Are96By96()
        {
            var prefab = PrefabTestUtil.Load(BarrierPrefabPath);
            try
            {
                var binder = prefab.GetComponent<BarrierVisualBinder>();
                Assert.NotNull(binder, "Barrier prefab must include BarrierVisualBinder.");

                AssertSpriteSizeIs96(GetSprite(binder, "northSprite"), "northSprite");
                AssertSpriteSizeIs96(GetSprite(binder, "eastSprite"), "eastSprite");
                AssertSpriteSizeIs96(GetSprite(binder, "southSprite"), "southSprite");
                AssertSpriteSizeIs96(GetSprite(binder, "westSprite"), "westSprite");
            }
            finally
            {
                PrefabTestUtil.Unload(prefab);
            }
        }

        [Test]
        public void BarrierPrefab_ColliderFootprint_IsThreeByThreeUnits()
        {
            var prefab = PrefabTestUtil.Load(BarrierPrefabPath);
            try
            {
                var collider = prefab.GetComponent<BoxCollider2D>();
                Assert.NotNull(collider, "Barrier prefab requires BoxCollider2D on root.");
                Assert.That(collider.size.x, Is.EqualTo(3f).Within(ColliderTolerance), "Barrier collider width should be 3 world units.");
                Assert.That(collider.size.y, Is.EqualTo(3f).Within(ColliderTolerance), "Barrier collider height should be 3 world units.");
            }
            finally
            {
                PrefabTestUtil.Unload(prefab);
            }
        }

        private static void AssertSpriteAssigned(BarrierVisualBinder binder, string fieldName)
        {
            var sprite = GetSprite(binder, fieldName);
            Assert.NotNull(sprite, $"BarrierVisualBinder field '{fieldName}' must be assigned.");
        }

        private static void AssertSpriteSizeIs96(Sprite sprite, string fieldName)
        {
            Assert.NotNull(sprite, $"BarrierVisualBinder field '{fieldName}' must be assigned.");
            Assert.That(sprite.rect.width, Is.EqualTo(96f), $"{fieldName} sprite width should be 96px.");
            Assert.That(sprite.rect.height, Is.EqualTo(96f), $"{fieldName} sprite height should be 96px.");
        }

        private static Sprite GetSprite(BarrierVisualBinder binder, string fieldName)
        {
            var field = typeof(BarrierVisualBinder).GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(field, $"Expected BarrierVisualBinder field '{fieldName}'.");
            return field.GetValue(binder) as Sprite;
        }
    }
}
