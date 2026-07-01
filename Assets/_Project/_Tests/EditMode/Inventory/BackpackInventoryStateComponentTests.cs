using Castlebound.Gameplay.Inventory;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Castlebound.Tests.Inventory
{
    public class BackpackInventoryStateComponentTests
    {
        [Test]
        public void State_ReturnsStableBackpackStateWithAuthoredCapacity()
        {
            var go = new GameObject("Backpack");

            try
            {
                var component = go.AddComponent<BackpackInventoryStateComponent>();
                component.MaxItemCount = 1;

                var state = component.State;

                Assert.AreSame(state, component.State);
                Assert.That(state.MaxItemCount, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
