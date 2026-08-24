using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Castlebound.Tests.AI
{
    public class EnemySeparationPrefabContractTests
    {
        private const float PrimaryBodyRadius = 0.87684506f;
        private const float SeparationRadius = 0.2f;

        [TestCase("Assets/_Project/Prefabs/Enemy_Goblin_Melee.prefab")]
        [TestCase("Assets/_Project/Prefabs/Enemy_Goblin_Ranged.prefab")]
        [TestCase("Assets/_Project/Prefabs/Enemy_Lurker.prefab")]
        public void ActiveEnemyPrefab_IsolatesAConsistentSmallSeparationFootprint(
            string prefabPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            Assert.NotNull(prefab, $"Expected enemy prefab at {prefabPath}.");
            Assert.That(prefab.tag, Is.EqualTo("Enemy"));
            Assert.That(LayerMask.LayerToName(prefab.layer), Is.EqualTo("Enemies"));

            Rigidbody2D body = prefab.GetComponent<Rigidbody2D>();
            Assert.NotNull(body);
            Assert.That(body.bodyType, Is.EqualTo(RigidbodyType2D.Dynamic));
            Assert.That(body.collisionDetectionMode, Is.EqualTo(CollisionDetectionMode2D.Continuous));
            Assert.That(body.constraints & RigidbodyConstraints2D.FreezeRotation,
                Is.EqualTo(RigidbodyConstraints2D.FreezeRotation));

            CircleCollider2D primary = prefab.GetComponent<CircleCollider2D>();
            Assert.NotNull(primary);
            Assert.IsFalse(primary.isTrigger);
            Assert.That(primary.radius, Is.EqualTo(PrimaryBodyRadius).Within(0.000001f),
                "The existing combat/body footprint must not be resized.");

            var engagementData = new SerializedObject(prefab.GetComponent<EnemyEngagement>());
            Assert.That(
                engagementData.FindProperty("bodyCollider").objectReferenceValue,
                Is.SameAs(primary),
                "Engagement distance must continue to use the primary body collider.");

            Transform separationTransform = prefab.transform.Find("EnemySeparation");
            Assert.NotNull(separationTransform);
            Assert.That(LayerMask.LayerToName(separationTransform.gameObject.layer),
                Is.EqualTo("Enemies"));
            Assert.That(separationTransform.tag, Is.EqualTo("Untagged"),
                "The separation footprint must not become an Enemy-tagged hurtbox.");
            Assert.That(separationTransform.localPosition, Is.EqualTo(Vector3.zero));
            Assert.That(separationTransform.localScale, Is.EqualTo(Vector3.one));

            EnemySeparationCollider marker =
                separationTransform.GetComponent<EnemySeparationCollider>();
            CircleCollider2D separation = separationTransform.GetComponent<CircleCollider2D>();
            Assert.NotNull(marker);
            Assert.NotNull(separation);
            Assert.That(marker.Collider, Is.SameAs(separation));
            Assert.IsTrue(separation.isTrigger,
                "The separation footprint must detect overlaps without physical impulses.");
            Assert.That(separation.radius, Is.EqualTo(SeparationRadius).Within(0.000001f));
            Assert.That(separation.radius, Is.LessThan(primary.radius * 0.25f));

            SpriteRenderer sprite = prefab.GetComponentInChildren<SpriteRenderer>(true);
            Assert.NotNull(sprite);
            Assert.NotNull(sprite.sprite);
            Assert.That(separation.radius * 2f,
                Is.LessThan(Mathf.Min(sprite.sprite.bounds.size.x, sprite.sprite.bounds.size.y)),
                "The physical minimum footprint must still permit visible sprite overlap.");

            Assert.IsNull(separation.sharedMaterial,
                "A non-impulse trigger must not retain the obsolete solid-contact material.");

            int enemiesMask = 1 << LayerMask.NameToLayer("Enemies");
            Assert.That(separation.includeLayers.value, Is.EqualTo(enemiesMask));
            Assert.That(separation.excludeLayers.value, Is.EqualTo(~enemiesMask));
            Assert.That(separation.forceSendLayers.value, Is.Zero,
                "The sensor must not transmit knockback or separation impulses.");
            Assert.That(separation.forceReceiveLayers.value, Is.Zero,
                "The sensor must not receive knockback or separation impulses.");
            Assert.That(separation.contactCaptureLayers.value, Is.EqualTo(enemiesMask));
            Assert.That(separation.callbackLayers.value, Is.EqualTo(enemiesMask),
                "Only Enemy overlaps may invoke bounded depenetration.");
            Assert.That(primary.includeLayers.value & enemiesMask, Is.Zero);
            Assert.That(primary.excludeLayers.value & enemiesMask, Is.EqualTo(enemiesMask));
            Assert.That(primary.layerOverridePriority,
                Is.GreaterThan(separation.layerOverridePriority),
                "Primary-vs-separation conflicts must preserve the primary exclusion.");

            Collider2D[] colliders = prefab.GetComponentsInChildren<Collider2D>(true);
            Assert.That(colliders, Has.Length.EqualTo(2));
            Assert.That(colliders, Does.Contain(primary));
            Assert.That(colliders, Does.Contain(separation));
        }

        [Test]
        public void ProjectCollisionContract_LeavesEnemiesSelfCollisionDisabledGlobally()
        {
            int enemiesLayer = LayerMask.NameToLayer("Enemies");

            Assert.That(enemiesLayer, Is.EqualTo(7),
                "Issue #277 must not add or remap project layers.");
            Assert.IsTrue(Physics2D.GetIgnoreLayerCollision(enemiesLayer, enemiesLayer),
                "Enemy self-contact must remain a per-collider override, not a global matrix change.");
            Assert.That(LayerMask.NameToLayer("Player"), Is.EqualTo(3));
            Assert.That(LayerMask.NameToLayer("Walls"), Is.EqualTo(6));
            Assert.That(LayerMask.NameToLayer("Environment"), Is.EqualTo(11));
        }
    }
}
