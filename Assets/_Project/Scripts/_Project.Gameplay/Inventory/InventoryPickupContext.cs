namespace Castlebound.Gameplay.Inventory
{
    public readonly struct InventoryPickupContext
    {
        public InventoryPickupContext(InventoryState activeInventory, BackpackInventoryState backpack)
        {
            ActiveInventory = activeInventory;
            Backpack = backpack;
        }

        public InventoryState ActiveInventory { get; }
        public BackpackInventoryState Backpack { get; }
    }
}
