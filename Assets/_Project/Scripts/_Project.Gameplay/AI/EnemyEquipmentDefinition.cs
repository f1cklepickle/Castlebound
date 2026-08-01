using Castlebound.Gameplay.Projectile;
using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    [CreateAssetMenu(menuName = "Castlebound/Enemies/Equipment Definition")]
    public class EnemyEquipmentDefinition : ScriptableObject
    {
        [SerializeField] private string equipmentId = "unarmed";
        [SerializeField] private EnemyAttackRole compatibleRole;
        [SerializeField] private Sprite handSprite;
        [SerializeField] private Vector2 handlePosition;
        [SerializeField] private float handleRotation;
        [SerializeField] private Vector2 handleScale = Vector2.one;
        [Header("Projectile")]
        [SerializeField] private ProjectileRuntime projectilePrefab;
        [SerializeField, Min(0f)] private float projectileSpeed = 6f;
        [SerializeField, Min(0)] private int projectileDamage = 1;
        [SerializeField, Min(0f)] private float projectileLifetime = 3f;
        [SerializeField] private LayerMask projectileTargetLayerMask;
        [SerializeField] private float projectileVisualAngleOffsetDegrees;

        public string EquipmentId { get => equipmentId; set => equipmentId = value; }
        public EnemyAttackRole CompatibleRole { get => compatibleRole; set => compatibleRole = value; }
        public Sprite HandSprite { get => handSprite; set => handSprite = value; }
        public Vector2 HandlePosition { get => handlePosition; set => handlePosition = value; }
        public float HandleRotation { get => handleRotation; set => handleRotation = value; }
        public Vector2 HandleScale { get => handleScale; set => handleScale = value; }
        public ProjectileRuntime ProjectilePrefab { get => projectilePrefab; set => projectilePrefab = value; }
        public float ProjectileSpeed { get => projectileSpeed; set => projectileSpeed = Mathf.Max(0f, value); }
        public int ProjectileDamage { get => projectileDamage; set => projectileDamage = Mathf.Max(0, value); }
        public float ProjectileLifetime { get => projectileLifetime; set => projectileLifetime = Mathf.Max(0f, value); }
        public LayerMask ProjectileTargetLayerMask { get => projectileTargetLayerMask; set => projectileTargetLayerMask = value; }
        public float ProjectileVisualAngleOffsetDegrees { get => projectileVisualAngleOffsetDegrees; set => projectileVisualAngleOffsetDegrees = value; }

        public bool IsCompatibleWith(EnemyAttackRole attackRole)
        {
            return compatibleRole == EnemyAttackRole.None || compatibleRole == attackRole;
        }

        private void OnValidate()
        {
            projectileSpeed = Mathf.Max(0f, projectileSpeed);
            projectileDamage = Mathf.Max(0, projectileDamage);
            projectileLifetime = Mathf.Max(0f, projectileLifetime);
            handleScale = new Vector2(
                Mathf.Approximately(handleScale.x, 0f) ? 1f : handleScale.x,
                Mathf.Approximately(handleScale.y, 0f) ? 1f : handleScale.y);
        }
    }
}
