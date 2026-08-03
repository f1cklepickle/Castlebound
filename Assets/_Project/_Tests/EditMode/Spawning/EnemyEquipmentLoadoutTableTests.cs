using Castlebound.Gameplay.AI;
using Castlebound.Gameplay.Spawning;
using NUnit.Framework;
using UnityEngine;
using Random = System.Random;

namespace Castlebound.Tests.Spawning
{
    public class EnemyEquipmentLoadoutTableTests
    {
        [Test]
        public void ClubChance_RampsLinearlyFromTwentyPercentToCertainByWaveTen()
        {
            var unarmed = ScriptableObject.CreateInstance<EnemyEquipmentDefinition>();
            var club = ScriptableObject.CreateInstance<EnemyEquipmentDefinition>();
            var table = ScriptableObject.CreateInstance<EnemyEquipmentLoadoutTable>();

            try
            {
                table.Entries = new[]
                {
                    new EnemyEquipmentLoadoutEntry(unarmed, 1, 80f, 10, 0f),
                    new EnemyEquipmentLoadoutEntry(club, 1, 20f, 10, 100f)
                };

                Assert.That(table.GetSelectionChance(club, 1), Is.EqualTo(0.2f).Within(0.0001f));
                Assert.That(table.GetSelectionChance(club, 5), Is.EqualTo(5f / 9f).Within(0.0001f));
                Assert.That(table.GetSelectionChance(club, 10), Is.EqualTo(1f).Within(0.0001f));
                Assert.That(table.GetSelectionChance(club, 20), Is.EqualTo(1f).Within(0.0001f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(table);
                UnityEngine.Object.DestroyImmediate(club);
                UnityEngine.Object.DestroyImmediate(unarmed);
            }
        }

        [Test]
        public void Select_SameSeedAndWave_ReplaysSameEquipmentOrder()
        {
            var unarmed = ScriptableObject.CreateInstance<EnemyEquipmentDefinition>();
            var club = ScriptableObject.CreateInstance<EnemyEquipmentDefinition>();
            var table = ScriptableObject.CreateInstance<EnemyEquipmentLoadoutTable>();

            try
            {
                table.Entries = new[]
                {
                    new EnemyEquipmentLoadoutEntry(unarmed, 1, 80f, 10, 0f),
                    new EnemyEquipmentLoadoutEntry(club, 1, 20f, 10, 100f)
                };
                var firstRandom = new Random(236);
                var replayRandom = new Random(236);

                for (int i = 0; i < 20; i++)
                {
                    Assert.That(table.Select(firstRandom, 4), Is.SameAs(table.Select(replayRandom, 4)));
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(table);
                UnityEngine.Object.DestroyImmediate(club);
                UnityEngine.Object.DestroyImmediate(unarmed);
            }
        }

        [Test]
        public void Select_NoPositiveValidWeight_ReturnsNoOverride()
        {
            var table = ScriptableObject.CreateInstance<EnemyEquipmentLoadoutTable>();
            try
            {
                table.Entries = System.Array.Empty<EnemyEquipmentLoadoutEntry>();

                Assert.That(table.Select(new Random(1), 1), Is.Null);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(table);
            }
        }
    }
}
