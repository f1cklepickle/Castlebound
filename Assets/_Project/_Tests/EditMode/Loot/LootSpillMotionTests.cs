using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.Loot;

namespace Castlebound.Tests.Loot
{
    public class LootSpillMotionTests
    {
        [Test]
        public void Step_MovesTowardsTarget()
        {
            var go = new GameObject("Spill");
            var motion = go.AddComponent<LootSpillMotion>();
            go.transform.position = Vector3.zero;

            motion.Initialize(new Vector3(1f, 0f, 0f), 1f);
            motion.Step(0.5f);

            Assert.That(go.transform.position.x, Is.EqualTo(0.5f).Within(0.01f));

            motion.Step(0.5f);
            Assert.That(go.transform.position.x, Is.EqualTo(1f).Within(0.01f));

            Object.DestroyImmediate(go);
        }
    }
}
