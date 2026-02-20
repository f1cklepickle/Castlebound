using System;
using System.Collections;
using System.Reflection;
using Castlebound.Gameplay.Castle;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Castle
{
    public class BarrierAssemblyBuilderTests
    {
        [TestCase("Barrier_Top", "North")]
        [TestCase("Barrier_Bottom", "South")]
        [TestCase("Barrier_Left", "West")]
        [TestCase("Barrier_Right", "East")]
        public void BarrierTileSideMapper_MapsTileNameToExpectedSide(string tileName, string expectedSide)
        {
            var mapperType = ResolveGameplayType("Castlebound.Gameplay.Castle.BarrierTileSideMapper");
            Assert.NotNull(mapperType, "Expected BarrierTileSideMapper to exist.");

            var method = mapperType.GetMethod("TryMapTileNameToSide", BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(method, "Expected static TryMapTileNameToSide(string, out BarrierSide).");

            var args = new object[] { tileName, null };
            var mapped = (bool)method.Invoke(null, args);

            Assert.IsTrue(mapped, $"Expected '{tileName}' to map to a barrier side.");
            Assert.NotNull(args[1], $"Expected mapped side output for '{tileName}'.");
            Assert.That(args[1].ToString(), Is.EqualTo(expectedSide));
        }

        [Test]
        public void BarrierAssemblyBuilder_Rebuild_SpawnsOneBarrierPerSlot_AndIsIdempotent()
        {
            var builderType = ResolveGameplayType("Castlebound.Gameplay.Castle.BarrierAssemblyBuilder");
            var slotType = ResolveGameplayType("Castlebound.Gameplay.Castle.BarrierPlacementSlot");
            var sideType = ResolveGameplayType("Castlebound.Gameplay.Castle.BarrierSide");

            Assert.NotNull(builderType, "Expected BarrierAssemblyBuilder to exist.");
            Assert.NotNull(slotType, "Expected BarrierPlacementSlot to exist.");
            Assert.NotNull(sideType, "Expected BarrierSide enum to exist.");

            var rebuild = builderType.GetMethod("Rebuild", BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(rebuild, "Expected static Rebuild(Transform, GameObject, IEnumerable<BarrierPlacementSlot>).");

            var parentGo = new GameObject("GeneratedBarriers");
            var prefabGo = new GameObject("Barrier_Gate_Template");
            try
            {
                var slots = CreateSlotsArray(
                    slotType,
                    sideType,
                    ("barrier_n", new Vector2(0f, 0f), "North"),
                    ("barrier_e", new Vector2(3f, 0f), "East"));

                rebuild.Invoke(null, new object[] { parentGo.transform, prefabGo, slots });
                Assert.That(parentGo.transform.childCount, Is.EqualTo(2), "Expected one spawned barrier per slot.");

                rebuild.Invoke(null, new object[] { parentGo.transform, prefabGo, slots });
                Assert.That(parentGo.transform.childCount, Is.EqualTo(2), "Rebuild should be idempotent and not duplicate barriers.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(prefabGo);
                UnityEngine.Object.DestroyImmediate(parentGo);
            }
        }

        [Test]
        public void BarrierAssemblyBuilder_Rebuild_PlacesBarriersOnThreeUnitLattice()
        {
            var builderType = ResolveGameplayType("Castlebound.Gameplay.Castle.BarrierAssemblyBuilder");
            var slotType = ResolveGameplayType("Castlebound.Gameplay.Castle.BarrierPlacementSlot");
            var sideType = ResolveGameplayType("Castlebound.Gameplay.Castle.BarrierSide");

            Assert.NotNull(builderType, "Expected BarrierAssemblyBuilder to exist.");
            Assert.NotNull(slotType, "Expected BarrierPlacementSlot to exist.");
            Assert.NotNull(sideType, "Expected BarrierSide enum to exist.");

            var rebuild = builderType.GetMethod("Rebuild", BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(rebuild, "Expected static Rebuild(Transform, GameObject, IEnumerable<BarrierPlacementSlot>).");

            var parentGo = new GameObject("GeneratedBarriers");
            var prefabGo = new GameObject("Barrier_Gate_Template");
            try
            {
                var slots = CreateSlotsArray(
                    slotType,
                    sideType,
                    ("barrier_n", new Vector2(0f, 0f), "North"),
                    ("barrier_w", new Vector2(-3f, 3f), "West"));

                rebuild.Invoke(null, new object[] { parentGo.transform, prefabGo, slots });

                foreach (Transform child in parentGo.transform)
                {
                    var worldPos = (Vector2)child.position;
                    Assert.IsTrue(
                        CastlePlacementRules.IsOnLattice(worldPos),
                        $"Spawned barrier '{child.name}' must land on 3-unit lattice. Position=({worldPos.x:0.###}, {worldPos.y:0.###})");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(prefabGo);
                UnityEngine.Object.DestroyImmediate(parentGo);
            }
        }

        private static Array CreateSlotsArray(Type slotType, Type sideType, params (string id, Vector2 position, string side)[] entries)
        {
            var array = Array.CreateInstance(slotType, entries.Length);
            for (var i = 0; i < entries.Length; i++)
            {
                var slot = Activator.CreateInstance(slotType);
                Assert.NotNull(slot, "Expected BarrierPlacementSlot to provide a parameterless constructor.");

                SetProperty(slotType, slot, "Id", entries[i].id);
                SetProperty(slotType, slot, "Position", entries[i].position);

                var sideValue = Enum.Parse(sideType, entries[i].side);
                SetProperty(slotType, slot, "Side", sideValue);

                array.SetValue(slot, i);
            }

            return array;
        }

        private static void SetProperty(Type type, object instance, string name, object value)
        {
            var property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(property, $"Expected property '{name}' on {type.Name}.");
            Assert.IsTrue(property.CanWrite, $"Expected property '{name}' on {type.Name} to be settable.");
            property.SetValue(instance, value);
        }

        private static Type ResolveGameplayType(string fullName)
        {
            return Type.GetType($"{fullName}, _Project.Gameplay");
        }
    }
}
