using JetBrains.Annotations;
using Systems.SimpleInventory.Components.Inventory;
using Systems.SimpleInventory.Data.Inventory;
using Systems.SimpleInventory.Data.Items.Base;

namespace Systems.SimpleInventory.Data.Context
{
    public readonly ref struct TakeItemContext
    {
        [CanBeNull] public readonly WorldItem exactItem;
        [NotNull] public readonly ItemBase itemInstance;
        [NotNull] public readonly InventoryBase inventory;
        public readonly int amount;

        public TakeItemContext(
            [NotNull] WorldItem exactItem,
            [NotNull] InventoryBase inventory,
            int amount)
        {
            this.exactItem = exactItem;
            itemInstance = exactItem.Item;
            this.inventory = inventory;
            this.amount = amount;
        }
        
        public TakeItemContext(
            [NotNull] ItemBase itemInstance,
            [NotNull] InventoryBase inventory,
            int amount)
        {
            exactItem = null;
            this.itemInstance = itemInstance;
            this.inventory = inventory;
            this.amount = amount;
        }
    }
}