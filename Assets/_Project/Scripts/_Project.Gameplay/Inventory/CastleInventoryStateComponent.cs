using UnityEngine;

namespace Castlebound.Gameplay.Inventory
{
    public class CastleInventoryStateComponent : MonoBehaviour
    {
        private CastleInventoryState state;

        public CastleInventoryState State
        {
            get
            {
                if (state == null)
                {
                    state = new CastleInventoryState();
                }

                return state;
            }
        }
    }
}
