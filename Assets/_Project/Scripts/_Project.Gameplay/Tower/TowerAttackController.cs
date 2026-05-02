using System;
using Castlebound.Gameplay.Projectile;
using UnityEngine;

namespace Castlebound.Gameplay.Tower
{
    public class TowerAttackController : MonoBehaviour
    {
        [SerializeField] private TowerTargetingController targetingController;
        [SerializeField] private ProjectileRuntime projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private int damage = 1;
        [SerializeField] private float cooldownSeconds = 1f;
        [SerializeField] private float projectileSpeed = 8f;
        [SerializeField] private float projectileLifetime = 3f;
        [SerializeField] private LayerMask targetLayerMask;

        private float nextFireTime;

        public event Action<ProjectileRuntime> Fired;

        public TowerTargetingController TargetingController
        {
            get => targetingController;
            set => targetingController = value;
        }

        public ProjectileRuntime ProjectilePrefab
        {
            get => projectilePrefab;
            set => projectilePrefab = value;
        }

        public Transform FirePoint
        {
            get => firePoint;
            set => firePoint = value;
        }

        public int Damage
        {
            get => damage;
            set => damage = Mathf.Max(0, value);
        }

        public float CooldownSeconds
        {
            get => cooldownSeconds;
            set => cooldownSeconds = Mathf.Max(0f, value);
        }

        public float ProjectileSpeed
        {
            get => projectileSpeed;
            set => projectileSpeed = Mathf.Max(0f, value);
        }

        public float ProjectileLifetime
        {
            get => projectileLifetime;
            set => projectileLifetime = Mathf.Max(0f, value);
        }

        public LayerMask TargetLayerMask
        {
            get => targetLayerMask;
            set => targetLayerMask = value;
        }

        private void Reset()
        {
            EnsureReferences();
            NormalizeStats();
        }

        private void Awake()
        {
            EnsureReferences();
        }

        private void OnValidate()
        {
            NormalizeStats();
        }

        private void Update()
        {
            TryFire(Time.time);
        }

        public ProjectileRuntime TryFire(float currentTime)
        {
            EnsureReferences();

            if (projectilePrefab == null || targetingController == null || currentTime < nextFireTime)
            {
                return null;
            }

            var target = targetingController.CurrentTarget;
            if (target == null)
            {
                return null;
            }

            var origin = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
            var projectile = Instantiate(projectilePrefab, origin, Quaternion.identity);
            var context = ProjectileLaunchContext.TowardPoint(
                origin,
                target.position,
                transform,
                projectileSpeed,
                damage,
                projectileLifetime,
                targetLayerMask);

            projectile.Launch(context);
            nextFireTime = currentTime + cooldownSeconds;
            Fired?.Invoke(projectile);
            return projectile;
        }

        private void EnsureReferences()
        {
            if (targetingController == null)
            {
                targetingController = GetComponent<TowerTargetingController>();
            }
        }

        private void NormalizeStats()
        {
            damage = Mathf.Max(0, damage);
            cooldownSeconds = Mathf.Max(0f, cooldownSeconds);
            projectileSpeed = Mathf.Max(0f, projectileSpeed);
            projectileLifetime = Mathf.Max(0f, projectileLifetime);
        }
    }
}
