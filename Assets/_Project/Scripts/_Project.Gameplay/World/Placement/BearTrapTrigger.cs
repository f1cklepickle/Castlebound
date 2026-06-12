using Castlebound.Gameplay.AI;
using Castlebound.Gameplay.Spawning;
using UnityEngine;

namespace Castlebound.Gameplay.World.Placement
{
    public class BearTrapTrigger : MonoBehaviour
    {
        [SerializeField] private int damage = 2;
        [SerializeField] private float holdDurationSeconds = 5f;
        [SerializeField] private int waveLifetime = 1;
        [SerializeField] private bool disappearAfterWaveEnd = true;
        [SerializeField] private LayerMask enemyLayers;
        [SerializeField] private BearTrapVisualState visualState;
        [SerializeField] private EnemySpawnerRunner waveRunner;

        private Collider2D triggerCollider;
        private bool isSpent;
        private int wavesSurvived;

        public int Damage => damage;
        public float HoldDurationSeconds => holdDurationSeconds;
        public int WaveLifetime => waveLifetime;
        public bool DisappearAfterWaveEnd => disappearAfterWaveEnd;
        public bool IsSpent => isSpent;
        public bool IsArmed => !isSpent;

        private void Reset()
        {
            NormalizeDefaults();
            EnsureReferences();
        }

        private void Awake()
        {
            NormalizeDefaults();
            EnsureReferences();
            ApplyArmedVisual();
        }

        private void OnEnable()
        {
            EnsureReferences();
            if (waveRunner != null)
            {
                waveRunner.OnWaveEnded += HandleWaveEnded;
            }
        }

        private void OnDisable()
        {
            if (waveRunner != null)
            {
                waveRunner.OnWaveEnded -= HandleWaveEnded;
            }
        }

        private void OnValidate()
        {
            NormalizeDefaults();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isSpent || other == null || !IsEnemy(other))
            {
                return;
            }

            var rootReceiver = ResolveRootReceiver(other);
            if (rootReceiver != null && rootReceiver.IsRooted)
            {
                return;
            }

            if (TryGetDamageable(other, out var damageable) && damage > 0)
            {
                damageable.TakeDamage(damage);
            }

            if (rootReceiver != null)
            {
                rootReceiver.RootAt(transform.position, holdDurationSeconds);
            }

            Spend();
        }

        public void HandleWaveEnded()
        {
            wavesSurvived++;
            if (wavesSurvived < waveLifetime)
            {
                return;
            }

            if (disappearAfterWaveEnd)
            {
                DestroyTrap();
                return;
            }

            ResetTrap();
        }

        public void ResetTrap()
        {
            isSpent = false;
            wavesSurvived = 0;
            ApplyArmedVisual();
        }

        private void Spend()
        {
            isSpent = true;
            ApplySpentVisual();
        }

        private void ApplyArmedVisual()
        {
            if (visualState == null)
            {
                visualState = GetComponent<BearTrapVisualState>();
            }

            visualState?.ApplyOpenVisual();
        }

        private void ApplySpentVisual()
        {
            if (visualState == null)
            {
                visualState = GetComponent<BearTrapVisualState>();
            }

            visualState?.ApplyClosedVisual();
        }

        private bool IsEnemy(Collider2D other)
        {
            if (HasEnemyTag(other.transform) || other.GetComponentInParent<EnemyController2D>() != null)
            {
                return true;
            }

            return (enemyLayers.value & (1 << other.gameObject.layer)) != 0;
        }

        private static bool HasEnemyTag(Transform source)
        {
            while (source != null)
            {
                if (source.CompareTag("Enemy"))
                {
                    return true;
                }

                source = source.parent;
            }

            return false;
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

        private static EnemyRootReceiver ResolveRootReceiver(Collider2D other)
        {
            var rootReceiver = other.GetComponentInParent<EnemyRootReceiver>();
            if (rootReceiver != null)
            {
                return rootReceiver;
            }

            var controller = other.GetComponentInParent<EnemyController2D>();
            if (controller != null)
            {
                return controller.gameObject.AddComponent<EnemyRootReceiver>();
            }

            var damageable = other.GetComponentInParent<IDamageable>();
            if (damageable is Component component)
            {
                return component.gameObject.AddComponent<EnemyRootReceiver>();
            }

            return other.gameObject.AddComponent<EnemyRootReceiver>();
        }

        private void EnsureReferences()
        {
            if (triggerCollider == null)
            {
                triggerCollider = GetComponent<Collider2D>();
            }

            if (triggerCollider != null)
            {
                triggerCollider.isTrigger = true;
            }

            if (visualState == null)
            {
                visualState = GetComponent<BearTrapVisualState>();
            }

            if (waveRunner == null)
            {
                waveRunner = FindObjectOfType<EnemySpawnerRunner>();
            }
        }

        private void NormalizeDefaults()
        {
            damage = Mathf.Max(0, damage);
            holdDurationSeconds = Mathf.Max(0f, holdDurationSeconds);
            waveLifetime = Mathf.Max(1, waveLifetime);

            if (enemyLayers.value == 0)
            {
                enemyLayers = LayerMask.GetMask("Enemies");
            }
        }

        private void DestroyTrap()
        {
            if (Application.isPlaying)
            {
                Destroy(gameObject);
                return;
            }

            DestroyImmediate(gameObject);
        }
    }
}
