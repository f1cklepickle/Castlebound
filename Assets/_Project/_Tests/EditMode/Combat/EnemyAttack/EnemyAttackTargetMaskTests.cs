using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.AI;

namespace Castlebound.Tests.Combat
{
    public class EnemyAttackTargetMaskTests
    {
        [Test]
        public void AssignsPlayerLayerMask_WhenUnset()
        {
            var enemy = new GameObject("Enemy");
            enemy.AddComponent<Rigidbody2D>();
            enemy.AddComponent<EnemyController2D>(); // required component
            var attack = enemy.AddComponent<EnemyAttack>();

            // Simulate default-unset mask
            var maskField = typeof(EnemyAttack).GetField("targetMask", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            maskField?.SetValue(attack, new LayerMask { value = 0 });

            attack.Debug_EnsureTargetMask();

            int expected = LayerMask.GetMask("Player");
            Assert.AreEqual(expected, attack.GetType().GetField("targetMask", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(attack) is LayerMask lm ? lm.value : -1,
                "EnemyAttack should default the target mask to Player layer when unset.");

            Object.DestroyImmediate(enemy);
        }

        [Test]
        public void PreservesMask_WhenAlreadySet()
        {
            var enemy = new GameObject("Enemy");
            enemy.AddComponent<Rigidbody2D>();
            enemy.AddComponent<EnemyController2D>(); // required component
            var attack = enemy.AddComponent<EnemyAttack>();

            int customMask = LayerMask.GetMask("Default");
            var maskField = typeof(EnemyAttack).GetField("targetMask", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            maskField?.SetValue(attack, new LayerMask { value = customMask });

            attack.Debug_EnsureTargetMask();

            int value = maskField?.GetValue(attack) is LayerMask lm ? lm.value : -1;
            Assert.AreEqual(customMask, value, "EnemyAttack should not override target mask if it is already set.");

            Object.DestroyImmediate(enemy);
        }
    }
}
