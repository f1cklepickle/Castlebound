using UnityEngine;

namespace Castlebound.Gameplay.Inventory
{
    public class BackpackInventoryStateComponent : MonoBehaviour
    {
        [SerializeField] private int maxItemCount = 8;

        private BackpackInventoryState state;

        public int MaxItemCount
        {
            get => maxItemCount;
            set
            {
                maxItemCount = Mathf.Max(0, value);
                if (state != null)
                {
                    state.MaxItemCount = maxItemCount;
                }
            }
        }

        public BackpackInventoryState State
        {
            get
            {
                if (state == null)
                {
                    state = new BackpackInventoryState(maxItemCount);
                }

                return state;
            }
        }

        private void OnValidate()
        {
            MaxItemCount = maxItemCount;
        }
    }
}
