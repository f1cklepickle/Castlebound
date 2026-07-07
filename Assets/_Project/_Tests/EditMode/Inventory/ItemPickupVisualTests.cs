using Castlebound.Gameplay.Inventory;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Inventory
{
    public class ItemPickupVisualTests
    {
        [Test]
        public void Refresh_UsesAssignedItemDefinitionIcon()
        {
            var root = new GameObject("Pickup");
            var pickup = root.AddComponent<ItemPickupComponent>();
            var renderer = root.AddComponent<SpriteRenderer>();
            var visual = root.AddComponent<ItemPickupVisual>();
            var definition = ScriptableObject.CreateInstance<WeaponDefinition>();
            var sprite = CreateSprite();
            definition.Icon = sprite;
            pickup.ItemDefinition = definition;

            visual.Refresh();

            Assert.That(renderer.sprite, Is.SameAs(sprite));
            Assert.That(renderer.enabled, Is.True);

            Object.DestroyImmediate(root);
            Object.DestroyImmediate(sprite.texture);
            Object.DestroyImmediate(sprite);
            Object.DestroyImmediate(definition);
        }

        [Test]
        public void Refresh_DisablesRenderer_WhenDefinitionHasNoIcon()
        {
            var root = new GameObject("Pickup");
            var pickup = root.AddComponent<ItemPickupComponent>();
            var renderer = root.AddComponent<SpriteRenderer>();
            var visual = root.AddComponent<ItemPickupVisual>();
            pickup.ItemDefinition = ScriptableObject.CreateInstance<WeaponDefinition>();

            visual.Refresh();

            Assert.That(renderer.sprite, Is.Null);
            Assert.That(renderer.enabled, Is.False);

            Object.DestroyImmediate(pickup.ItemDefinition);
            Object.DestroyImmediate(root);
        }

        private static Sprite CreateSprite()
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        }
    }
}
