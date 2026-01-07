namespace Castlebound.Gameplay.Inventory
{
    public interface IPotionDefinitionResolver
    {
        PotionDefinition Resolve(string potionId);
    }
}
