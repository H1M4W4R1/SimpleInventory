using JetBrains.Annotations;
using Systems.SimpleInventory.Components.Inventory;
using Systems.SimpleInventory.Data.Items;
using UnityEngine.Assertions;

namespace Systems.SimpleInventory.Data.Context
{
    public readonly ref struct DropItemContext
    {
        public readonly InventoryBase inventory;
        public readonly ItemBase item;
        public readonly int amount;

        public DropItemContext([NotNull] InventoryBase inventory, [NotNull] ItemBase item, int amount)
        {
            this.inventory = inventory;
            this.item = item;
            this.amount = amount;
        }
    }
}