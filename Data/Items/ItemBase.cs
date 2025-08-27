using System;
using Systems.SimpleInventory.Data.Context;
using Systems.SimpleInventory.Data.Native.Item;
using UnityEngine;

namespace Systems.SimpleInventory.Data.Items
{
    /// <summary>
    ///     Basic class for inventory items - should be used as base for all inventory items
    ///     with custom logic.
    /// </summary>
    [Serializable] 
    public abstract class ItemBase : ScriptableObject, IComparable<ItemBase>, IComparable<ItemID>
    {
        /// <summary>
        ///     Identifier of this item
        /// </summary>
        [field: SerializeField] public ItemID Identifier { get; private set; } = ItemID.New();
        
        /// <summary>
        ///     Maximum stack count for this item.
        /// </summary>
        [field: SerializeField] public int MaxStack { get; private set; } = 1;

        /// <summary>
        ///     Checks if this item is equippable
        /// </summary>
        public bool IsEquippable => this is EquippableItemBase;
        
        /// <summary>
        ///     Checks if this item is usable
        /// </summary>
        public bool IsUsable => this is UsableItemBase;

        /// <summary>
        ///     Compares this item with another item, ignores MaxStackCount and focuses on Identifier
        /// </summary>
        public int CompareTo(ItemBase other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (other is null) return 1;
            return Identifier.CompareTo(other.Identifier);
        }

        /// <summary>
        ///     Compares this item with another item identifier
        /// </summary>
        public int CompareTo(ItemID other)
        {
            return Identifier.CompareTo(other);
        }

        /// <summary>
        ///     Event called when item is picked up
        /// </summary>
        /// <param name="context">Context of the pickup event</param>
        protected internal virtual void OnPickedUp(PickupItemContext context){}

        /// <summary>
        ///     Event called when item pickup fails
        /// </summary>
        /// <param name="context">Context of the pickup event</param>
        protected internal virtual void OnPickupFailed(PickupItemContext context){}
        
        // TODO: Events - drop, transfer
    }
}