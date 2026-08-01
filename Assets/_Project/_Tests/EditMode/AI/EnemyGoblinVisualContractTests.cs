using System.IO;
using System.Linq;
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

        [TestCase("Goblin-Attack.png", 7)]
        [TestCase("Goblin-Idle.png", 9)]
        [TestCase("Goblin-Walk.png", 6)]
        public void GoblinSheet_UsesMultiSpritePixelArtImportContract(string fileName, int frameCount)
        {
            string path = $"{ArtRoot}/{fileName}";
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            Assert.NotNull(importer);
            Assert.That(importer.spriteImportMode, Is.EqualTo(SpriteImportMode.Multiple));
            Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Point));
            Assert.That(importer.textureCompression, Is.EqualTo(TextureImporterCompression.Uncompressed));
            Assert.IsFalse(importer.mipmapEnabled);
            Assert.IsFalse(importer.isReadable, "Authored sprite clips do not require readable textures.");
            Assert.That(importer.spritePixelsPerUnit, Is.EqualTo(32f));
            Assert.That(AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().Count(), Is.EqualTo(frameCount));
        }

        [TestCase("Goblin_Idle.anim", 9)]
        [TestCase("Goblin_Walk.anim", 6)]
        [TestCase("Goblin_Attack.anim", 7)]
        public void GoblinClip_AnimatesEveryAuthoredSprite(string fileName, int frameCount)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{ArtRoot}/{fileName}");
            Assert.NotNull(clip);
            var spriteBinding = AnimationUtility.GetObjectReferenceCurveBindings(clip)
                .Single(binding => binding.path == "VisualRoot/Sprite" && binding.propertyName == "m_Sprite");
            Assert.That(AnimationUtility.GetObjectReferenceCurve(clip, spriteBinding).Length, Is.EqualTo(frameCount));
            Assert.That(AnimationUtility.GetCurveBindings(clip).Count(binding => binding.path == "VisualRoot/HandSocket"),
                Is.GreaterThanOrEqualTo(3));
        }

        [Test]
        public void AttackClip_AnimatesRealHandSocketPositionAndRotation()
        {
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{ArtRoot}/Goblin_Attack.anim");
            string[] properties = AnimationUtility.GetCurveBindings(clip)
                .Where(binding => binding.path == "VisualRoot/HandSocket")
                .Select(binding => binding.propertyName)
                .ToArray();
            CollectionAssert.Contains(properties, "m_LocalPosition.x");
            CollectionAssert.Contains(properties, "m_LocalPosition.y");
            CollectionAssert.Contains(properties, "localEulerAnglesRaw.z");
        }

        [Test]
        public void EnemyPrefab_AuthorsAnimatorSocketAndUnarmedSpawnEquipment()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.NotNull(prefab);
            Assert.NotNull(prefab.GetComponent<Animator>()?.runtimeAnimatorController);
            Transform visualRoot = prefab.transform.Find("VisualRoot");
            Assert.NotNull(visualRoot);
            Assert.That(prefab.GetComponent<EnemyFacing>().VisualTransform, Is.EqualTo(visualRoot));
            Assert.NotNull(prefab.transform.Find("VisualRoot/HandSocket"));
            Assert.NotNull(prefab.transform.Find("VisualRoot/HandSocket/Weapon")?.GetComponent<SpriteRenderer>());
            Assert.That(prefab.GetComponent<EnemyEquipment>().SpawnEquipment.EquipmentId, Is.EqualTo("unarmed"));
            var presenter = prefab.GetComponent<EnemyAnimationPresenter>();
            Assert.That(presenter.AuthoredImpactTimeSeconds, Is.EqualTo(20f / 60f).Within(0.0001f));

            var attackClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{ArtRoot}/Goblin_Attack.anim");
            var spriteBinding = AnimationUtility.GetObjectReferenceCurveBindings(attackClip)
                .Single(binding => binding.path == "VisualRoot/Sprite" && binding.propertyName == "m_Sprite");
            var spriteKeys = AnimationUtility.GetObjectReferenceCurve(attackClip, spriteBinding);
            Assert.That(spriteKeys.Last().time, Is.EqualTo(presenter.AuthoredImpactTimeSeconds).Within(0.0001f));
            Assert.That(spriteKeys.Select(key => key.time).Distinct().Count(), Is.EqualTo(spriteKeys.Length));
        }

        [Test]
        public void RuntimeSpriteSlicing_IsRemoved()
        {
            string presenterSource = File.ReadAllText(
                "Assets/_Project/Scripts/_Project.Gameplay/AI/EnemyAnimationPresenter.cs");
            StringAssert.DoesNotContain("Sprite.Create", presenterSource);
            StringAssert.DoesNotContain("Texture2D", presenterSource);

            string attackSource = File.ReadAllText(
                "Assets/_Project/Scripts/_Project.Gameplay/Combat/EnemyAttack.cs");
            StringAssert.DoesNotContain("SetTrigger", attackSource,
                "EnemyAttack must delegate presentation without owning Animator triggers.");
        }
    }
}
