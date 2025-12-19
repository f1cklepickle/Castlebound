using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

namespace Castlebound.Tests.AI
{
    public class EnemyControllerValidationTests
    {
        [Test]
        public void AssignsPlayerByTag_WhenReferenceMissing()
        {
            // Arrange: isolate tag so only our player is discoverable.
            var existingPlayers = GameObject.FindGameObjectsWithTag("Player");
            var restoredTags = new System.Collections.Generic.List<(GameObject go, string tag)>(existingPlayers.Length);
            foreach (var go in existingPlayers)
            {
                restoredTags.Add((go, go.tag));
                go.tag = "Untagged";
            }

            var player = new GameObject("Player");
            player.tag = "Player";

            var enemyGO = new GameObject("Enemy");
            enemyGO.AddComponent<Rigidbody2D>();
            var controller = enemyGO.AddComponent<EnemyController2D>();

            try
            {
                // Act: trigger lookup via debug helper.
                controller.Debug_EnsurePlayerReference();

                // Assert: player reference is picked up by tag and target set.
                var assignedPlayer = GetPrivateField<Transform>(controller, "player");
                Assert.IsNotNull(assignedPlayer, "Controller should assign player reference by Player tag if missing.");
                Assert.AreSame(player.transform, assignedPlayer, "Controller should bind to the tagged Player we created.");
                Assert.AreSame(player.transform, controller.Target, "Controller target should be set to the found player.");
            }
            finally
            {
                Object.DestroyImmediate(enemyGO);
                Object.DestroyImmediate(player);

                // Restore original player tags.
                foreach (var entry in restoredTags)
                {
                    if (entry.go != null)
                    {
                        entry.go.tag = entry.tag;
                    }
                }
            }
        }

        private static T GetPrivateField<T>(object obj, string fieldName) where T : class
        {
            var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return field?.GetValue(obj) as T;
        }
    }
}
