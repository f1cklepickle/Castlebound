using Castlebound.Gameplay.Combat;
using UnityEngine;

namespace Castlebound.Gameplay.Inventory
{
    [CreateAssetMenu(menuName = "Castlebound/Items/Weapon Definition")]
    public class WeaponDefinition : ItemDefinition
    {
        [SerializeField] private CombatEquipmentProfile combatProfile;
        [SerializeField] private Vector2 hitboxSize = new Vector2(1f, 1f);
        [SerializeField] private Vector2 handleOffset = Vector2.zero;
        [SerializeField] private Vector2 hitboxOffset = Vector2.zero;

        public CombatEquipmentProfile CombatProfile { get => combatProfile; set => combatProfile = value; }
        public int Damage => combatProfile != null ? combatProfile.DamageBonus : 0;
        public float AttackSpeed => combatProfile != null ? combatProfile.AttackRateMultiplier : 1f;

        public Vector2 HitboxSize
        {
            get => hitboxSize;
            set => hitboxSize = value;
        }

        public float Knockback => combatProfile != null ? combatProfile.KnockbackBonus : 0f;
        public Sprite HandSprite => combatProfile != null ? combatProfile.HandSprite : null;

        public Vector2 HandleOffset
        {
            get => handleOffset;
            set => handleOffset = value;
        }

        public Vector2 HitboxOffset
        {
            get => hitboxOffset;
            set => hitboxOffset = value;
        }
    }
}
