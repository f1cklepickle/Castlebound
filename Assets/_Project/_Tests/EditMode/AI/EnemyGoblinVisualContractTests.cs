using System.IO;
using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Castlebound.Tests.AI
{
    public class EnemyGoblinVisualContractTests
    {
        private const string ArtRoot = "Assets/_Project/Art/Goblin_Assets";
        private const string PrefabPath = "Assets/_Project/Prefabs/Enemy.prefab";

        [TestCase("Goblin-Attack.png", 448, 64)]
        [TestCase("Goblin-Idle.png", 512, 128)]
        [TestCase("Goblin-Walk.png", 384, 64)]
        public void GoblinSheet_UsesPixelArtImportContract(string fileName, int width, int height)
        {
            string path = $"{ArtRoot}/{fileName}";
            Assert.IsTrue(File.Exists(path), $"Missing goblin sheet: {path}");

            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;

            Assert.NotNull(texture);
            Assert.NotNull(importer);
            Assert.That(texture.width, Is.EqualTo(width));
            Assert.That(texture.height, Is.EqualTo(height));
            Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Point));
            Assert.That(importer.textureCompression, Is.EqualTo(TextureImporterCompression.Uncompressed));
            Assert.IsFalse(importer.mipmapEnabled);
            Assert.IsTrue(importer.isReadable, "Runtime sheet slicing requires readable source textures.");
            Assert.That(importer.spritePixelsPerUnit, Is.EqualTo(32f));
        }

        [Test]
        public void EnemyPrefab_AuthorsGoblinPresentationReferences()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.NotNull(prefab);

            var presenter = prefab.GetComponent<EnemyAnimationPresenter>();
            Assert.NotNull(presenter);
            Assert.NotNull(presenter.IdleSheet);
            Assert.NotNull(presenter.WalkSheet);
            Assert.NotNull(presenter.AttackSheet);
            Assert.That(presenter.IdleFrameCount, Is.EqualTo(9));
            Assert.That(presenter.WalkFrameCount, Is.EqualTo(6));
            Assert.That(presenter.AttackFrameCount, Is.EqualTo(7));
            Assert.NotNull(presenter.AttackTiming);
            Assert.That(presenter.AttackTiming.ImpactHoldWindupRatio, Is.EqualTo(0.2f));
            Assert.That(presenter.AttackTiming.WindupFrameCount, Is.EqualTo(7));
            Assert.That(presenter.AttackTiming.ResolveFrame(0.15f, 0.3f, 7, 6), Is.EqualTo(3));
            Assert.That(presenter.AttackTiming.ResolveFrame(0.25f, 0.3f, 7, 6), Is.EqualTo(5));
            Assert.That(presenter.IdleDelaySeconds, Is.EqualTo(2f));

            var presenterData = new SerializedObject(presenter);
            Assert.That(presenterData.FindProperty("attackImpactFrameIndex").intValue, Is.EqualTo(6));

            var attack = prefab.GetComponent<EnemyAttack>();
            Assert.NotNull(attack);
            var attackData = new SerializedObject(attack);
            Assert.That(attackData.FindProperty("windupSeconds").floatValue, Is.EqualTo(0.3f));
        }

        [Test]
        public void CompletedAttack_SettlesOnNeutralFrameWithoutStartingIdleLoop()
        {
            GameObject prefabContents = PrefabUtility.LoadPrefabContents(PrefabPath);
            Assert.NotNull(prefabContents);

            try
            {
                var presenter = prefabContents.GetComponent<EnemyAnimationPresenter>();
                var targetRenderer = prefabContents.GetComponentInChildren<SpriteRenderer>();
                presenter.InitializePresentation();
                Sprite neutralFrame = targetRenderer.sprite;

                presenter.PlayAttack(0.3f);
                presenter.Advance(0.361f);

                Assert.That(presenter.CurrentState, Is.EqualTo(EnemyAnimationPresenter.PresentationState.Hold));
                Assert.That(targetRenderer.sprite, Is.SameAs(neutralFrame));
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabContents);
            }
        }
    }
}
