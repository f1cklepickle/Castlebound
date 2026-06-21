using UnityEngine;

namespace Castlebound.Gameplay.Projectile
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class ProjectileLaunchSorting : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private int flightSortingOrder = 4;
        [SerializeField] private int towerLaunchSortingOrder = 15;
        [SerializeField] private float towerClearDistance = 0.75f;

        private Vector3 launchPosition;
        private float clearDistanceSquared;
        private bool waitingForTowerClear;

        private void Awake()
        {
            ResolveRenderer();
            NormalizeClearDistance();
            ApplySortingOrder(flightSortingOrder);
            enabled = false;
        }

        private void OnValidate()
        {
            NormalizeClearDistance();
        }

        private void Update()
        {
            RefreshSorting(transform.position);
        }

        public void BeginTowerLaunch(Vector3 origin)
        {
            ResolveRenderer();
            NormalizeClearDistance();
            launchPosition = origin;
            waitingForTowerClear = true;
            enabled = true;
            ApplySortingOrder(towerLaunchSortingOrder);
        }

        public void RefreshSorting(Vector3 currentPosition)
        {
            if (!waitingForTowerClear)
            {
                return;
            }

            if ((currentPosition - launchPosition).sqrMagnitude < clearDistanceSquared)
            {
                return;
            }

            waitingForTowerClear = false;
            ApplySortingOrder(flightSortingOrder);
            enabled = false;
        }

        private void ResolveRenderer()
        {
            if (targetRenderer == null)
            {
                targetRenderer = GetComponent<SpriteRenderer>();
            }
        }

        private void NormalizeClearDistance()
        {
            towerClearDistance = Mathf.Max(0f, towerClearDistance);
            clearDistanceSquared = towerClearDistance * towerClearDistance;
        }

        private void ApplySortingOrder(int sortingOrder)
        {
            if (targetRenderer != null)
            {
                targetRenderer.sortingOrder = sortingOrder;
            }
        }
    }
}
