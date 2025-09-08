using Systems.SimpleCore.Operations;

namespace Systems.SimpleInventory.Components.Items.Pickup
{
    /// <summary>
    ///     Pick-up item that destroys itself after all items are picked up
    /// </summary>
    // ReSharper disable once ClassCanBeSealed.Global
    public class PickupItemWithDestroy : PickupItem
    {
        protected internal override void OnPickupAttemptComplete(in OperationResult result, int amountLeft)
        {
            if (Amount != 0) return;
            Destroy(gameObject);
        }
    }
}