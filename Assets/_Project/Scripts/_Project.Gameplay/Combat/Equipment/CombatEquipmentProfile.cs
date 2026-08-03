using Castlebound.Gameplay.Projectile;
using UnityEngine;

namespace Castlebound.Gameplay.Combat
{
    [CreateAssetMenu(menuName = "Castlebound/Combat/Equipment Profile")]
    public class CombatEquipmentProfile : ScriptableObject
    {
        [SerializeField] private string equipmentId;
        [SerializeField, Min(0)] private int damageBonus;
        [SerializeField, Min(0.01f)] private float attackRateMultiplier = 1f;
        [SerializeField] private float rangeBonus;
        [SerializeField] private float knockbackBonus;
        [SerializeField] private CombatEquipmentCapability requiredCapabilities;
        [Header("Presentation")]
        [SerializeField] private Sprite handSprite;
        [Header("Projectile Delivery")]
        [SerializeField] private ProjectileRuntime projectilePrefab;
        [SerializeField, Min(0f)] private float projectileSpeed;
        [SerializeField, Min(0f)] private float projectileLifetime;
        [SerializeField] private float projectileVisualAngleOffsetDegrees;

        public string EquipmentId { get => equipmentId; set => equipmentId = value; }
        public int DamageBonus { get => damageBonus; set => damageBonus = Mathf.Max(0, value); }
        public float AttackRateMultiplier { get => attackRateMultiplier; set => attackRateMultiplier = Mathf.Max(0.01f, value); }
        public float RangeBonus { get => rangeBonus; set => rangeBonus = value; }
        public float KnockbackBonus { get => knockbackBonus; set => knockbackBonus = value; }
        public CombatEquipmentCapability RequiredCapabilities { get => requiredCapabilities; set => requiredCapabilities = value; }
        public Sprite HandSprite { get => handSprite; set => handSprite = value; }
        public ProjectileRuntime ProjectilePrefab { get => projectilePrefab; set => projectilePrefab = value; }
        public float ProjectileSpeed { get => projectileSpeed; set => projectileSpeed = Mathf.Max(0f, value); }
        public float ProjectileLifetime { get => projectileLifetime; set => projectileLifetime = Mathf.Max(0f, value); }
        public float ProjectileVisualAngleOffsetDegrees { get => projectileVisualAngleOffsetDegrees; set => projectileVisualAngleOffsetDegrees = value; }

        public bool CanEquip(CombatEquipmentCapability holderCapabilities)
        {
            return (holderCapabilities & requiredCapabilities) == requiredCapabilities;
        }

        private void OnValidate()
        {
            damageBonus = Mathf.Max(0, damageBonus);
            attackRateMultiplier = Mathf.Max(0.01f, attackRateMultiplier);
            projectileSpeed = Mathf.Max(0f, projectileSpeed);
            projectileLifetime = Mathf.Max(0f, projectileLifetime);
        }
    }
}
