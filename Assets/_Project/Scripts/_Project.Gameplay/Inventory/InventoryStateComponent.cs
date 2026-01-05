using UnityEngine;

namespace Castlebound.Gameplay.Inventory
{
    public class InventoryStateComponent : MonoBehaviour
    {
        private InventoryState state;

        public InventoryState State
        {
            get
            {
                if (state == null)
                {
                    state = new InventoryState();
                }

                return state;
            }
        }
    }
}
