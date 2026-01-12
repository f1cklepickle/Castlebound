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
        [SerializeField] private Sprite handSprite;
        [SerializeField] private Vector2 handleOffset = Vector2.zero;
        [SerializeField] private Vector2 hitboxOffset = Vector2.zero;

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

        public Sprite HandSprite
        {
            get => handSprite;
            set => handSprite = value;
        }

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
