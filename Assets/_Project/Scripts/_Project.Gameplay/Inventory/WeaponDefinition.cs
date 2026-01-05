using UnityEngine;

namespace Castlebound.Gameplay.Inventory
{
    [CreateAssetMenu(menuName = "Castlebound/Items/Weapon Definition")]
    public class WeaponDefinition : ItemDefinition
    {
        [SerializeField] private int damage = 1;
        [SerializeField] private float attackSpeed = 1f;
        [SerializeField] private Vector2 hitboxSize = new Vector2(1f, 1f);
        [SerializeField] private float knockback = 1f;

        public int Damage
        {
            get => damage;
            set => damage = value;
        }

        public float AttackSpeed
        {
            get => attackSpeed;
            set => attackSpeed = value;
        }

        public Vector2 HitboxSize
        {
            get => hitboxSize;
            set => hitboxSize = value;
        }

        public float Knockback
        {
            get => knockback;
            set => knockback = value;
        }
    }
}
