using UnityEngine;

namespace Castlebound.Gameplay.Inventory
{
    public class InventoryStateComponent : MonoBehaviour
    {
        [SerializeField] private int startingGold;
        private InventoryState state;

        public InventoryState State
        {
            get
            {
                if (state == null)
                {
                    state = new InventoryState();
                    if (startingGold > 0)
                    {
                        state.AddGold(startingGold);
                    }
                }

                return state;
            }
        }
    }
}
