using Castlebound.Gameplay.Castle;
using NUnit.Framework;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Castlebound.Tests.Castle
{
    public class VaultPrefabContractTests
    {
        private const string PrefabPath = "Assets/_Project/Prefabs/Vault.prefab";
        private const string MainPrototypePath = "Assets/_Project/Scenes/MainPrototype.unity";
        private const string VaultSpritePath = "Assets/_Project/Art/Castle/Vault/castle_vault_01.png";
        private const string FoundationSpritePath = "Assets/_Project/Art/Castle/Vault/castle_vault_foundation_01.png";
        private const string OutlineSpritePath = "Assets/_Project/Art/Castle/Vault/castle_vault_outline_01.png";

        [Test]
        public void VaultPrefab_UsesExistingTagLayer_AndBlockingCollider()
        {
            var prefab = LoadPrefab();

            Assert.That(prefab.tag, Is.EqualTo("Wall"));
            Assert.That(LayerMask.LayerToName(prefab.layer), Is.EqualTo("Walls"));

            var blocker = prefab.GetComponent<BoxCollider2D>();
            Assert.NotNull(blocker);
            Assert.IsFalse(blocker.isTrigger);
            Assert.That(blocker.size.x, Is.EqualTo(1.1f).Within(0.001f));
            Assert.That(blocker.size.y, Is.EqualTo(0.9f).Within(0.001f));
            Assert.That(blocker.offset, Is.EqualTo(Vector2.zero));
            Assert.That(blocker.edgeRadius, Is.EqualTo(0.06f).Within(0.001f));
        }

        [Test]
        public void VaultPrefab_SortsFoundationBelowActors_AndMainAboveItems()
        {
            var prefab = LoadPrefab();
            var foundation = FindRenderer(prefab, "Foundation");
            var main = FindRenderer(prefab, "Main");
            var outline = FindRenderer(prefab, "Outline");

            Assert.That(foundation.sortingOrder, Is.EqualTo(2));
            Assert.That(foundation.sortingOrder, Is.LessThan(3));
            Assert.That(main.sortingOrder, Is.GreaterThan(12));
            Assert.That(outline.sortingOrder, Is.GreaterThan(main.sortingOrder));
            Assert.That(outline.transform.localScale, Is.EqualTo(Vector3.one));
            Assert.IsFalse(outline.enabled);
        }

        [Test]
        public void VaultPrefab_HasInteractionTrigger_AndOutlinePresenter()
        {
            var prefab = LoadPrefab();
            var interaction = prefab.GetComponentInChildren<VaultWorldInteraction>(true);
            var outline = prefab.GetComponentInChildren<VaultOutlinePresenter>(true);
            var trigger = interaction.GetComponent<CircleCollider2D>();

            Assert.NotNull(interaction);
            Assert.NotNull(outline);
            Assert.NotNull(trigger);
            Assert.IsTrue(trigger.isTrigger);
            Assert.That(trigger.radius, Is.EqualTo(2.4f).Within(0.001f));

            var serialized = new SerializedObject(interaction);
            Assert.That(serialized.FindProperty("touchTargetCollider").objectReferenceValue, Is.SameAs(trigger));
        }

        [Test]
        public void VaultSprites_UseProjectNaming_AndPixelImportSettings()
        {
            AssertPixelSprite(VaultSpritePath, "castle_vault_01");
            AssertPixelSprite(FoundationSpritePath, "castle_vault_foundation_01");
            AssertPixelSprite(OutlineSpritePath, "castle_vault_outline_01");
        }

        [Test]
        public void MainPrototypeVaultInstance_WiresSceneOpenReferences()
        {
            var scene = File.ReadAllText(MainPrototypePath);

            StringAssert.Contains("propertyPath: inventoryPanel", scene);
            StringAssert.Contains("objectReference: {fileID: 977390127}", scene);
            StringAssert.Contains("propertyPath: waveRunner", scene);
            StringAssert.Contains("objectReference: {fileID: 1166075763}", scene);
        }

        private static GameObject LoadPrefab()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.NotNull(prefab, $"Expected Vault prefab at {PrefabPath}.");
            return prefab;
        }

        private static SpriteRenderer FindRenderer(GameObject prefab, string childName)
        {
            var renderers = prefab.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var renderer in renderers)
            {
                if (renderer.name == childName)
                {
                    return renderer;
                }
            }

            Assert.Fail($"Expected SpriteRenderer child '{childName}'.");
            return null;
        }

        private static void AssertPixelSprite(string path, string expectedName)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            Assert.NotNull(sprite, $"Expected sprite at {path}.");
            Assert.That(sprite.name, Is.EqualTo(expectedName));
            Assert.That(sprite.texture.width, Is.EqualTo(192));
            Assert.That(sprite.texture.height, Is.EqualTo(192));

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            Assert.NotNull(importer);
            Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite));
            Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Point));
            Assert.IsFalse(importer.mipmapEnabled);
            Assert.That(importer.spritePixelsPerUnit, Is.EqualTo(32));
            Assert.That(importer.maxTextureSize, Is.GreaterThanOrEqualTo(192));
        }
    }
}
