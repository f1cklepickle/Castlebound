using Castlebound.Gameplay.Castle;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Castle
{
    public class BarrierPrefabVisualContractTests
    {
        private const string BarrierPrefabPath = "Assets/_Project/Prefabs/Barrier_Gate.prefab";
        private const string EnemyPrefabPath = "Assets/_Project/Prefabs/Enemy.prefab";
        private const float ColliderTolerance = 0.05f;

        [Test]
        public void BarrierPrefab_HasVisualBinder_WithAllSideSpritesAssigned()
        {
            var prefab = PrefabTestUtil.Load(BarrierPrefabPath);
            try
            {
                var binder = prefab.GetComponent<BarrierVisualBinder>();
                Assert.NotNull(binder, "Barrier prefab must include BarrierVisualBinder.");

                AssertLayerSpritesAssigned(binder, "groundSprites");
                AssertLayerSpritesAssigned(binder, "gateSprites");
                AssertLayerSpritesAssigned(binder, "wallSprites");
                AssertLayerSpritesAssigned(binder, "archSprites");
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

                AssertLayerSpritesAre96By96(binder, "groundSprites");
                AssertLayerSpritesAre96By96(binder, "gateSprites");
                AssertLayerSpritesAre96By96(binder, "wallSprites");
                AssertLayerSpritesAre96By96(binder, "archSprites");
            }
            finally
            {
                PrefabTestUtil.Unload(prefab);
            }
        }

        [Test]
        public void BarrierPrefab_LayeredVisuals_HaveExpectedHierarchySortingAndNoPhysics()
        {
            var prefab = PrefabTestUtil.Load(BarrierPrefabPath);
            try
            {
                var ground = FindChildRecursive(prefab.transform, "GroundRenderer");
                var gateShakeRoot = FindChildRecursive(prefab.transform, "GateShakeRoot");
                var gate = FindChildRecursive(prefab.transform, "GateRenderer");
                var wall = FindChildRecursive(prefab.transform, "WallRenderer");
                var arch = FindChildRecursive(prefab.transform, "ArchRenderer");

                Assert.NotNull(ground);
                Assert.NotNull(gateShakeRoot);
                Assert.NotNull(gate);
                Assert.NotNull(wall);
                Assert.NotNull(arch);
                Assert.AreSame(gateShakeRoot, gate.parent, "GateRenderer should be the only visual under GateShakeRoot.");

                AssertRendererOrder(ground, 0);
                AssertRendererOrder(gate, 1);
                AssertRendererOrder(wall, 2);
                AssertRendererOrder(arch, 10);

                foreach (var visual in new[] { ground, gateShakeRoot, gate, wall, arch })
                {
                    Assert.That(visual.localPosition, Is.EqualTo(Vector3.zero), $"{visual.name} must align to the shared 96x96 canvas.");
                    Assert.IsNull(visual.GetComponent<Collider2D>(), $"{visual.name} must remain visual-only.");
                    Assert.IsNull(visual.GetComponent<Rigidbody2D>(), $"{visual.name} must remain visual-only.");
                }

                var shake = prefab.GetComponent<BarrierHitShake>();
                Assert.NotNull(shake);
                Assert.AreSame(gateShakeRoot, shake.ShakeTarget, "Only GateShakeRoot should move during barrier hit shake.");

                var health = prefab.GetComponent<BarrierHealth>();
                Assert.NotNull(health);
                var gateField = typeof(BarrierHealth).GetField("barrierGateRenderer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                Assert.NotNull(gateField);
                Assert.AreSame(gate.GetComponent<SpriteRenderer>(), gateField.GetValue(health), "Broken barriers should hide only GateRenderer.");
            }
            finally
            {
                PrefabTestUtil.Unload(prefab);
            }
        }

        [Test]
        public void BarrierAndEnemyPrefabs_RenderEnemyAboveWallAndBelowArch()
        {
            var barrier = PrefabTestUtil.Load(BarrierPrefabPath);
            var enemy = PrefabTestUtil.Load(EnemyPrefabPath);
            try
            {
                var wallOrder = FindChildRecursive(barrier.transform, "WallRenderer").GetComponent<SpriteRenderer>().sortingOrder;
                var archOrder = FindChildRecursive(barrier.transform, "ArchRenderer").GetComponent<SpriteRenderer>().sortingOrder;
                var enemyOrder = enemy.GetComponentInChildren<SpriteRenderer>(true).sortingOrder;

                Assert.That(enemyOrder, Is.GreaterThan(wallOrder));
                Assert.That(enemyOrder, Is.LessThan(archOrder));
            }
            finally
            {
                PrefabTestUtil.Unload(enemy);
                PrefabTestUtil.Unload(barrier);
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

        private static void AssertLayerSpritesAssigned(BarrierVisualBinder binder, string fieldName)
        {
            var spriteSet = GetSpriteSet(binder, fieldName);
            foreach (BarrierSide side in System.Enum.GetValues(typeof(BarrierSide)))
            {
                Assert.NotNull(spriteSet.GetSprite(side), $"BarrierVisualBinder field '{fieldName}' must assign {side}.");
            }
        }

        private static void AssertLayerSpritesAre96By96(BarrierVisualBinder binder, string fieldName)
        {
            var spriteSet = GetSpriteSet(binder, fieldName);
            foreach (BarrierSide side in System.Enum.GetValues(typeof(BarrierSide)))
            {
                var sprite = spriteSet.GetSprite(side);
                Assert.NotNull(sprite, $"BarrierVisualBinder field '{fieldName}' must assign {side}.");
                Assert.That(sprite.rect.width, Is.EqualTo(96f), $"{fieldName} {side} sprite width should be 96px.");
                Assert.That(sprite.rect.height, Is.EqualTo(96f), $"{fieldName} {side} sprite height should be 96px.");
            }
        }

        private static BarrierDirectionalSpriteSet GetSpriteSet(BarrierVisualBinder binder, string fieldName)
        {
            var field = typeof(BarrierVisualBinder).GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(field, $"Expected BarrierVisualBinder field '{fieldName}'.");
            return field.GetValue(binder) as BarrierDirectionalSpriteSet;
        }

        private static void AssertRendererOrder(Transform transform, int expectedOrder)
        {
            var renderer = transform.GetComponent<SpriteRenderer>();
            Assert.NotNull(renderer, $"{transform.name} requires a SpriteRenderer.");
            Assert.That(renderer.sortingOrder, Is.EqualTo(expectedOrder));
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
