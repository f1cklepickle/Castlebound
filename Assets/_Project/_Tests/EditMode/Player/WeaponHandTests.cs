using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.Combat;
using Castlebound.Gameplay.Inventory;

namespace Castlebound.Tests.Player
{
    public class WeaponHandTests
    {
        private class FakeWeaponDefinitionResolver : MonoBehaviour, IWeaponDefinitionResolver
        {
            private readonly Dictionary<string, WeaponDefinition> definitions =
                new Dictionary<string, WeaponDefinition>();

            public void SetDefinition(WeaponDefinition definition)
            {
                if (definition == null || string.IsNullOrWhiteSpace(definition.ItemId))
                {
                    return;
                }

                definitions[definition.ItemId] = definition;
            }

            public WeaponDefinition Resolve(string weaponId)
            {
                if (string.IsNullOrWhiteSpace(weaponId))
                {
                    return null;
                }

                WeaponDefinition definition;
                return definitions.TryGetValue(weaponId, out definition) ? definition : null;
            }
        }

        [Test]
        public void Initialize_ActiveWeapon_UpdatesSprite()
        {
            var root = new GameObject("Root");
            root.SetActive(false);
            var inventoryComponent = root.AddComponent<InventoryStateComponent>();
            var inventory = inventoryComponent.State;
            inventory.AddWeapon("weapon_a");
            inventory.SetActiveWeaponSlot(0);

            var resolverObject = new GameObject("Resolver");
            var resolver = resolverObject.AddComponent<FakeWeaponDefinitionResolver>();
            var weaponA = CreateWeaponDefinition("weapon_a", CreateSprite(8, new Vector2(0.5f, 0.5f)));
            resolver.SetDefinition(weaponA);

            var handObject = new GameObject("WeaponHand");
            handObject.transform.SetParent(root.transform, false);
            var spriteRenderer = handObject.AddComponent<SpriteRenderer>();

            var hand = root.AddComponent<WeaponHand>();
            SetPrivateField(hand, "inventorySource", inventoryComponent);
            SetPrivateField(hand, "resolverSource", resolver);
            SetPrivateField(hand, "spriteRenderer", spriteRenderer);

            hand.Initialize();

            Assert.AreEqual(weaponA.HandSprite, spriteRenderer.sprite);

            Object.DestroyImmediate(root);
            Object.DestroyImmediate(resolverObject);
            Object.DestroyImmediate(weaponA);
        }

        [Test]
        public void Initialize_EmptySlot_ClearsSprite()
        {
            var root = new GameObject("Root");
            root.SetActive(false);
            var inventoryComponent = root.AddComponent<InventoryStateComponent>();
            var inventory = inventoryComponent.State;
            inventory.AddWeapon("weapon_a");
            inventory.SetActiveWeaponSlot(1);

            var resolverObject = new GameObject("Resolver");
            var resolver = resolverObject.AddComponent<FakeWeaponDefinitionResolver>();
            var weaponA = CreateWeaponDefinition("weapon_a", CreateSprite(8, new Vector2(0.5f, 0.5f)));
            resolver.SetDefinition(weaponA);

            var handObject = new GameObject("WeaponHand");
            handObject.transform.SetParent(root.transform, false);
            var spriteRenderer = handObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = weaponA.HandSprite;
            spriteRenderer.enabled = true;

            var hand = root.AddComponent<WeaponHand>();
            SetPrivateField(hand, "inventorySource", inventoryComponent);
            SetPrivateField(hand, "resolverSource", resolver);
            SetPrivateField(hand, "spriteRenderer", spriteRenderer);

            hand.Initialize();

            Assert.IsNull(spriteRenderer.sprite);
            Assert.IsFalse(spriteRenderer.enabled);

            Object.DestroyImmediate(root);
            Object.DestroyImmediate(resolverObject);
            Object.DestroyImmediate(weaponA);
        }

        [Test]
        public void InventoryChange_RefreshesSpriteOnActiveSlotSwap()
        {
            var root = new GameObject("Root");
            root.SetActive(false);
            var inventoryComponent = root.AddComponent<InventoryStateComponent>();
            var inventory = inventoryComponent.State;
            inventory.AddWeapon("weapon_a");
            inventory.AddWeapon("weapon_b");
            inventory.SetActiveWeaponSlot(0);

            var resolverObject = new GameObject("Resolver");
            var resolver = resolverObject.AddComponent<FakeWeaponDefinitionResolver>();
            var weaponA = CreateWeaponDefinition("weapon_a", CreateSprite(8, new Vector2(0.5f, 0.5f)));
            var weaponB = CreateWeaponDefinition("weapon_b", CreateSprite(8, new Vector2(0.5f, 0.5f)));
            resolver.SetDefinition(weaponA);
            resolver.SetDefinition(weaponB);

            var handObject = new GameObject("WeaponHand");
            handObject.transform.SetParent(root.transform, false);
            var spriteRenderer = handObject.AddComponent<SpriteRenderer>();

            var hand = root.AddComponent<WeaponHand>();
            SetPrivateField(hand, "inventorySource", inventoryComponent);
            SetPrivateField(hand, "resolverSource", resolver);
            SetPrivateField(hand, "spriteRenderer", spriteRenderer);

            hand.Initialize();

            inventory.SetActiveWeaponSlot(1);

            Assert.AreEqual(weaponB.HandSprite, spriteRenderer.sprite);

            Object.DestroyImmediate(root);
            Object.DestroyImmediate(resolverObject);
            Object.DestroyImmediate(weaponA);
            Object.DestroyImmediate(weaponB);
        }

        [Test]
        public void Initialize_UsesHandSocketTransform_AndAppliesHandleOffset()
        {
            var root = new GameObject("Root");
            root.SetActive(false);
            var inventoryComponent = root.AddComponent<InventoryStateComponent>();
            var inventory = inventoryComponent.State;
            inventory.AddWeapon("weapon_a");
            inventory.SetActiveWeaponSlot(0);

            var resolverObject = new GameObject("Resolver");
            var resolver = resolverObject.AddComponent<FakeWeaponDefinitionResolver>();
            var weaponA = CreateWeaponDefinition("weapon_a", CreateSprite(10, new Vector2(0.5f, 0.5f)));
            weaponA.HandleOffset = new Vector2(0.25f, -0.5f);
            resolver.SetDefinition(weaponA);

            var handSocket = new GameObject("HandSocket");
            handSocket.transform.SetParent(root.transform, false);

            var handObject = new GameObject("WeaponHand");
            handObject.transform.SetParent(handSocket.transform, false);
            var spriteRenderer = handObject.AddComponent<SpriteRenderer>();

            var hand = root.AddComponent<WeaponHand>();
            SetPrivateField(hand, "inventorySource", inventoryComponent);
            SetPrivateField(hand, "resolverSource", resolver);
            SetPrivateField(hand, "spriteRenderer", spriteRenderer);
            SetPrivateField(hand, "handSocket", handSocket.transform);

            hand.Initialize();

            Assert.AreEqual(new Vector3(weaponA.HandleOffset.x, weaponA.HandleOffset.y, 0f),
                spriteRenderer.transform.localPosition);

            Object.DestroyImmediate(root);
            Object.DestroyImmediate(resolverObject);
            Object.DestroyImmediate(weaponA);
        }

        private static WeaponDefinition CreateWeaponDefinition(string id, Sprite sprite)
        {
            var definition = ScriptableObject.CreateInstance<WeaponDefinition>();
            definition.ItemId = id;
            definition.HandSprite = sprite;
            return definition;
        }

        private static Sprite CreateSprite(int size, Vector2 pivot)
        {
            var texture = new Texture2D(size, size);
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), pivot);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                field.SetValue(target, value);
            }
        }
    }
}
