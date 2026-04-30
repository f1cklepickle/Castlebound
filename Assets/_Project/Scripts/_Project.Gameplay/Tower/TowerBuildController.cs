using Castlebound.Gameplay.Castle;
using Castlebound.Gameplay.Inventory;
using Castlebound.Gameplay.Spawning;
using UnityEngine;

namespace Castlebound.Gameplay.Tower
{
    public class TowerBuildController : MonoBehaviour
    {
        [SerializeField] private TowerBuildConfig config;
        [SerializeField] private InventoryStateComponent inventorySource;
        [SerializeField] private EnemySpawnerRunner waveRunner;
        [SerializeField] private FeedbackEventChannel feedbackChannel;
        [SerializeField] private Transform towerParent;

        private InventoryState inventory;
        private WavePhaseTracker phaseTracker;

        public TowerBuildConfig Config
        {
            get => config;
            set => config = value;
        }

        public Transform TowerParent
        {
            get => towerParent;
            set => towerParent = value;
        }

        public TowerBuildResult TryBuild(TowerPlot plot)
        {
            var result = ValidateBuild(plot, out var source, out var anchor);
            if (result != TowerBuildResult.Success)
            {
                RaiseFeedback(result, plot, anchor);
                return result;
            }

            int cost = config.BuildCost;
            if (cost > 0 && !source.TrySpendGold(cost))
            {
                result = TowerBuildResult.InsufficientGold;
                RaiseFeedback(result, plot, anchor);
                return result;
            }

            var instance = SpawnTower(anchor);
            if (!plot.TryAssignOccupant(instance))
            {
                DestroySpawnedTower(instance);
                if (cost > 0)
                {
                    source.AddGold(cost);
                }
                result = TowerBuildResult.Occupied;
                RaiseFeedback(result, plot, anchor);
                return result;
            }

            RaiseFeedback(TowerBuildResult.Success, plot, anchor);
            return TowerBuildResult.Success;
        }

        public void SetInventory(InventoryState state)
        {
            inventory = state;
        }

        public void SetPhaseTracker(WavePhaseTracker tracker)
        {
            phaseTracker = tracker;
        }

        public void SetFeedbackChannel(FeedbackEventChannel channel)
        {
            feedbackChannel = channel;
        }

        private TowerBuildResult ValidateBuild(TowerPlot plot, out InventoryState source, out Transform anchor)
        {
            source = null;
            anchor = plot != null ? plot.Anchor : null;

            if (plot == null)
            {
                return TowerBuildResult.InvalidPlot;
            }

            if (plot.IsOccupied)
            {
                return TowerBuildResult.Occupied;
            }

            if (!IsInPreWave())
            {
                return TowerBuildResult.NotPreWave;
            }

            if (config == null)
            {
                return TowerBuildResult.MissingConfig;
            }

            if (config.TowerPrefab == null)
            {
                return TowerBuildResult.MissingPrefab;
            }

            source = GetInventory();
            if (source == null)
            {
                return TowerBuildResult.MissingInventory;
            }

            return TowerBuildResult.Success;
        }

        private GameObject SpawnTower(Transform anchor)
        {
            var parent = towerParent != null ? towerParent : null;
            return parent != null
                ? Instantiate(config.TowerPrefab, anchor.position, anchor.rotation, parent)
                : Instantiate(config.TowerPrefab, anchor.position, anchor.rotation);
        }

        private static void DestroySpawnedTower(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(instance);
            }
            else
            {
                DestroyImmediate(instance);
            }
        }

        private bool IsInPreWave()
        {
            if (phaseTracker != null)
            {
                return phaseTracker.CurrentPhase == WavePhase.PreWave;
            }

            if (waveRunner == null)
            {
                waveRunner = FindObjectOfType<EnemySpawnerRunner>();
            }

            return waveRunner != null
                && waveRunner.PhaseTracker != null
                && waveRunner.PhaseTracker.CurrentPhase == WavePhase.PreWave;
        }

        private InventoryState GetInventory()
        {
            if (inventory != null)
            {
                return inventory;
            }

            if (inventorySource == null)
            {
                inventorySource = GetComponent<InventoryStateComponent>();
            }

            inventory = inventorySource != null ? inventorySource.State : null;
            return inventory;
        }

        private void RaiseFeedback(TowerBuildResult result, TowerPlot plot, Transform anchor)
        {
            if (feedbackChannel == null)
            {
                return;
            }

            var type = result == TowerBuildResult.Success
                ? FeedbackCueType.UpgradeSuccess
                : FeedbackCueType.UpgradeDenied;
            var position = anchor != null ? anchor.position : Vector3.zero;
            var targetId = plot != null ? plot.GetInstanceID() : 0;
            feedbackChannel.Raise(new FeedbackCue(type, position, targetId));
        }
    }
}
