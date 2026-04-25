using UnityEngine;

namespace Castlebound.Gameplay.Tower
{
    public class TowerRuntime : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 10;
        [SerializeField] private int currentHealth = 10;
        [SerializeField] private Transform aimPivot;
        [SerializeField] private Transform towerVisual;
        [SerializeField] private Transform platformVisual;

        public int MaxHealth => maxHealth;
        public int CurrentHealth => currentHealth;
        public Transform AimPivot
        {
            get
            {
                EnsureReferences();
                return aimPivot;
            }
        }

        public Transform TowerVisual
        {
            get
            {
                EnsureReferences();
                return towerVisual;
            }
        }

        public Transform PlatformVisual
        {
            get
            {
                EnsureReferences();
                return platformVisual;
            }
        }

        private void Reset()
        {
            maxHealth = Mathf.Max(1, maxHealth);
            currentHealth = maxHealth;
            EnsureReferences();
        }

        private void Awake()
        {
            NormalizeState();
            EnsureReferences();
        }

        private void OnValidate()
        {
            NormalizeState();
            EnsureReferences();
        }

        private void NormalizeState()
        {
            maxHealth = Mathf.Max(1, maxHealth);
            currentHealth = Mathf.Clamp(currentHealth, 1, maxHealth);
        }

        private void EnsureReferences()
        {
            if (aimPivot == null)
            {
                aimPivot = transform.Find("AimPivot");
            }

            if (towerVisual == null && aimPivot != null)
            {
                towerVisual = aimPivot.Find("TowerVisual");
            }

            if (platformVisual == null)
            {
                platformVisual = transform.Find("PlatformVisual");
            }
        }
    }
}
