using Castlebound.Gameplay.Balance;
using UnityEngine;

namespace Castlebound.Gameplay.Inventory
{
    public class InventoryStateComponent : MonoBehaviour
    {
        [SerializeField] private GameBalanceStation balanceStation;
        [SerializeField] private int startingGold;
        [SerializeField] private int startingXp;
        private InventoryState state;

        public GameBalanceStation BalanceStation
        {
            get => balanceStation;
            set => balanceStation = value;
        }

        public int StartingGold
        {
            get => startingGold;
            set => startingGold = Mathf.Max(0, value);
        }

        public int StartingXp
        {
            get => startingXp;
            set => startingXp = Mathf.Max(0, value);
        }

        public InventoryState State
        {
            get
            {
                if (state == null)
                {
                    int initialGold = ActiveEconomyTable != null ? ActiveEconomyTable.StartingGold : startingGold;
                    int initialXp = ActiveEconomyTable != null ? ActiveEconomyTable.StartingXp : startingXp;
                    state = new InventoryState(initialGold, initialXp);
                }

                return state;
            }
        }

        private EconomyBalanceTable ActiveEconomyTable => balanceStation != null ? balanceStation.Economy : null;

        private void OnValidate()
        {
            StartingGold = startingGold;
            StartingXp = startingXp;
        }
    }
}
