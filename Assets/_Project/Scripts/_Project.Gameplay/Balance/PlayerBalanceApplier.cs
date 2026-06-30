using UnityEngine;

namespace Castlebound.Gameplay.Balance
{
    public class PlayerBalanceApplier : MonoBehaviour
    {
        [SerializeField] private GameBalanceStation balanceStation;
        [SerializeField] private PlayerController playerController;

        private bool hasApplied;

        public GameBalanceStation BalanceStation
        {
            get => balanceStation;
            set => balanceStation = value;
        }

        public PlayerController PlayerController
        {
            get => playerController;
            set => playerController = value;
        }

        public bool Apply()
        {
            var table = balanceStation != null ? balanceStation.Player : null;
            if (table == null)
            {
                return false;
            }

            ApplyHealth(table);
            ApplyMovement(table);
            ApplyRepair(table);
            hasApplied = true;
            return true;
        }

        private void Start()
        {
            if (hasApplied)
            {
                return;
            }

            Apply();
        }

        private void ApplyHealth(PlayerBalanceTable table)
        {
            var health = GetComponent<Health>();
            if (health != null)
            {
                health.ConfigureMaxHealth(table.BaseMaxHealth, true);
            }
        }

        private void ApplyMovement(PlayerBalanceTable table)
        {
            var mover = GetComponent<PlayerCollisionMove2D>();
            if (mover != null)
            {
                mover.MoveSpeed = table.BaseMoveSpeed;
            }
        }

        private void ApplyRepair(PlayerBalanceTable table)
        {
            var controller = playerController != null ? playerController : GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.RepairRange = table.BaseRepairRange;
                controller.RepairCooldownSeconds = table.BaseRepairCooldownSeconds;
            }
        }
    }
}
