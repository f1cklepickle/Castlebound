using Castlebound.Gameplay.Balance;
using Castlebound.Gameplay.Inventory;
using Castlebound.Gameplay.Spawning;
using UnityEngine;

namespace Castlebound.Gameplay.Player
{
    public sealed class PickupMagnetField : MonoBehaviour
    {
        [SerializeField] private GameBalanceStation balanceStation;
        [SerializeField] private EnemySpawnerRunner waveRunner;

        private InventoryStateComponent inventoryComponent;
        private BackpackInventoryStateComponent backpackComponent;
        private bool isSweepActive;

        public GameBalanceStation BalanceStation
        {
            get => balanceStation;
            set => balanceStation = value;
        }

        public EnemySpawnerRunner WaveRunner
        {
            get => waveRunner;
            set => waveRunner = value;
        }

        public bool IsSweepActive => isSweepActive;
        public float ActiveRange => Economy == null
            ? 0f
            : isSweepActive ? Economy.PickupSweepRange : Economy.PickupMagnetRange;
        public float ActiveSpeed => Economy == null
            ? 0f
            : isSweepActive ? Economy.PickupSweepSpeed : Economy.PickupMagnetSpeed;

        private EconomyBalanceTable Economy => balanceStation != null ? balanceStation.Economy : null;

        private void Awake()
        {
            inventoryComponent = GetComponentInChildren<InventoryStateComponent>();
            backpackComponent = GetComponentInChildren<BackpackInventoryStateComponent>();
            EnsureWaveRunner();
        }

        private void OnEnable()
        {
            EnsureWaveRunner();
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void HandleWaveStarted(int waveIndex)
        {
            isSweepActive = false;
        }

        public void HandleWaveEnded()
        {
            isSweepActive = true;
        }

        public bool CanAttract(ItemPickupComponent pickup)
        {
            if (inventoryComponent == null)
            {
                inventoryComponent = GetComponentInChildren<InventoryStateComponent>();
            }

            if (backpackComponent == null)
            {
                backpackComponent = GetComponentInChildren<BackpackInventoryStateComponent>();
            }

            if (pickup == null || pickup.IsConsumed || inventoryComponent == null)
            {
                return false;
            }

            var context = new InventoryPickupContext(
                inventoryComponent.State,
                backpackComponent != null ? backpackComponent.State : null);
            return pickup.CanAutoPickup(context);
        }

        public bool IsWithinRange(Vector3 position)
        {
            float range = ActiveRange;
            return range > 0f && (transform.position - position).sqrMagnitude <= range * range;
        }

        private void EnsureWaveRunner()
        {
            if (waveRunner == null)
            {
                waveRunner = FindObjectOfType<EnemySpawnerRunner>();
            }
        }

        private void Subscribe()
        {
            if (waveRunner == null)
            {
                return;
            }

            waveRunner.OnWaveStarted -= HandleWaveStarted;
            waveRunner.OnWaveEnded -= HandleWaveEnded;
            waveRunner.OnWaveStarted += HandleWaveStarted;
            waveRunner.OnWaveEnded += HandleWaveEnded;
        }

        private void Unsubscribe()
        {
            if (waveRunner == null)
            {
                return;
            }

            waveRunner.OnWaveStarted -= HandleWaveStarted;
            waveRunner.OnWaveEnded -= HandleWaveEnded;
        }
    }
}
