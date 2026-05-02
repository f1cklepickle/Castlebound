using UnityEngine;

namespace Castlebound.Gameplay.Tower
{
    [DefaultExecutionOrder(100)]
    public class TowerAimController : MonoBehaviour
    {
        [SerializeField] private bool aimEnabled = true;
        [SerializeField] private TowerTargetingController targetingController;
        [SerializeField] private Transform aimPivot;
        [SerializeField] private TowerAimMode aimMode;
        [SerializeField] private float rotationSpeedDegrees = 720f;
        [SerializeField] private float aimOffsetDegrees;

        public bool AimEnabled
        {
            get => aimEnabled;
            set => aimEnabled = value;
        }

        public TowerTargetingController TargetingController
        {
            get
            {
                EnsureReferences();
                return targetingController;
            }
            set => targetingController = value;
        }

        public Transform AimPivot
        {
            get
            {
                EnsureReferences();
                return aimPivot;
            }
            set => aimPivot = value;
        }

        public TowerAimMode AimMode
        {
            get => aimMode;
            set => aimMode = value;
        }

        public float RotationSpeedDegrees
        {
            get => rotationSpeedDegrees;
            set => rotationSpeedDegrees = Mathf.Max(0f, value);
        }

        public float AimOffsetDegrees
        {
            get => aimOffsetDegrees;
            set => aimOffsetDegrees = value;
        }

        private void Reset()
        {
            EnsureReferences();
        }

        private void Awake()
        {
            EnsureReferences();
            rotationSpeedDegrees = Mathf.Max(0f, rotationSpeedDegrees);
        }

        private void OnValidate()
        {
            rotationSpeedDegrees = Mathf.Max(0f, rotationSpeedDegrees);
            EnsureReferences();
        }

        private void LateUpdate()
        {
            ApplyAimNow(Time.deltaTime);
        }

        public void ApplyAimNow(float deltaTime)
        {
            if (!aimEnabled)
            {
                return;
            }

            EnsureReferences();

            if (targetingController == null || aimPivot == null || targetingController.CurrentTarget == null)
            {
                return;
            }

            var direction = targetingController.CurrentTarget.position - aimPivot.position;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            var targetRotation = Quaternion.Euler(0f, 0f, GetTargetAngle(direction));
            if (aimMode == TowerAimMode.RotateToward)
            {
                aimPivot.rotation = Quaternion.RotateTowards(
                    aimPivot.rotation,
                    targetRotation,
                    rotationSpeedDegrees * Mathf.Max(0f, deltaTime));
                return;
            }

            aimPivot.rotation = targetRotation;
        }

        private float GetTargetAngle(Vector3 direction)
        {
            return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f + aimOffsetDegrees;
        }

        private void EnsureReferences()
        {
            if (targetingController == null)
            {
                targetingController = GetComponent<TowerTargetingController>();
            }

            if (aimPivot == null)
            {
                var runtime = GetComponent<TowerRuntime>();
                if (runtime != null)
                {
                    aimPivot = runtime.AimPivot;
                }
            }
        }
    }
}
