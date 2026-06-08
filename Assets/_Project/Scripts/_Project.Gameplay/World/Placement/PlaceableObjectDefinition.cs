using Castlebound.Gameplay.Castle;
using UnityEngine;

namespace Castlebound.Gameplay.World.Placement
{
    [CreateAssetMenu(menuName = "Castlebound/World/Placement/Placeable Object Definition")]
    public class PlaceableObjectDefinition : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private PlaceableObjectCategory category;
        [SerializeField] private int footprintWidth = 3;
        [SerializeField] private int footprintHeight = 3;
        [SerializeField] private GameObject prefab;

        public string Id
        {
            get => id;
            set => id = value;
        }

        public string DisplayName
        {
            get => displayName;
            set => displayName = value;
        }

        public PlaceableObjectCategory Category
        {
            get => category;
            set => category = value;
        }

        public int FootprintWidth
        {
            get => footprintWidth;
            set => footprintWidth = value;
        }

        public int FootprintHeight
        {
            get => footprintHeight;
            set => footprintHeight = value;
        }

        public GameObject Prefab
        {
            get => prefab;
            set => prefab = value;
        }

        public bool HasValidFootprint => footprintWidth > 0 && footprintHeight > 0;

        public GridFootprint Footprint => HasValidFootprint
            ? new GridFootprint(footprintWidth, footprintHeight)
            : default;

        public bool IsValid => !string.IsNullOrWhiteSpace(id)
            && !string.IsNullOrWhiteSpace(displayName)
            && HasValidFootprint
            && prefab != null;

        public void SetFootprint(GridFootprint value)
        {
            if (!value.IsValid)
            {
                throw new System.ArgumentException("Cannot assign an invalid grid footprint.", nameof(value));
            }

            footprintWidth = value.Width;
            footprintHeight = value.Height;
        }
    }
}
