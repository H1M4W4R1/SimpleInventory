using Systems.SimpleInventory.Components.Inventory;
using Systems.SimpleInventory.Data.Inventory;

namespace Systems.SimpleInventory.Data.Context
{
    public readonly ref struct AddItemContext
    {
        public readonly WorldItem itemInstance;
        public readonly InventoryBase inventory;
        public readonly int amount;

        public AddItemContext(WorldItem itemInstance, InventoryBase inventory, int amount)
        {
            this.itemInstance = itemInstance;
            this.inventory = inventory;
            this.amount = amount;
        }
    }
}