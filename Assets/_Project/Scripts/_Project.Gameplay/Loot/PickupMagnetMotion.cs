using Castlebound.Gameplay.Inventory;
using Castlebound.Gameplay.Player;
using UnityEngine;

namespace Castlebound.Gameplay.Loot
{
    [RequireComponent(typeof(ItemPickupComponent))]
    public sealed class PickupMagnetMotion : MonoBehaviour
    {
        [SerializeField] private PickupMagnetField magnetField;

        private ItemPickupComponent pickup;
        private LootSpillMotion spillMotion;

        public PickupMagnetField MagnetField
        {
            get => magnetField;
            set => magnetField = value;
        }

        private void Awake()
        {
            pickup = GetComponent<ItemPickupComponent>();
            spillMotion = GetComponent<LootSpillMotion>();
        }

        private void Update()
        {
            Step(Time.deltaTime);
        }

        public void Step(float deltaTime)
        {
            EnsureReferences();
            if (magnetField == null || pickup == null || (spillMotion != null && spillMotion.IsActive))
            {
                return;
            }

            if (!magnetField.IsWithinRange(transform.position) || !magnetField.CanAttract(pickup))
            {
                return;
            }

            float distance = magnetField.ActiveSpeed * Mathf.Max(0f, deltaTime);
            transform.position = Vector3.MoveTowards(transform.position, magnetField.transform.position, distance);
        }

        private void EnsureReferences()
        {
            if (pickup == null)
            {
                pickup = GetComponent<ItemPickupComponent>();
            }

            if (spillMotion == null)
            {
                spillMotion = GetComponent<LootSpillMotion>();
            }

            if (magnetField == null)
            {
                magnetField = FindObjectOfType<PickupMagnetField>();
            }
        }
    }
}
