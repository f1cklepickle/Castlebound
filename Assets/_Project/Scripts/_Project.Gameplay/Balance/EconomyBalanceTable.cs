using UnityEngine;

namespace Castlebound.Gameplay.Balance
{
    [CreateAssetMenu(menuName = "Castlebound/Balance/Economy Balance Table")]
    public class EconomyBalanceTable : ScriptableObject
    {
        [SerializeField] private int startingGold = 0;
        [SerializeField] private int startingXp = 0;
        [SerializeField] private float pickupMagnetRange = 8f;
        [SerializeField] private float pickupMagnetSpeed = 10.4f;
        [SerializeField] private float pickupSweepRange = 30f;
        [SerializeField] private float pickupSweepSpeed = 26f;

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

        public float PickupMagnetRange
        {
            get => pickupMagnetRange;
            set => pickupMagnetRange = Mathf.Max(0f, value);
        }

        public float PickupMagnetSpeed
        {
            get => pickupMagnetSpeed;
            set => pickupMagnetSpeed = Mathf.Max(0f, value);
        }

        public float PickupSweepRange
        {
            get => pickupSweepRange;
            set => pickupSweepRange = Mathf.Max(0f, value);
        }

        public float PickupSweepSpeed
        {
            get => pickupSweepSpeed;
            set => pickupSweepSpeed = Mathf.Max(0f, value);
        }

        private void OnValidate()
        {
            StartingGold = startingGold;
            StartingXp = startingXp;
            PickupMagnetRange = pickupMagnetRange;
            PickupMagnetSpeed = pickupMagnetSpeed;
            PickupSweepRange = pickupSweepRange;
            PickupSweepSpeed = pickupSweepSpeed;
        }
    }
}
