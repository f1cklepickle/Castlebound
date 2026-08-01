using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    public class EnemyEquipment : MonoBehaviour
    {
        [SerializeField] private EnemyEquipmentDefinition spawnEquipment;
        [SerializeField] private SpriteRenderer weaponRenderer;

        public EnemyEquipmentDefinition SpawnEquipment => spawnEquipment;
        public EnemyEquipmentDefinition ActiveEquipment { get; private set; }

        private void Awake() => Equip(spawnEquipment);

        public bool Equip(EnemyEquipmentDefinition equipment)
        {
            ActiveEquipment = equipment;
            Sprite sprite = equipment != null ? equipment.HandSprite : null;
            if (weaponRenderer == null)
                return true;
            weaponRenderer.sprite = sprite;
            weaponRenderer.enabled = sprite != null;
            if (sprite == null)
                return true;

            weaponRenderer.transform.localPosition = equipment.HandlePosition;
            weaponRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, equipment.HandleRotation);
            Vector2 scale = equipment.HandleScale;
            weaponRenderer.transform.localScale = new Vector3(
                Mathf.Approximately(scale.x, 0f) ? 1f : scale.x,
                Mathf.Approximately(scale.y, 0f) ? 1f : scale.y,
                1f);
            return true;
        }
    }
}
