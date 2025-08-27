using Systems.SimpleInventory.Data.Items;

namespace Systems.SimpleInventory.Data.Inventory
{
    /// <summary>
    ///     Reference to an item in inventory
    /// </summary>
    public readonly struct InventoryItemReference<TItemType> where TItemType : ItemBase
    {
        public readonly int slotIndex;
        public readonly TItemType item;
        
        public InventoryItemReference(int slotIndex, TItemType item)
        {
            this.slotIndex = slotIndex;
            this.item = item;
        }
    }
}