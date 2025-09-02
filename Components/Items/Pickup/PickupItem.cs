using JetBrains.Annotations;
using Systems.SimpleInventory.Components.Inventory;
using Systems.SimpleInventory.Data.Context;
using Systems.SimpleInventory.Data.Inventory;
using UnityEngine;

namespace Systems.SimpleInventory.Components.Items.Pickup
{
    /// <summary>
    ///     Item that can be picked up
    /// </summary>
    public abstract class PickupItem : MonoBehaviour
    {
        /// <summary>
        ///     Item that can be picked up
        /// </summary>
        [field: SerializeReference] public WorldItem ItemInstance { get; private set; }
        
        /// <summary>
        ///     Amount of items that can can be picked up from this item
        /// </summary>
        [field: SerializeField] public int Amount { get; private set; }

        /// <summary>
        ///     Method to configure PickupItem when dropping
        /// </summary>
        /// <param name="item">Item to drop</param>
        /// <param name="amount">Amount of items to drop</param>
        internal void SetData([NotNull] WorldItem item, int amount)
        {
            ItemInstance = item;
            Amount = amount;
        }
        
        /// <summary>
        ///     Picks up item
        /// </summary>
        /// <param name="toInventory">Inventory to pick up item to</param>
        public virtual void Pickup([NotNull] InventoryBase toInventory)
        {
            // Perform
            int amountLeft = toInventory.TryAdd(ItemInstance, Amount);
            int pickedUpAmount = Amount - amountLeft;
            
            // Create context of operation
            PickupItemContext context = new(this, toInventory, pickedUpAmount);
            
            // Call inventory and item picked up events
            if (pickedUpAmount > 0)
            {
                toInventory.OnItemPickedUp(context);
                ItemInstance.Item.OnPickup(context);
            }
            else
            {
                toInventory.OnItemPickupFailed(context);
                ItemInstance.Item.OnPickupFailed(context);
            }

            // Pickup was performed, update amount and check object events for re-pooling or destroy
            Amount = amountLeft;
            OnPickupAttemptComplete(context);
        }

        /// <summary>
        ///     Handles pickup performed, intended to be used for re-pooling or destroying the object,
        ///     can also trigger UI events when amount of picked up items is 0 to draw "Inventory full" message
        /// </summary>
        /// <param name="context">Context of pickup</param>
        protected internal abstract void OnPickupAttemptComplete(in PickupItemContext context);
    }
}