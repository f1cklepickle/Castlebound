using System.Collections;
using Castlebound.Gameplay.AI;
using Castlebound.Gameplay.Castle;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.Tilemaps;

namespace Castlebound.Tests.PlayMode.Castle
{
    public class CastleTilemapRuntimeContractsPlayTests
    {
        private const string MainPrototypeSceneName = "MainPrototype";
        private const string WallsTilemapName = "CastleWallsTilemap";
        private const string FloorTilemapName = "CastleFloorTilemap";
        private const string BarriersTilemapName = "CastleBarriersTilemap";
        private const string GeneratedParentName = "GeneratedBarriers";

        [UnityTest]
        public IEnumerator MainPrototype_WallsTilemap_HasActiveSolidCollider()
        {
            yield return LoadMainPrototype();

            var walls = FindByName<Tilemap>(WallsTilemapName);
            Assert.NotNull(walls, $"Expected tilemap '{WallsTilemapName}' in MainPrototype.");

            var collider = walls.GetComponent<TilemapCollider2D>();
            Assert.NotNull(collider, "CastleWallsTilemap must include TilemapCollider2D.");
            Assert.IsTrue(collider.enabled, "CastleWallsTilemap collider must be enabled in PlayMode.");
            Assert.IsFalse(collider.isTrigger, "CastleWallsTilemap collider must remain solid (non-trigger).");
        }

        [UnityTest]
        public IEnumerator MainPrototype_FloorTilemap_HasTriggerRegion_WithCastleRegionTracker()
        {
            yield return LoadMainPrototype();

            var floor = FindByName<Tilemap>(FloorTilemapName);
            Assert.NotNull(floor, $"Expected tilemap '{FloorTilemapName}' in MainPrototype.");

            var collider = floor.GetComponent<TilemapCollider2D>();
            Assert.NotNull(collider, "CastleFloorTilemap must include TilemapCollider2D.");
            Assert.IsTrue(collider.enabled, "CastleFloorTilemap collider must be enabled in PlayMode.");
            Assert.IsTrue(collider.isTrigger, "CastleFloorTilemap collider must be trigger-based for region detection.");

            var tracker = floor.GetComponent<CastleRegionTracker>();
            Assert.NotNull(tracker, "CastleRegionTracker must live on CastleFloorTilemap region source.");
        }

        [UnityTest]
        public IEnumerator MainPrototype_BarrierBreak_DisablesGeneratedBarrierCollider()
        {
            yield return LoadMainPrototype();

            var barriersTilemap = FindByName<Tilemap>(BarriersTilemapName);
            Assert.NotNull(barriersTilemap, $"Expected tilemap '{BarriersTilemapName}' in MainPrototype.");

            var bootstrap = barriersTilemap.GetComponent<BarrierAssemblyBootstrap>();
            Assert.NotNull(bootstrap, "CastleBarriersTilemap must include BarrierAssemblyBootstrap.");

            var generatedRoot = FindByName<GameObject>(GeneratedParentName);
            if (generatedRoot == null || generatedRoot.transform.childCount == 0)
            {
                bootstrap.RebuildNow();
                yield return null;
                generatedRoot = FindByName<GameObject>(GeneratedParentName);
            }

            Assert.NotNull(generatedRoot, "Expected GeneratedBarriers root after bootstrap rebuild.");
            Assert.That(generatedRoot.transform.childCount, Is.GreaterThan(0), "Expected generated barriers in PlayMode.");

            var barrier = generatedRoot.GetComponentInChildren<BarrierHealth>(true);
            Assert.NotNull(barrier, "Expected generated barrier with BarrierHealth.");

            var collider = barrier.GetComponent<Collider2D>();
            Assert.NotNull(collider, "Expected collider on generated barrier.");
            Assert.IsTrue(collider.enabled, "Barrier collider should start enabled.");

            var damage = Mathf.Max(1, barrier.MaxHealth);
            barrier.TakeDamage(damage);
            yield return null;

            Assert.IsTrue(barrier.IsBroken, "Barrier should be broken after lethal damage.");
            Assert.IsFalse(collider.enabled, "Barrier collider should disable when barrier is broken.");
        }

        [UnityTest]
        public IEnumerator MainPrototype_BarrierBootstrap_Rebuild_GeneratesBarriersWithExpectedRotationContract()
        {
            yield return LoadMainPrototype();

            var barriersTilemap = FindByName<Tilemap>(BarriersTilemapName);
            Assert.NotNull(barriersTilemap, $"Expected tilemap '{BarriersTilemapName}' in MainPrototype.");

            var bootstrap = barriersTilemap.GetComponent<BarrierAssemblyBootstrap>();
            Assert.NotNull(bootstrap, "CastleBarriersTilemap must include BarrierAssemblyBootstrap.");

            var source = new BarrierTilemapLayoutSource(barriersTilemap);
            var slots = new System.Collections.Generic.Dictionary<string, BarrierSide>();
            foreach (var slot in source.GetSlots())
            {
                slots[slot.Id] = slot.Side;
            }

            Assert.That(slots.Count, Is.GreaterThan(0), "Barrier tilemap should produce at least one assembly slot.");

            bootstrap.RebuildNow();
            yield return null;

            var generatedRoot = FindByName<GameObject>(GeneratedParentName);
            Assert.NotNull(generatedRoot, "Expected GeneratedBarriers root after bootstrap rebuild.");
            Assert.That(generatedRoot.transform.childCount, Is.EqualTo(slots.Count), "Generated barrier count should match marker slot count.");

            foreach (Transform child in generatedRoot.transform)
            {
                Assert.IsTrue(slots.TryGetValue(child.name, out var side),
                    $"Generated barrier '{child.name}' should map to a barrier marker slot.");

                var rootZ = Mathf.DeltaAngle(0f, child.eulerAngles.z);
                Assert.That(rootZ, Is.EqualTo(0f).Within(0.5f), $"Barrier root '{child.name}' should remain unrotated.");

                var systemsRoot = child.Find("SystemsRoot");
                Assert.NotNull(systemsRoot, $"Generated barrier '{child.name}' missing SystemsRoot.");
                var systemsZ = Mathf.DeltaAngle(0f, systemsRoot.localEulerAngles.z);
                Assert.That(systemsZ, Is.EqualTo(ExpectedRotation(side)).Within(0.5f),
                    $"SystemsRoot rotation mismatch for barrier '{child.name}' side {side}.");

                var health = child.GetComponent<BarrierHealth>();
                var binder = child.GetComponent<BarrierVisualBinder>();
                var renderer = child.GetComponent<SpriteRenderer>();
                Assert.NotNull(health, $"Generated barrier '{child.name}' missing BarrierHealth.");
                Assert.NotNull(binder, $"Generated barrier '{child.name}' missing BarrierVisualBinder.");
                Assert.NotNull(renderer, $"Generated barrier '{child.name}' missing SpriteRenderer.");
                Assert.NotNull(renderer.sprite, $"Generated barrier '{child.name}' missing assigned side sprite.");
            }
        }

        private static IEnumerator LoadMainPrototype()
        {
            var load = SceneManager.LoadSceneAsync(MainPrototypeSceneName, LoadSceneMode.Single);
            while (!load.isDone)
            {
                yield return null;
            }

            yield return null;
            yield return new WaitForFixedUpdate();
        }

        private static T FindByName<T>(string name) where T : class
        {
            var scene = SceneManager.GetActiveScene();
            foreach (var root in scene.GetRootGameObjects())
            {
                if (typeof(T) == typeof(GameObject) && root.name == name)
                {
                    return root as T;
                }

                foreach (var tr in root.GetComponentsInChildren<Transform>(true))
                {
                    if (tr.name != name)
                    {
                        continue;
                    }

                    if (typeof(T) == typeof(GameObject))
                    {
                        return tr.gameObject as T;
                    }

                    var component = tr.GetComponent(typeof(T));
                    if (component != null)
                    {
                        return component as T;
                    }
                }
            }

            return null;
        }

        private static float ExpectedRotation(BarrierSide side)
        {
            switch (side)
            {
                case BarrierSide.East:
                    return -90f;
                case BarrierSide.South:
                    return 180f;
                case BarrierSide.West:
                    return 90f;
                default:
                    return 0f;
            }
        }
    }
}
