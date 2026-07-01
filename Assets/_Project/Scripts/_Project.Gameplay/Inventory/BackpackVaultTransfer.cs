using Castlebound.Gameplay.Spawning;
using UnityEngine;

namespace Castlebound.Gameplay.Inventory
{
    public class BackpackVaultTransfer : MonoBehaviour
    {
        [SerializeField] private BackpackInventoryStateComponent backpackSource;
        [SerializeField] private CastleInventoryStateComponent castleInventorySource;
        [SerializeField] private EnemySpawnerRunner waveRunner;

        private EnemySpawnerRunner hookedWaveRunner;

        public BackpackInventoryStateComponent BackpackSource => backpackSource;
        public CastleInventoryStateComponent CastleInventorySource => castleInventorySource;

        private void Reset()
        {
            EnsureReferences();
        }

        private void Awake()
        {
            EnsureReferences();
        }

        private void OnEnable()
        {
            EnsureReferences();
            HookWaveRunner();
        }

        private void OnDisable()
        {
            UnhookWaveRunner();
        }

        public void Configure(
            BackpackInventoryStateComponent backpack,
            CastleInventoryStateComponent castleInventory,
            EnemySpawnerRunner runner = null)
        {
            UnhookWaveRunner();
            backpackSource = backpack;
            castleInventorySource = castleInventory;
            waveRunner = runner;

            if (isActiveAndEnabled)
            {
                HookWaveRunner();
            }
        }

        public void HandleWaveEnded()
        {
            TransferBackpackToVault();
        }

        public bool TransferBackpackToVault()
        {
            EnsureReferences();

            if (backpackSource == null || castleInventorySource == null)
            {
                return false;
            }

            var backpack = backpackSource.State;
            if (backpack.ItemCount == 0)
            {
                return false;
            }

            var entries = backpack.Entries;
            var transferred = false;
            for (var i = 0; i < entries.Count; i++)
            {
                transferred |= castleInventorySource.State.AddItem(entries[i].ItemId, entries[i].Count);
            }

            if (transferred)
            {
                backpack.Clear();
            }

            return transferred;
        }

        private void EnsureReferences()
        {
            if (backpackSource == null)
            {
                backpackSource = GetComponent<BackpackInventoryStateComponent>();
            }

            if (castleInventorySource == null)
            {
                castleInventorySource = GetComponent<CastleInventoryStateComponent>();
            }

            if (waveRunner == null)
            {
                waveRunner = FindObjectOfType<EnemySpawnerRunner>();
            }
        }

        private void HookWaveRunner()
        {
            if (waveRunner == null || hookedWaveRunner == waveRunner)
            {
                return;
            }

            hookedWaveRunner = waveRunner;
            hookedWaveRunner.OnWaveEnded += HandleWaveEnded;
        }

        private void UnhookWaveRunner()
        {
            if (hookedWaveRunner == null)
            {
                return;
            }

            hookedWaveRunner.OnWaveEnded -= HandleWaveEnded;
            hookedWaveRunner = null;
        }
    }
}
