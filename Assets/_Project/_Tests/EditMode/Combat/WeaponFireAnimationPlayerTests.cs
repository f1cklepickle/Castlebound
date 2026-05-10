using Castlebound.Gameplay.Combat;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Combat
{
    public class WeaponFireAnimationPlayerTests
    {
        [Test]
        public void CalculatePlaybackSpeed_ScalesFromCooldownAndClamps()
        {
            var go = new GameObject("WeaponFireAnimationPlayer");
            var player = go.AddComponent<WeaponFireAnimationPlayer>();
            player.ReferenceCooldownSeconds = 1f;
            player.MinPlaybackSpeed = 0.5f;
            player.MaxPlaybackSpeed = 3f;

            try
            {
                Assert.That(player.CalculatePlaybackSpeed(1f), Is.EqualTo(1f).Within(0.001f));
                Assert.That(player.CalculatePlaybackSpeed(0.5f), Is.EqualTo(2f).Within(0.001f));
                Assert.That(player.CalculatePlaybackSpeed(4f), Is.EqualTo(0.5f).Within(0.001f));
                Assert.That(player.CalculatePlaybackSpeed(0.1f), Is.EqualTo(3f).Within(0.001f));
                Assert.That(player.CalculatePlaybackSpeed(0f), Is.EqualTo(3f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
