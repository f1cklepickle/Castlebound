using Castlebound.Gameplay.Inventory;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Castlebound.Tests.Inventory
{
    public class CastleInventoryStateComponentTests
    {
        [Test]
        public void State_ReturnsStableVaultState()
        {
            var go = new GameObject("CastleInventory");

            try
            {
                var component = go.AddComponent<CastleInventoryStateComponent>();
                var state = component.State;

                state.AddItem("potion_basic", 2);

                Assert.AreSame(state, component.State);
                Assert.That(component.State.GetCount("potion_basic"), Is.EqualTo(2));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
