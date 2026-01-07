using UnityEngine;

namespace Castlebound.Gameplay.Inventory
{
    public class PotionDefinitionResolverComponent : MonoBehaviour, IPotionDefinitionResolver
    {
        [SerializeField] private PotionDefinition[] definitions;

        public PotionDefinition Resolve(string potionId)
        {
            if (definitions == null || string.IsNullOrWhiteSpace(potionId))
            {
                return null;
            }

            for (int i = 0; i < definitions.Length; i++)
            {
                var definition = definitions[i];
                if (definition != null && definition.ItemId == potionId)
                {
                    return definition;
                }
            }

            return null;
        }
    }
}
