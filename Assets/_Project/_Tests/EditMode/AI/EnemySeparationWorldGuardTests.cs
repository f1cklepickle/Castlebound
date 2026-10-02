using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.AI
{
    public class EnemySeparationWorldGuardTests
    {
        [TestCase(0f)] [TestCase(2200f)]
        public void InwardRecovery_PreservesSignedWallGap_AndAllowsEscape(float coordinate)
        {
            var actor = new GameObject("GuardBody");
            var wall = new GameObject("GuardWall");
            try
            {
                actor.layer = LayerMask.NameToLayer("Enemies");
                wall.layer = LayerMask.NameToLayer("Walls");
                actor.transform.position = new Vector2(coordinate, coordinate);
                wall.transform.position = actor.transform.position + Vector3.left * 1.09f;
                var body = actor.AddComponent<Rigidbody2D>(); body.gravityScale = 0f;
                var circle = actor.AddComponent<CircleCollider2D>(); circle.radius = 0.87684506f;
                var box = wall.AddComponent<BoxCollider2D>(); box.size = new Vector2(0.4f, 4f);
                Physics2D.SyncTransforms();
                float gap = circle.Distance(box).distance;
                Assert.That(gap, Is.GreaterThanOrEqualTo(-0.002f));
                var guard = new EnemySeparationWorldGuard(circle);
                Vector2 inward = guard.Constrain(Vector2.left * 0.06f);
                Assert.That(inward.magnitude, Is.LessThanOrEqualTo(Mathf.Max(0f, gap - Physics2D.defaultContactOffset) + 0.00001f));
                Assert.That(guard.Constrain(Vector2.right * 0.06f).x, Is.EqualTo(0.06f).Within(0.00001f));
            }
            finally { Object.DestroyImmediate(actor); Object.DestroyImmediate(wall); Physics2D.SyncTransforms(); }
        }
    }
}
