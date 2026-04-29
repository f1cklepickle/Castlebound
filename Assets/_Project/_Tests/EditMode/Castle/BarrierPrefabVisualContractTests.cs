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

        [Test]
        public void BarrierPrefab_SystemsRoot_OwnsDirectionalAndUiChildren()
        {
            var prefab = PrefabTestUtil.Load(BarrierPrefabPath);
            try
            {
                var binder = prefab.GetComponent<BarrierVisualBinder>();
                Assert.NotNull(binder, "Barrier prefab must include BarrierVisualBinder.");

                var systemsRoot = GetTransform(binder, "systemsRoot");
                Assert.NotNull(systemsRoot, "BarrierVisualBinder field 'systemsRoot' must be assigned.");

                AssertIsDescendantOfSystemsRoot(prefab, systemsRoot, "SpawnPointMarker");
                AssertIsDescendantOfSystemsRoot(prefab, systemsRoot, "HoldAnchor");
                AssertIsDescendantOfSystemsRoot(prefab, systemsRoot, "DamageHitbox");
                AssertIsDescendantOfSystemsRoot(prefab, systemsRoot, "PulseOrigin");
                AssertIsDescendantOfSystemsRoot(prefab, systemsRoot, "BarrierHealthbar");
                AssertIsDescendantOfSystemsRoot(prefab, systemsRoot, "TowerPlotLeftFlank");
                AssertIsDescendantOfSystemsRoot(prefab, systemsRoot, "TowerPlotRightFlank");
            }
            finally
            {
                PrefabTestUtil.Unload(prefab);
            }
        }

        [Test]
        public void BarrierPrefab_ProvidesTwoDistinctTowerPlots_OnFlankingOffsets()
        {
            var prefab = PrefabTestUtil.Load(BarrierPrefabPath);
            try
            {
                var collection = prefab.GetComponent<BarrierTowerPlotCollection>();
                Assert.NotNull(collection, "Barrier prefab must include BarrierTowerPlotCollection.");
                Assert.That(collection.PlotCount, Is.EqualTo(2), "Barrier prefab should expose two flanking tower plots.");

                var plots = collection.Plots;
                Assert.NotNull(plots[0], "Left flanking plot should be assigned.");
                Assert.NotNull(plots[1], "Right flanking plot should be assigned.");
                Assert.AreNotSame(plots[0], plots[1], "Barrier prefab tower plots must be distinct instances.");

                Assert.That(plots[0].transform.localPosition, Is.EqualTo(new Vector3(-3f, 0f, 0f)));
                Assert.That(plots[1].transform.localPosition, Is.EqualTo(new Vector3(3f, 0f, 0f)));
                Assert.AreSame(plots[0].transform, plots[0].Anchor, "Tower plot should default anchor to its own transform.");
                Assert.AreSame(plots[1].transform, plots[1].Anchor, "Tower plot should default anchor to its own transform.");
                Assert.IsFalse(plots[0].IsOccupied, "Tower plots should start empty on the prefab.");
                Assert.IsFalse(plots[1].IsOccupied, "Tower plots should start empty on the prefab.");
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

        private static Transform GetTransform(BarrierVisualBinder binder, string fieldName)
        {
            var field = typeof(BarrierVisualBinder).GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(field, $"Expected BarrierVisualBinder field '{fieldName}'.");
            return field.GetValue(binder) as Transform;
        }

        private static void AssertIsDescendantOfSystemsRoot(GameObject prefab, Transform systemsRoot, string childName)
        {
            var child = FindChildRecursive(prefab.transform, childName);
            Assert.NotNull(child, $"Barrier prefab missing child '{childName}'.");
            Assert.IsTrue(child.IsChildOf(systemsRoot), $"'{childName}' should be parented under SystemsRoot for side-rotation behavior.");
        }

        private static Transform FindChildRecursive(Transform root, string childName)
        {
            foreach (var tr in root.GetComponentsInChildren<Transform>(true))
            {
                if (tr.name == childName)
                {
                    return tr;
                }
            }

            return null;
        }
    }
}
