using Castlebound.Gameplay.Combat;
using Castlebound.Gameplay.Projectile;
using UnityEngine;

namespace Castlebound.Gameplay.Tower
{
    public class TowerWeaponFireAnimationBinder : MonoBehaviour
    {
        [SerializeField] private TowerAttackController attackController;
        [SerializeField] private WeaponFireAnimationPlayer animationPlayer;

        private bool isHooked;

        public TowerAttackController AttackController
        {
            get => attackController;
            set
            {
                if (attackController == value)
                {
                    return;
                }

                Unhook();
                attackController = value;
                Hook();
            }
        }

        public WeaponFireAnimationPlayer AnimationPlayer
        {
            get => animationPlayer;
            set => animationPlayer = value;
        }

        private void Reset()
        {
            EnsureReferences();
        }

        private void OnEnable()
        {
            EnsureReferences();
            Hook();
        }

        private void OnDisable()
        {
            Unhook();
        }

        private void HandleFired(ProjectileRuntime projectile)
        {
            if (attackController == null || animationPlayer == null)
            {
                return;
            }

            animationPlayer.Play(attackController.CooldownSeconds);
        }

        private void Hook()
        {
            if (!isHooked && attackController != null)
            {
                attackController.Fired += HandleFired;
                isHooked = true;
            }
        }

        private void Unhook()
        {
            if (isHooked && attackController != null)
            {
                attackController.Fired -= HandleFired;
            }

            isHooked = false;
        }

        private void EnsureReferences()
        {
            if (attackController == null)
            {
                attackController = GetComponent<TowerAttackController>();
            }

            if (animationPlayer == null)
            {
                animationPlayer = GetComponentInChildren<WeaponFireAnimationPlayer>();
            }
        }
    }
}
