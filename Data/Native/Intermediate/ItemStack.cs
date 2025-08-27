using JetBrains.Annotations;
using Systems.SimpleInventory.Data.Items;
using Systems.SimpleInventory.Data.Native.Inventory;
using Systems.SimpleInventory.Data.Native.Item;

namespace Systems.SimpleInventory.Data.Native.Intermediate
{
    /// <summary>
    ///     Represents a stack of items, used to transfer items
    ///     between inventories or perform actions.
    ///
    ///     Burst-compatible to create fast item stack operations.
    /// </summary>
    public readonly ref struct ItemStack
    {
        /// <summary>
        ///     Item in the stack.
        /// </summary>
        public readonly ItemData itemInfo; // 20B
        
        /// <summary>
        ///     Count of items in the stack.
        /// </summary>
        public readonly int currentStack; // 4B

        /// <summary>
        ///     Creates a new instance of <see cref="ItemStack"/>.
        /// </summary>
        public ItemStack([NotNull] ItemBase item, int currentStack)
        {
            itemInfo = new(item.Identifier, item.MaxStack);
            this.currentStack = currentStack;
        }
        
        /// <summary>
        ///     Creates a new instance of <see cref="ItemStack"/>.
        /// </summary>
        public ItemStack(in ItemID item, int maxStack, int currentStack)
        {
            itemInfo = new(item, maxStack);
            this.currentStack = currentStack;
        }

        public ItemStack(in InventorySlotData inventorySlot)
        {
            itemInfo = inventorySlot.itemInfo;
            currentStack = inventorySlot.currentStack;
        }
    }
}