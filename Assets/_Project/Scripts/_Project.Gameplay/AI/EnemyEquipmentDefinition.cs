using Castlebound.Gameplay.Combat;
using Castlebound.Gameplay.Projectile;
using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    [CreateAssetMenu(menuName = "Castlebound/Enemies/Equipment Definition")]
    public class EnemyEquipmentDefinition : ScriptableObject
    {
        [SerializeField] private CombatEquipmentProfile combatProfile;
        [SerializeField] private EnemyAttackRole compatibleRole;
        [SerializeField] private Vector2 handlePosition;
        [SerializeField] private float handleRotation;
        [SerializeField] private Vector2 handleScale = Vector2.one;
        [Header("Enemy Targeting")]
        [SerializeField] private LayerMask projectileTargetLayerMask;

        public CombatEquipmentProfile CombatProfile { get => combatProfile; set => combatProfile = value; }
        public string EquipmentId => combatProfile != null ? combatProfile.EquipmentId : null;
        public EnemyAttackRole CompatibleRole { get => compatibleRole; set => compatibleRole = value; }
        public Sprite HandSprite => combatProfile != null ? combatProfile.HandSprite : null;
        public Vector2 HandlePosition { get => handlePosition; set => handlePosition = value; }
        public float HandleRotation { get => handleRotation; set => handleRotation = value; }
        public Vector2 HandleScale { get => handleScale; set => handleScale = value; }
        public ProjectileRuntime ProjectilePrefab => combatProfile != null ? combatProfile.ProjectilePrefab : null;
        public float ProjectileSpeed => combatProfile != null ? combatProfile.ProjectileSpeed : 0f;
        public int ProjectileDamage => combatProfile != null ? combatProfile.DamageBonus : 0;
        public float ProjectileLifetime => combatProfile != null ? combatProfile.ProjectileLifetime : 0f;
        public LayerMask ProjectileTargetLayerMask { get => projectileTargetLayerMask; set => projectileTargetLayerMask = value; }
        public float ProjectileVisualAngleOffsetDegrees => combatProfile != null
            ? combatProfile.ProjectileVisualAngleOffsetDegrees
            : 0f;

        public bool IsCompatibleWith(EnemyAttackRole attackRole)
        {
            if (combatProfile == null ||
                (compatibleRole != EnemyAttackRole.None && compatibleRole != attackRole))
            {
                return false;
            }

            CombatEquipmentCapability capabilities = CombatEquipmentCapability.HandSocket;
            if (attackRole == EnemyAttackRole.Melee)
            {
                capabilities |= CombatEquipmentCapability.MeleeDelivery;
            }
            else if (attackRole == EnemyAttackRole.Ranged)
            {
                capabilities |= CombatEquipmentCapability.ProjectileDelivery;
            }

            return combatProfile.CanEquip(capabilities);
        }

        private void OnValidate()
        {
            handleScale = new Vector2(
                Mathf.Approximately(handleScale.x, 0f) ? 1f : handleScale.x,
                Mathf.Approximately(handleScale.y, 0f) ? 1f : handleScale.y);
        }
    }
}
