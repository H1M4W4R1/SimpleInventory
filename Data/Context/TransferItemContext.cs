using Systems.SimpleInventory.Data.Context;
using Systems.SimpleInventory.Data.Native.Intermediate;

namespace Systems.SimpleInventory.Data.Native.Context
{
    /// <summary>
    ///     Item transfer request from one inventory to another or within the same inventory.
    /// </summary>
    public readonly ref struct TransferItemContext
    {
        /// <summary>
        ///     Item stack to transfer.
        /// </summary>
        public readonly ItemStack itemStack;
        
        /// <summary>
        ///     Source slot of the transfer.
        /// </summary>
        public readonly InventorySlotContext sourceSlot;
        
        /// <summary>
        ///     Destination slot of the transfer.
        /// </summary>
        public readonly InventorySlotContext destinationSlot;

        public TransferItemContext(in ItemStack itemStack, in InventorySlotContext sourceSlot, in InventorySlotContext destinationSlot)
        {
            this.itemStack = itemStack;
            this.sourceSlot = sourceSlot;
            this.destinationSlot = destinationSlot;
        }
    }
}