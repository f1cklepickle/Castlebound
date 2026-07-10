using Castlebound.Gameplay.Inventory;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Inventory
{
    public class BackpackDropDirectionResolverTests
    {
        [Test]
        public void ResolveVisualForward_UsesNegativeTransformUp()
        {
            var origin = new GameObject("DropOrigin").transform;

            try
            {
                origin.up = Vector3.right;

                var actual = BackpackDropDirectionResolver.ResolveVisualForward(origin);

                Assert.That(Vector3.Distance(actual, Vector3.left), Is.LessThan(0.0001f));
            }
            finally
            {
                Object.DestroyImmediate(origin.gameObject);
            }
        }
    }
}
