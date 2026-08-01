using System;
using Castlebound.Gameplay.Inventory;
using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    public class EnemyEquipment : MonoBehaviour
    {
        public enum Loadout { Unarmed, Club }

        [Serializable]
        private struct WeaponPresentation
        {
            public Loadout loadout;
            public WeaponDefinition weapon;
            public Vector2 handlePosition;
            public float handleRotation;
            public Vector2 handleScale;
        }

        [SerializeField] private Loadout spawnLoadout = Loadout.Unarmed;
        [SerializeField] private SpriteRenderer weaponRenderer;
        [SerializeField] private WeaponPresentation[] weaponPresentations = Array.Empty<WeaponPresentation>();

        public Loadout SpawnLoadout => spawnLoadout;
        public Loadout EquippedLoadout { get; private set; }

        private void Awake() => Equip(spawnLoadout);

        public void Equip(Loadout loadout)
        {
            EquippedLoadout = loadout;
            WeaponPresentation presentation = default;
            bool found = false;
            for (int i = 0; i < weaponPresentations.Length; i++)
            {
                if (weaponPresentations[i].loadout != loadout)
                    continue;
                presentation = weaponPresentations[i];
                found = true;
                break;
            }

            Sprite sprite = found && presentation.weapon != null ? presentation.weapon.HandSprite : null;
            if (weaponRenderer == null)
                return;
            weaponRenderer.sprite = sprite;
            weaponRenderer.enabled = sprite != null;
            if (sprite == null)
                return;

            weaponRenderer.transform.localPosition = presentation.handlePosition;
            weaponRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, presentation.handleRotation);
            Vector2 scale = presentation.handleScale;
            weaponRenderer.transform.localScale = new Vector3(
                Mathf.Approximately(scale.x, 0f) ? 1f : scale.x,
                Mathf.Approximately(scale.y, 0f) ? 1f : scale.y,
                1f);
        }
    }
}
