using UnityEngine;

namespace Castlebound.Gameplay.Balance
{
    [CreateAssetMenu(menuName = "Castlebound/Balance/Game Balance Station")]
    public class GameBalanceStation : ScriptableObject
    {
        [SerializeField] private TowerBalanceTable tower;
        [SerializeField] private BarrierBalanceTable barrier;
        [SerializeField] private WaveBalanceTable wave;
        [SerializeField] private EnemyBalanceTable enemy;
        [SerializeField] private PlayerBalanceTable player;
        [SerializeField] private EconomyBalanceTable economy;

        public TowerBalanceTable Tower
        {
            get => tower;
            set => tower = value;
        }

        public BarrierBalanceTable Barrier
        {
            get => barrier;
            set => barrier = value;
        }

        public WaveBalanceTable Wave
        {
            get => wave;
            set => wave = value;
        }

        public EnemyBalanceTable Enemy
        {
            get => enemy;
            set => enemy = value;
        }

        public PlayerBalanceTable Player
        {
            get => player;
            set => player = value;
        }

        public EconomyBalanceTable Economy
        {
            get => economy;
            set => economy = value;
        }
    }
}
