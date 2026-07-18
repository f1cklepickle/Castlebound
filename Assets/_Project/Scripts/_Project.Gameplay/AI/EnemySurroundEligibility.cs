using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    public class EnemySurroundEligibility : MonoBehaviour
    {
        private EnemyController2D controller;
        private EnemyRootReceiver rootReceiver;
        private Health health;

        private void Awake()
        {
            CacheComponents();
        }

        public bool IsEligibleFor(Transform player)
        {
            CacheComponents();

            return Evaluate(
                isActiveAndEnabled,
                controller != null && controller.enabled,
                health != null && health.Current > 0,
                rootReceiver != null && rootReceiver.IsRooted,
                controller != null ? controller.CurrentTargetType : EnemyTargetType.None,
                controller != null && controller.Target == player);
        }

        public static bool Evaluate(
            bool isActiveAndEnabled,
            bool isControllerEnabled,
            bool isAlive,
            bool isRooted,
            EnemyTargetType targetType,
            bool targetsPlayer)
        {
            return isActiveAndEnabled &&
                   isControllerEnabled &&
                   isAlive &&
                   !isRooted &&
                   targetType == EnemyTargetType.Player &&
                   targetsPlayer;
        }

        private void CacheComponents()
        {
            if (controller == null)
                controller = GetComponent<EnemyController2D>();
            if (rootReceiver == null)
                rootReceiver = GetComponent<EnemyRootReceiver>();
            if (health == null)
                health = GetComponent<Health>();
        }
    }
}
