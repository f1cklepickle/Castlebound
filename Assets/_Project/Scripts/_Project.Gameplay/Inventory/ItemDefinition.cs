using System;
using UnityEngine;

namespace Castlebound.Gameplay.Inventory
{
    public class ItemDefinition : ScriptableObject
    {
        [SerializeField] private string itemId;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite icon;

        public string ItemId
        {
            get => itemId;
            set => itemId = value;
        }

        public string DisplayName
        {
            get => displayName;
            set => displayName = value;
        }

        public Sprite Icon
        {
            get => icon;
            set => icon = value;
        }

        public bool IsValidId => !string.IsNullOrWhiteSpace(itemId);
    }
}
