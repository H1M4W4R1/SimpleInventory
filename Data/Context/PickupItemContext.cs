using JetBrains.Annotations;
using Systems.SimpleInventory.Components.Inventory;
using Systems.SimpleInventory.Components.Items.Pickup;

namespace Systems.SimpleInventory.Data.Context
{
    /// <summary>
    ///     Context for picking up item
    /// </summary>
    public readonly ref struct PickupItemContext
    {
        /// <summary>
        ///     Source of pickup
        /// </summary>
        [NotNull] public readonly PickupItem pickupSource;
        
        /// <summary>
        ///     Inventory where item was added
        /// </summary>
        [NotNull] public readonly InventoryBase targetInventory;
        
        /// <summary>
        ///     Amount of items picked up
        /// </summary>
        public readonly int amountPickedUp;

        public PickupItemContext([NotNull] PickupItem pickupSource, [NotNull] InventoryBase targetInventory, int amountPickedUp)
        {
            this.pickupSource = pickupSource;
            this.targetInventory = targetInventory;
            this.amountPickedUp = amountPickedUp;
        }
    }
}