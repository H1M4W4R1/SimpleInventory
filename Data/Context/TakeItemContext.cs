using JetBrains.Annotations;
using Systems.SimpleInventory.Components.Inventory;
using Systems.SimpleInventory.Data.Inventory;
using Systems.SimpleInventory.Data.Items.Base;

namespace Systems.SimpleInventory.Data.Context
{
    public readonly ref struct TakeItemContext
    {
        [CanBeNull] public readonly WorldItem exactItem;
        [NotNull] public readonly ItemBase item;
        [NotNull] public readonly InventoryBase inventory;
        public readonly int amount;

        public TakeItemContext(
            [CanBeNull] WorldItem exactItem,
            [NotNull] ItemBase item,
            [NotNull] InventoryBase inventory,
            int amount)
        {
            this.exactItem = exactItem;
            this.item = item;
            this.inventory = inventory;
            this.amount = amount;
        }
    }
}