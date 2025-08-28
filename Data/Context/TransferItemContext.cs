using JetBrains.Annotations;
using Systems.SimpleInventory.Components.Inventory;
using Systems.SimpleInventory.Data.Items;

namespace Systems.SimpleInventory.Data.Context
{
    /// <summary>
    ///     Context for item transfer events - either within inventory or between inventories
    /// </summary>
    public readonly ref struct TransferItemContext
    {
        /// <summary>
        ///     Origin inventory
        /// </summary>
        [NotNull] public readonly InventoryBase sourceInventory;
        
        /// <summary>
        ///     Destination inventory
        /// </summary>
        [NotNull] public readonly InventoryBase destinationInventory;
        
        /// <summary>
        ///     Item being transferred
        /// </summary>
        [NotNull] public readonly ItemBase item;
        
        /// <summary>
        ///     Amount of item being transferred
        /// </summary>
        public readonly int amountTransferred;

        /// <summary>
        ///     Check if transfer is within same inventory e.g. from slot A to slot B
        /// </summary>
        public bool IsWithinInventory => ReferenceEquals(sourceInventory, destinationInventory);
        
        public TransferItemContext(
            [NotNull] InventoryBase sourceInventory,
            [NotNull] InventoryBase destinationInventory,
            [NotNull] ItemBase item,
            int amountTransferred)
        {
            this.sourceInventory = sourceInventory;
            this.destinationInventory = destinationInventory;
            this.amountTransferred = amountTransferred;
            this.item = item;
        }
    }
}