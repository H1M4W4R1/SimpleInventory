namespace Systems.SimpleInventory.Data.Inventory
{
    /// <summary>
    ///     Reference to an item in inventory
    /// </summary>
    public readonly struct InventoryItemReference 
    {
        public readonly int slotIndex;
        public readonly WorldItem item;
        
        public InventoryItemReference(int slotIndex, WorldItem item)
        {
            this.slotIndex = slotIndex;
            this.item = item;
        }
    }
}