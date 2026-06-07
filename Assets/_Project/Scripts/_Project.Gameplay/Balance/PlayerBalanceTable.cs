using UnityEngine;

namespace Castlebound.Gameplay.Balance
{
    [CreateAssetMenu(menuName = "Castlebound/Balance/Player Balance Table")]
    public class PlayerBalanceTable : ScriptableObject
    {
        [SerializeField] private int baseMaxHealth = 200;
        [SerializeField] private float baseMoveSpeed = 12f;
        [SerializeField] private float baseRepairRange = 2f;

        public int BaseMaxHealth
        {
            get => baseMaxHealth;
            set => baseMaxHealth = Mathf.Max(0, value);
        }

        public float BaseMoveSpeed
        {
            get => baseMoveSpeed;
            set => baseMoveSpeed = Mathf.Max(0f, value);
        }

        public float BaseRepairRange
        {
            get => baseRepairRange;
            set => baseRepairRange = Mathf.Max(0f, value);
        }

        private void OnValidate()
        {
            BaseMaxHealth = baseMaxHealth;
            BaseMoveSpeed = baseMoveSpeed;
            BaseRepairRange = baseRepairRange;
        }
    }
}
