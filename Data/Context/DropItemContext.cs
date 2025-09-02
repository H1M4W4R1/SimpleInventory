using JetBrains.Annotations;
using Systems.SimpleInventory.Components.Inventory;
using Systems.SimpleInventory.Data.Inventory;

namespace Systems.SimpleInventory.Data.Context
{
    /// <summary>
    ///     Context for item drop events
    /// </summary>
    public readonly ref struct DropItemContext
    {
        /// <summary>
        ///     Inventory from which item is dropped, can be null if item is dropped from world
        /// </summary>
        [CanBeNull] public readonly InventoryBase inventory;
        
        /// <summary>
        ///     Item being dropped
        /// </summary>
        [NotNull] public readonly WorldItem itemInstance;
        
        /// <summary>
        ///     Amount of item being dropped
        /// </summary>
        public readonly int amount;

        public DropItemContext([CanBeNull] InventoryBase inventory, [NotNull] WorldItem itemInstance, int amount)
        {
            this.inventory = inventory;
            this.itemInstance = itemInstance;
            this.amount = amount;
        }
    }
}