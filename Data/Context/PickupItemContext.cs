using Systems.SimpleInventory.Components.Inventory;
using Systems.SimpleInventory.Components.Items.Pickup;

namespace Systems.SimpleInventory.Data.Context
{
    public readonly ref struct PickupItemContext
    {
        public readonly PickupItem pickupSource;
        public readonly InventoryBase targetInventory;
        public readonly int amountPickedUp;

        public PickupItemContext(PickupItem pickupSource, InventoryBase targetInventory, int amountPickedUp)
        {
            this.pickupSource = pickupSource;
            this.targetInventory = targetInventory;
            this.amountPickedUp = amountPickedUp;
        }
    }
}