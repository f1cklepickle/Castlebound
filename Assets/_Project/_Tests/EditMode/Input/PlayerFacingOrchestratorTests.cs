using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Input
{
    public class PlayerFacingOrchestratorTests
    {
        [Test]
        public void Tick_AppliesResolvedDirectionWithoutOwningSourceSelection()
        {
            var player = new GameObject("Player");

            try
            {
                var orchestrator = new PlayerFacingOrchestrator();

                orchestrator.Tick(player.transform, new Vector2(2f, 0f), 1f);

                Assert.Less(Vector2.Distance(orchestrator.LastFacingDirection, Vector2.right), 0.001f);
                Assert.That(
                    Quaternion.Angle(player.transform.rotation, Quaternion.Euler(0f, 0f, -90f)),
                    Is.LessThan(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(player);
            }
        }
    }
}
