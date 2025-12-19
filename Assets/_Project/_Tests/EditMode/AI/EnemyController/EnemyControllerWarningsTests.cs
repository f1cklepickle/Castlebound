using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Castlebound.Tests.AI
{
    public class EnemyControllerWarningsTests
    {
        [Test]
        public void LogsWarning_WhenPlayerMissing()
        {
            // Detag any existing players.
            foreach (var go in GameObject.FindGameObjectsWithTag("Player"))
            {
                go.tag = "Untagged";
            }

            var enemy = new GameObject("Enemy");
            enemy.AddComponent<Rigidbody2D>();
            var controller = enemy.AddComponent<EnemyController2D>();

            LogAssert.Expect(LogType.Warning, "[EnemyController2D] Player reference not found in scene. Enemy will have no target.");
            controller.Debug_ValidateRefs();

            Object.DestroyImmediate(enemy);
        }

        [Test]
        public void LogsWarning_WhenHomeBarrierMissing()
        {
            // No barriers in scene; ensure player exists so only barrier warning fires.
            var player = new GameObject("Player");
            player.tag = "Player";

            var enemy = new GameObject("Enemy");
            enemy.AddComponent<Rigidbody2D>();
            var controller = enemy.AddComponent<EnemyController2D>();

            // Force barrier targeting on.
            var useBarrierField = typeof(EnemyController2D).GetField("useBarrierTargeting", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            useBarrierField?.SetValue(controller, true);

            LogAssert.Expect(LogType.Warning, "[EnemyController2D] No home barrier found while barrier targeting is enabled.");
            controller.Debug_ValidateRefs();

            Object.DestroyImmediate(enemy);
            Object.DestroyImmediate(player);
        }
    }
}
