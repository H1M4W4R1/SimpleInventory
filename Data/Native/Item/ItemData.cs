namespace Systems.SimpleInventory.Data.Native.Item
{
    public readonly struct ItemData
    {
        public static readonly ItemData Invalid = new(ItemID.Invalid, 0);
        
        public readonly ItemID itemID;
        public readonly int maxStack;

        public ItemData(in ItemID itemID, int maxStack)
        {
            this.itemID = itemID;
            this.maxStack = maxStack;
        }

        
    }
}