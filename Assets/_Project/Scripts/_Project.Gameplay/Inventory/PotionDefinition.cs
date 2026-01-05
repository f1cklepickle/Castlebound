using UnityEngine;

namespace Castlebound.Gameplay.Inventory
{
    [CreateAssetMenu(menuName = "Castlebound/Items/Potion Definition")]
    public class PotionDefinition : ItemDefinition
    {
        [SerializeField] private int healAmount = 1;
        [SerializeField] private float cooldownSeconds = 1f;

        public int HealAmount
        {
            get => healAmount;
            set => healAmount = value;
        }

        public float CooldownSeconds
        {
            get => cooldownSeconds;
            set => cooldownSeconds = value;
        }
    }
}
