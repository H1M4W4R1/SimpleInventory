using JetBrains.Annotations;
using Systems.SimpleInventory.Components.Inventory;
using Systems.SimpleInventory.Data.Inventory;

namespace Systems.SimpleInventory.Data.Context.Internal
{
    /// <summary>
    ///     Represents information about inventory slot
    /// </summary>
    public readonly ref struct InventorySlotContext 
    {
        [CanBeNull] public readonly InventoryBase inventory;
        public readonly int slotIndex;

        [CanBeNull] public WorldItem Item
        {
            get
            {
                // ReSharper disable once UseNullPropagation
                if (inventory is null) return null;
                return inventory.GetItemAt(slotIndex);
            }
        }
        
        public InventorySlotContext([CanBeNull] InventoryBase inventory, int slotIndex)
        {
            this.inventory = inventory;
            this.slotIndex = slotIndex;
        }
    }
}