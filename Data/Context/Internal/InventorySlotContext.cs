using JetBrains.Annotations;
using Systems.SimpleInventory.Components.Inventory;
using Systems.SimpleInventory.Data.Items;

namespace Systems.SimpleInventory.Data.Context
{
    /// <summary>
    ///     Represents information about inventory slot
    /// </summary>
    public readonly ref struct InventorySlotContext 
    {
        [NotNull] public readonly InventoryBase inventory;
        public readonly int slotIndex;
        
        [CanBeNull] public ItemBase Item => inventory.GetItemAt(slotIndex);
        
        public InventorySlotContext([NotNull] InventoryBase inventory, int slotIndex)
        {
            this.inventory = inventory;
            this.slotIndex = slotIndex;
        }
    }
}