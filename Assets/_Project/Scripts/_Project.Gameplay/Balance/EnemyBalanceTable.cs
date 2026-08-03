using UnityEngine;
using Castlebound.Gameplay.Spawning;

namespace Castlebound.Gameplay.Balance
{
    [CreateAssetMenu(menuName = "Castlebound/Balance/Enemy Balance Table")]
    public class EnemyBalanceTable : ScriptableObject
    {
        [SerializeField] private EnemyBalanceEntry[] enemies =
        {
            new EnemyBalanceEntry(),
            new EnemyBalanceEntry
            {
                EnemyTypeId = EnemyArchetypeIds.GoblinRanged,
                MaxHealth = 10,
                MoveSpeed = 8f,
                AttackDamage = 1,
                AttackCooldownSeconds = 0.8f,
                XpReward = 5
            },
            new EnemyBalanceEntry
            {
                EnemyTypeId = EnemyArchetypeIds.Lurker,
                MaxHealth = 35,
                MoveSpeed = 3f,
                AttackDamage = 1,
                AttackCooldownSeconds = 0.8f,
                XpReward = 5
            }
        };

        public EnemyBalanceEntry[] Enemies
        {
            get => enemies;
            set => enemies = value;
        }

        public EnemyBalanceEntry Find(string enemyTypeId)
        {
            if (string.IsNullOrWhiteSpace(enemyTypeId) || enemies == null)
            {
                return null;
            }

            string canonicalId = EnemyArchetypeIds.Canonicalize(enemyTypeId);
            for (int i = 0; i < enemies.Length; i++)
            {
                var entry = enemies[i];
                if (entry != null && entry.EnemyTypeId == canonicalId)
                {
                    return entry;
                }
            }

            return null;
        }

        private void OnValidate()
        {
            if (enemies == null)
            {
                return;
            }

            for (int i = 0; i < enemies.Length; i++)
            {
                enemies[i]?.Normalize();
            }
        }
    }
}
