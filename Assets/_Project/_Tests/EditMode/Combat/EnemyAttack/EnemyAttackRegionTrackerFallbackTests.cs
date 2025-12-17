using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Castlebound.Gameplay.AI;
using System.Reflection;

namespace Castlebound.Tests.Combat
{
    public class EnemyAttackRegionTrackerFallbackTests
    {
        [Test]
        public void BarrierGate_TreatsMissingRegionTrackerAsOutside_AndWarns()
        {
            // Ensure no tracker instance.
            SetRegionTrackerInstance(null);

            var enemy = new GameObject("Enemy");
            enemy.AddComponent<Rigidbody2D>();
            var controller = enemy.AddComponent<EnemyController2D>();
            var attack = enemy.AddComponent<EnemyAttack>();

            // Simulate selector decision: barrier target type.
            controller.Debug_SetupRefs(null, null);
            typeof(EnemyController2D)
                .GetField("_currentTargetType", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(controller, EnemyTargetType.Barrier);

            EnemyAttack.Debug_ResetMissingRegionWarning();
            LogAssert.Expect(LogType.Warning, "[EnemyAttack] CastleRegionTracker.Instance is missing; treating enemy/player as outside for barrier gating.");

            attack.Debug_GetRegionState(out bool enemyInside, out bool playerInside);

            Assert.IsFalse(enemyInside, "Missing tracker should default enemyInside to false.");
            Assert.IsFalse(playerInside, "Missing tracker should default playerInside to false.");

            Object.DestroyImmediate(enemy);
        }

        private static void SetRegionTrackerInstance(CastleRegionTracker value)
        {
            var prop = typeof(CastleRegionTracker).GetProperty("Instance", BindingFlags.Static | BindingFlags.Public);
            var backingField = typeof(CastleRegionTracker).GetField("<Instance>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic);
            if (backingField != null)
            {
                backingField.SetValue(null, value);
            }
            else if (prop != null && prop.CanWrite)
            {
                prop.SetValue(null, value, null);
            }
        }
    }
}
