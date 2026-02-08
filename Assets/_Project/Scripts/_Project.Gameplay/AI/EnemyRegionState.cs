using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    [RequireComponent(typeof(EnemyController2D))]
    public class EnemyRegionState : MonoBehaviour
    {
        [SerializeField] private CastleRegionTracker regionOverride;

        public bool EnemyInside => enemyInside;
        public bool PlayerInside => playerInside;

        bool enemyInside;
        bool playerInside;
        EnemyController2D controller;
        CastleRegionTracker region;
        static bool missingRegionWarningLogged;

        bool isBound;

        void Awake()
        {
            controller = GetComponent<EnemyController2D>();
            CastleRegionTracker.InstanceReady += HandleRegionReady;
            BindRegion();
        }

        void OnEnable()
        {
            CastleRegionTracker.InstanceReady += HandleRegionReady;
            BindRegion();
        }

        void OnDisable()
        {
            CastleRegionTracker.InstanceReady -= HandleRegionReady;
            UnbindRegion();
        }

        void OnDestroy()
        {
            CastleRegionTracker.InstanceReady -= HandleRegionReady;
        }

        void BindRegion()
        {
            if (isBound)
            {
                return;
            }

            if (controller == null)
            {
                controller = GetComponent<EnemyController2D>();
            }

            region = regionOverride != null ? regionOverride : CastleRegionTracker.Instance;
            if (region == null)
            {
                if (!missingRegionWarningLogged)
                {
                    Debug.LogWarning("[EnemyRegionState] CastleRegionTracker.Instance is missing; treating enemy/player as outside.", this);
                    missingRegionWarningLogged = true;
                }

                enemyInside = false;
                playerInside = false;
                return;
            }

            region.OnEnemyEntered += HandleEnemyEntered;
            region.OnEnemyExited += HandleEnemyExited;
            region.OnPlayerEntered += HandlePlayerEntered;
            region.OnPlayerExited += HandlePlayerExited;
            isBound = true;

            playerInside = region.PlayerInside;
            enemyInside = controller != null && region.EnemyInside(controller);
        }

        void UnbindRegion()
        {
            if (region == null)
            {
                return;
            }

            region.OnEnemyEntered -= HandleEnemyEntered;
            region.OnEnemyExited -= HandleEnemyExited;
            region.OnPlayerEntered -= HandlePlayerEntered;
            region.OnPlayerExited -= HandlePlayerExited;
            region = null;
            isBound = false;
        }

        void HandleEnemyEntered(EnemyController2D enemy)
        {
            if (enemy == controller)
            {
                enemyInside = true;
            }
        }

        void HandleEnemyExited(EnemyController2D enemy)
        {
            if (enemy == controller)
            {
                enemyInside = false;
            }
        }

        void HandlePlayerEntered()
        {
            playerInside = true;
        }

        void HandlePlayerExited()
        {
            playerInside = false;
        }

        void HandleRegionReady()
        {
            if (this == null)
            {
                return;
            }

            BindRegion();
        }

#if UNITY_EDITOR
        public static void Debug_ResetMissingRegionWarning()
        {
            missingRegionWarningLogged = false;
        }

        public void Debug_EnsureBound()
        {
            BindRegion();
        }
#endif
    }
}
