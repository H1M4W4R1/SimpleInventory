using Systems.SimpleInventory.Data.Context;

namespace Systems.SimpleInventory.Components.Items.Pickup
{
    /// <summary>
    ///     Pick-up item that destroys itself after all items are picked up
    /// </summary>
    public sealed class PickupItemWithDestroy : PickupItem
    {
        protected internal override void OnPickupAttemptComplete(in PickupItemContext context)
        {
            if (Amount != 0) return;
            Destroy(gameObject);
        }
    }
}