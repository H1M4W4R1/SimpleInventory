using Systems.SimpleInventory.Components.Inventory;
using Systems.SimpleInventory.Data.Inventory;

namespace Systems.SimpleInventory.Data.Context
{
    public readonly ref struct AddItemContext
    {
        public readonly WorldItem item;
        public readonly InventoryBase inventory;
        public readonly int amount;

        public AddItemContext(WorldItem item, InventoryBase inventory, int amount)
        {
            this.item = item;
            this.inventory = inventory;
            this.amount = amount;
        }
    }
}