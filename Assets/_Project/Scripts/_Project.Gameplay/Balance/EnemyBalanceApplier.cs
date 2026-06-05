using Castlebound.Gameplay.Loot;
using Castlebound.Gameplay.Spawning;
using UnityEngine;

namespace Castlebound.Gameplay.Balance
{
    public class EnemyBalanceApplier : MonoBehaviour
    {
        [SerializeField] private string enemyTypeId = "grunt";
        [SerializeField] private GameBalanceStation balanceStation;
        private bool hasApplied;

        public string EnemyTypeId
        {
            get => enemyTypeId;
            set => enemyTypeId = value;
        }

        public GameBalanceStation BalanceStation
        {
            get => balanceStation;
            set => balanceStation = value;
        }

        public bool Apply(int waveIndex)
        {
            var table = balanceStation != null ? balanceStation.Enemy : null;
            var entry = table != null ? table.Find(enemyTypeId) : null;
            if (entry == null)
            {
                return false;
            }

            ApplyStats(entry);
            ApplyRewards(entry, Mathf.Max(1, waveIndex));
            hasApplied = true;
            return true;
        }

        public void Initialize(GameBalanceStation station, string typeId, int waveIndex)
        {
            balanceStation = station;
            if (!string.IsNullOrWhiteSpace(typeId))
            {
                enemyTypeId = typeId;
            }

            Apply(waveIndex);
        }

        private void Start()
        {
            if (hasApplied)
            {
                return;
            }

            Apply(ResolveWaveIndex());
        }

        private void ApplyStats(EnemyBalanceEntry entry)
        {
            var health = GetComponent<Health>();
            if (health != null)
            {
                health.ConfigureMaxHealth(entry.MaxHealth, true);
            }

            var controller = GetComponent<EnemyController2D>();
            if (controller != null)
            {
                controller.Speed = entry.MoveSpeed;
            }

            var attack = GetComponent<EnemyAttack>();
            if (attack != null)
            {
                attack.Damage = entry.AttackDamage;
                attack.CooldownSeconds = entry.AttackCooldownSeconds;
            }
        }

        private void ApplyRewards(EnemyBalanceEntry entry, int waveIndex)
        {
            var dropper = GetComponent<LootDropper>();
            if (dropper == null)
            {
                return;
            }

            dropper.SetXpAmount(entry.XpReward);
            if (entry.LootProfile != null)
            {
                dropper.ConfigureLootTables(entry.LootProfile.LootTables, entry.LootProfile.GlobalMaxTables);
            }

            if (GameManager.I != null)
            {
                dropper.PreRoll(GameManager.I.LootRng, waveIndex);
            }
        }

        private int ResolveWaveIndex()
        {
            var provider = GetComponent<IWaveIndexProvider>();
            if (provider != null)
            {
                return provider.CurrentWaveIndex;
            }

            var parentProvider = GetComponentInParent<IWaveIndexProvider>();
            return parentProvider != null ? parentProvider.CurrentWaveIndex : 1;
        }
    }
}
