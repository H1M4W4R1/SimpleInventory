using JetBrains.Annotations;
using Systems.SimpleInventory.Components.Inventory;
using Systems.SimpleInventory.Data.Inventory;

namespace Systems.SimpleInventory.Data.Context
{
    /// <summary>
    ///     Context for item transfer events - either within inventory or between inventories
    /// </summary>
    public ref struct TransferItemContext
    {
        /// <summary>
        ///     Origin inventory
        /// </summary>
        public readonly InventoryBase sourceInventory;

        /// <summary>
        ///     Target inventory
        /// </summary>
        public readonly InventoryBase targetInventory;

        /// <summary>
        ///     Source slot index
        /// </summary>
        public readonly int sourceSlotIndex;

        /// <summary>
        ///     Target slot index
        /// </summary>
        public readonly int targetSlotIndex;

        /// <summary>
        ///     Item being transferred
        /// </summary>
        [CanBeNull] public readonly WorldItem sourceItem;

        /// <summary>
        ///     Item being transferred
        /// </summary>
        [CanBeNull] public readonly WorldItem targetItem;

        /// <summary>
        ///     Amount of item being transferred
        /// </summary>
        public int sourceAmount;

        /// <summary>
        ///     Amount of item being transferred out
        /// </summary>
        public int targetAmount;

        /// <summary>
        ///     Transfer flags
        /// </summary>
        public ItemTransferFlags transferFlags;

        [CanBeNull] public InventorySlot SourceSlot =>
            sourceSlotIndex > 0 ? sourceInventory.GetSlotAt(sourceSlotIndex) : null;

        [CanBeNull] public InventorySlot TargetSlot =>
            targetSlotIndex > 0 ? targetInventory.GetSlotAt(targetSlotIndex) : null;

        public int SourceSpaceLeft => SourceSlot?.SpaceLeft ?? 0;
        public int TargetSpaceLeft => TargetSlot?.SpaceLeft ?? 0;

        /// <summary>
        ///     Checks if transfer is from specified inventory
        /// </summary>
        public bool IsSource([NotNull] InventoryBase inventory)
            => ReferenceEquals(sourceInventory, inventory);

        /// <summary>
        ///     Checks if transfer is to specified inventory
        /// </summary>
        public bool IsTarget([NotNull] InventoryBase inventory)
            => ReferenceEquals(targetInventory, inventory);

        /// <summary>
        ///     Check if transfer is within same inventory e.g. from slot A to slot B
        /// </summary>
        public bool IsWithinInventory => ReferenceEquals(sourceInventory, targetInventory);

        /// <summary>
        ///     Checks if transfer is between multiple slots
        /// </summary>
        public bool IsMultiSlotTransfer => sourceSlotIndex < 0 && targetSlotIndex < 0;

        public TransferItemContext(
            [NotNull] InventoryBase sourceInventory,
            [NotNull] InventoryBase targetInventory,
            WorldItem sourceItem,
            int sourceAmount) : this(sourceInventory, -1, targetInventory, 
            -1, sourceItem, null, sourceAmount, 0, ItemTransferFlags.None)
        {
        }

        public TransferItemContext(
            [NotNull] InventoryBase sourceInventory,
            [NotNull] InventoryBase targetInventory,
            WorldItem sourceItem,
            WorldItem targetItem,
            int sourceAmount,
            int targetAmount) : this(sourceInventory, -1, targetInventory, 
            -1, sourceItem, targetItem, sourceAmount, targetAmount, ItemTransferFlags.None)
        {
        }

        public TransferItemContext(
            [NotNull] InventoryBase sourceInventory,
            int sourceSlotIndex,
            [NotNull] InventoryBase targetInventory,
            int targetSlotIndex,
            [CanBeNull] WorldItem sourceItem,
            [CanBeNull] WorldItem targetItem,
            int sourceAmount,
            int targetAmount,
            ItemTransferFlags transferFlags)
        {
            this.sourceInventory = sourceInventory;
            this.sourceSlotIndex = sourceSlotIndex;
            this.targetInventory = targetInventory;
            this.targetSlotIndex = targetSlotIndex;
            this.sourceAmount = sourceAmount;
            this.targetAmount = targetAmount;
            this.sourceItem = sourceItem;
            this.targetItem = targetItem;
            this.transferFlags = transferFlags;
        }
    }
}