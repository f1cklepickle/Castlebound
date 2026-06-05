using Castlebound.Gameplay.Loot;
using UnityEngine;

namespace Castlebound.Gameplay.Balance
{
    [System.Serializable]
    public class EnemyBalanceEntry
    {
        [SerializeField] private string enemyTypeId = "grunt";
        [SerializeField] private int maxHealth = 10;
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private int attackDamage = 1;
        [SerializeField] private float attackCooldownSeconds = 0.8f;
        [SerializeField] private int xpReward = 5;
        [SerializeField] private EnemyLootProfile lootProfile;

        public string EnemyTypeId
        {
            get => enemyTypeId;
            set => enemyTypeId = value;
        }

        public int MaxHealth
        {
            get => maxHealth;
            set => maxHealth = Mathf.Max(0, value);
        }

        public float MoveSpeed
        {
            get => moveSpeed;
            set => moveSpeed = Mathf.Max(0f, value);
        }

        public int AttackDamage
        {
            get => attackDamage;
            set => attackDamage = Mathf.Max(0, value);
        }

        public float AttackCooldownSeconds
        {
            get => attackCooldownSeconds;
            set => attackCooldownSeconds = Mathf.Max(0f, value);
        }

        public int XpReward
        {
            get => xpReward;
            set => xpReward = Mathf.Max(0, value);
        }

        public EnemyLootProfile LootProfile
        {
            get => lootProfile;
            set => lootProfile = value;
        }

        public void Normalize()
        {
            MaxHealth = maxHealth;
            MoveSpeed = moveSpeed;
            AttackDamage = attackDamage;
            AttackCooldownSeconds = attackCooldownSeconds;
            XpReward = xpReward;
        }
    }
}
