using System;
using System.Collections.Generic;
using System.Reflection;
using Castlebound.Gameplay.Castle;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Castle
{
    public class BarrierVisualBindingTests
    {
        [Test]
        public void BarrierVisualBinder_ApplySide_SetsExpectedSprite()
        {
            var binderType = ResolveGameplayType("Castlebound.Gameplay.Castle.BarrierVisualBinder");
            Assert.NotNull(binderType, "Expected BarrierVisualBinder to exist.");

            var go = new GameObject("Barrier");
            var renderer = go.AddComponent<SpriteRenderer>();
            var binder = go.AddComponent(binderType);

            var north = CreateSprite("North");
            var east = CreateSprite("East");
            var south = CreateSprite("South");
            var west = CreateSprite("West");

            try
            {
                SetMember(binderType, binder, "targetRenderer", renderer);
                SetMember(binderType, binder, "northSprite", north);
                SetMember(binderType, binder, "eastSprite", east);
                SetMember(binderType, binder, "southSprite", south);
                SetMember(binderType, binder, "westSprite", west);

                var applySide = binderType.GetMethod("ApplySide", BindingFlags.Public | BindingFlags.Instance);
                Assert.NotNull(applySide, "Expected public ApplySide(BarrierSide).");

                applySide.Invoke(binder, new object[] { BarrierSide.East });
                Assert.That(renderer.sprite, Is.EqualTo(east), "ApplySide(East) should assign eastSprite.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void BarrierVisualBinder_ApplySide_RotatesSystemsRoot_WithoutRotatingSpriteRendererTransform()
        {
            var binderType = ResolveGameplayType("Castlebound.Gameplay.Castle.BarrierVisualBinder");
            Assert.NotNull(binderType, "Expected BarrierVisualBinder to exist.");

            var root = new GameObject("Barrier");
            var spriteRoot = new GameObject("SpriteRoot");
            var systemsRoot = new GameObject("SystemsRoot");
            spriteRoot.transform.SetParent(root.transform, false);
            systemsRoot.transform.SetParent(root.transform, false);

            var renderer = spriteRoot.AddComponent<SpriteRenderer>();
            var binder = root.AddComponent(binderType);

            var north = CreateSprite("North");
            var east = CreateSprite("East");
            var south = CreateSprite("South");
            var west = CreateSprite("West");

            try
            {
                SetMember(binderType, binder, "targetRenderer", renderer);
                SetMember(binderType, binder, "northSprite", north);
                SetMember(binderType, binder, "eastSprite", east);
                SetMember(binderType, binder, "southSprite", south);
                SetMember(binderType, binder, "westSprite", west);
                SetMember(binderType, binder, "systemsRoot", systemsRoot.transform);

                var applySide = binderType.GetMethod("ApplySide", BindingFlags.Public | BindingFlags.Instance);
                Assert.NotNull(applySide, "Expected public ApplySide(BarrierSide).");

                applySide.Invoke(binder, new object[] { BarrierSide.East });

                Assert.That(renderer.sprite, Is.EqualTo(east), "ApplySide(East) should assign eastSprite.");
                var systemsZ = Mathf.DeltaAngle(0f, systemsRoot.transform.localEulerAngles.z);
                Assert.That(systemsZ, Is.EqualTo(-90f).Within(0.5f), "SystemsRoot should rotate to East.");
                var spriteZ = Mathf.DeltaAngle(0f, spriteRoot.transform.localEulerAngles.z);
                Assert.That(spriteZ, Is.EqualTo(0f).Within(0.5f), "SpriteRoot should remain unrotated.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void BarrierAssemblyBuilder_Rebuild_AppliesSlotSide_ToVisualBinder()
        {
            var binderType = ResolveGameplayType("Castlebound.Gameplay.Castle.BarrierVisualBinder");
            Assert.NotNull(binderType, "Expected BarrierVisualBinder to exist.");

            var parent = new GameObject("GeneratedBarriers");
            var prefab = new GameObject("BarrierPrefab");
            var renderer = prefab.AddComponent<SpriteRenderer>();
            var binder = prefab.AddComponent(binderType);

            var north = CreateSprite("North");
            var east = CreateSprite("East");
            var south = CreateSprite("South");
            var west = CreateSprite("West");

            try
            {
                SetMember(binderType, binder, "targetRenderer", renderer);
                SetMember(binderType, binder, "northSprite", north);
                SetMember(binderType, binder, "eastSprite", east);
                SetMember(binderType, binder, "southSprite", south);
                SetMember(binderType, binder, "westSprite", west);

                var slots = new List<BarrierPlacementSlot>
                {
                    new BarrierPlacementSlot { Id = "barrier_n", Position = Vector2.zero, Side = BarrierSide.North },
                    new BarrierPlacementSlot { Id = "barrier_s", Position = new Vector2(3f, 0f), Side = BarrierSide.South }
                };

                BarrierAssemblyBuilder.Rebuild(parent.transform, prefab, slots);

                var northInstance = parent.transform.Find("barrier_n");
                var southInstance = parent.transform.Find("barrier_s");
                Assert.NotNull(northInstance, "Expected north barrier instance.");
                Assert.NotNull(southInstance, "Expected south barrier instance.");

                var northRenderer = northInstance.GetComponent<SpriteRenderer>();
                var southRenderer = southInstance.GetComponent<SpriteRenderer>();
                Assert.That(northRenderer.sprite, Is.EqualTo(north), "North slot should apply northSprite.");
                Assert.That(southRenderer.sprite, Is.EqualTo(south), "South slot should apply southSprite.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(prefab);
                UnityEngine.Object.DestroyImmediate(parent);
            }
        }

        private static void SetMember(Type type, object instance, string name, object value)
        {
            var field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(instance, value);
                return;
            }

            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(instance, value);
                return;
            }

            Assert.Fail($"Expected member '{name}' on {type.Name}.");
        }

        private static Sprite CreateSprite(string name)
        {
            var texture = new Texture2D(2, 2);
            texture.name = $"{name}_Texture";
            var sprite = Sprite.Create(texture, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 32f);
            sprite.name = $"{name}_Sprite";
            return sprite;
        }

        private static Type ResolveGameplayType(string fullName)
        {
            return Type.GetType($"{fullName}, _Project.Gameplay");
        }
    }
}
