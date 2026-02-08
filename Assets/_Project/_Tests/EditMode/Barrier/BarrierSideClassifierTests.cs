using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.Barrier;
using Castlebound.Gameplay.AI;

namespace Castlebound.Tests.Gate
{
    public class BarrierSideClassifierTests
    {
        [Test]
        public void IsEnemyPastThreshold_ReturnsTrue_WhenEnemyIsPastInnerThreshold()
        {
            var barrier = new GameObject("Barrier");
            var barrierCollider = barrier.AddComponent<BoxCollider2D>();
            barrierCollider.size = new Vector2(2f, 2f);

            var hold = barrier.AddComponent<EnemyBarrierHoldBehavior>();
            var anchor = new GameObject("Anchor");
            anchor.transform.position = new Vector2(2f, 0f);
            hold.Debug_SetAnchor(anchor.transform);

            var enemy = new GameObject("Enemy");
            var enemyCollider = enemy.AddComponent<BoxCollider2D>();
            enemy.transform.position = new Vector2(0.5f, 0f);
            Physics2D.SyncTransforms();

            var classifier = ResolveClassifierType();
            Assert.NotNull(classifier, "Expected Castlebound.Gameplay.Barrier.BarrierSideClassifier.");

            var method = classifier.GetMethod("IsEnemyPastThreshold", BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(method, "Expected public static IsEnemyPastThreshold method.");

            bool result = (bool)method.Invoke(null, new object[] { barrierCollider, enemyCollider, 0.5f });
            Assert.IsTrue(result, "Enemy should be classified as past threshold.");

            UnityEngine.Object.DestroyImmediate(anchor);
            UnityEngine.Object.DestroyImmediate(enemy);
            UnityEngine.Object.DestroyImmediate(barrier);
        }

        private static Type ResolveClassifierType()
        {
            return Type.GetType("Castlebound.Gameplay.Barrier.BarrierSideClassifier, _Project.Gameplay");
        }
    }
}
