using UnityEngine;

namespace Castlebound.Gameplay.Projectile
{
    [RequireComponent(typeof(Collider2D))]
    public class ProjectileRuntime : MonoBehaviour
    {
        [SerializeField] private float defaultSpeed = 8f;
        [SerializeField] private int defaultDamage = 1;
        [SerializeField] private float defaultLifetime = 3f;
        [SerializeField] private LayerMask defaultTargetLayerMask;

        private Collider2D projectileCollider;
        private Transform owner;
        private Vector2 direction = Vector2.right;
        private float speed;
        private int damage;
        private float remainingLifetime;
        private LayerMask targetLayerMask;
        private bool launched;

        private void Reset()
        {
            NormalizeDefaults();
            EnsureCollider();
        }

        private void Awake()
        {
            EnsureCollider();
        }

        private void OnValidate()
        {
            NormalizeDefaults();
        }

        private void Update()
        {
            if (!launched)
            {
                return;
            }

            transform.position += (Vector3)(direction * speed * Time.deltaTime);
            remainingLifetime -= Time.deltaTime;

            if (remainingLifetime <= 0f)
            {
                Expire();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!launched || other == null || ShouldIgnore(other) || !IsInTargetLayer(other.gameObject.layer))
            {
                return;
            }

            if (damage > 0 && TryGetDamageable(other, out var damageable))
            {
                damageable.TakeDamage(damage);
            }

            Expire();
        }

        public void Launch(ProjectileLaunchContext context)
        {
            transform.position = context.Origin;
            direction = context.Direction.sqrMagnitude > 0f ? context.Direction.normalized : Vector2.right;
            owner = context.Owner;
            speed = context.Speed > 0f ? context.Speed : defaultSpeed;
            damage = context.Damage > 0 ? context.Damage : defaultDamage;
            remainingLifetime = context.Lifetime > 0f ? context.Lifetime : defaultLifetime;
            targetLayerMask = context.TargetLayerMask.value != 0 ? context.TargetLayerMask : defaultTargetLayerMask;
            launched = true;
        }

        private bool ShouldIgnore(Collider2D other)
        {
            var otherTransform = other.transform;
            if (otherTransform == transform || otherTransform.IsChildOf(transform))
            {
                return true;
            }

            return owner != null && (otherTransform == owner || otherTransform.IsChildOf(owner));
        }

        private bool IsInTargetLayer(int layer)
        {
            return (targetLayerMask.value & (1 << layer)) != 0;
        }

        private static bool TryGetDamageable(Collider2D other, out IDamageable damageable)
        {
            if (other.TryGetComponent(out damageable))
            {
                return true;
            }

            damageable = other.GetComponentInParent<IDamageable>();
            return damageable != null;
        }

        private void EnsureCollider()
        {
            if (projectileCollider == null)
            {
                projectileCollider = GetComponent<Collider2D>();
            }

            if (projectileCollider != null)
            {
                projectileCollider.isTrigger = true;
            }
        }

        private void Expire()
        {
            launched = false;

            if (Application.isPlaying)
            {
                Destroy(gameObject);
            }
            else
            {
                DestroyImmediate(gameObject);
            }
        }

        private void NormalizeDefaults()
        {
            defaultSpeed = Mathf.Max(0f, defaultSpeed);
            defaultDamage = Mathf.Max(0, defaultDamage);
            defaultLifetime = Mathf.Max(0f, defaultLifetime);
        }
    }
}
