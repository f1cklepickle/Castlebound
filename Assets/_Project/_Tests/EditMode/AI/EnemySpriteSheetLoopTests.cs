using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.AI
{
    public class EnemySpriteSheetLoopTests
    {
        [Test]
        public void Configure_SlicesSheetAndAppliesFirstFrame()
        {
            var root = new GameObject("SpriteLoop");
            var renderer = root.AddComponent<SpriteRenderer>();
            var loop = root.AddComponent<EnemySpriteSheetLoop>();
            var texture = new Texture2D(128, 64);

            try
            {
                loop.Configure(texture, 64, 64, 2, 4f);

                Assert.NotNull(renderer.sprite);
                Assert.That(loop.FrameCount, Is.EqualTo(2));
                Assert.That(loop.CurrentFrameIndex, Is.EqualTo(0));
                Assert.That(renderer.sprite.rect.width, Is.EqualTo(64f));
                Assert.That(renderer.sprite.rect.height, Is.EqualTo(64f));
            }
            finally
            {
                Object.DestroyImmediate(texture);
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Advance_LoopsFramesByConfiguredRate()
        {
            var root = new GameObject("SpriteLoop");
            root.AddComponent<SpriteRenderer>();
            var loop = root.AddComponent<EnemySpriteSheetLoop>();
            var texture = new Texture2D(128, 64);

            try
            {
                loop.Configure(texture, 64, 64, 2, 4f);

                loop.Advance(0.25f);
                Assert.That(loop.CurrentFrameIndex, Is.EqualTo(1));

                loop.Advance(0.25f);
                Assert.That(loop.CurrentFrameIndex, Is.EqualTo(0));
            }
            finally
            {
                Object.DestroyImmediate(texture);
                Object.DestroyImmediate(root);
            }
        }
    }
}
