using UnityEngine;

namespace Castlebound.Gameplay.Balance
{
    [CreateAssetMenu(menuName = "Castlebound/Balance/Economy Balance Table")]
    public class EconomyBalanceTable : ScriptableObject
    {
        [SerializeField] private int startingGold = 0;
        [SerializeField] private int startingXp = 0;

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

        private void OnValidate()
        {
            StartingGold = startingGold;
            StartingXp = startingXp;
        }
    }
}
