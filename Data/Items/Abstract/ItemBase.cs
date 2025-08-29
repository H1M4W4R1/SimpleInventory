using System;
using JetBrains.Annotations;
using Systems.SimpleCore.Automation.Attributes;
using Systems.SimpleInventory.Components.Items.Pickup;
using Systems.SimpleInventory.Data.Context;
using Systems.SimpleInventory.Data.Native.Item;
using UnityEngine;

namespace Systems.SimpleInventory.Data.Items.Abstract
{
    /// <summary>
    ///     Basic class for inventory items - should be used as base for all inventory items
    ///     with custom logic.
    /// </summary>
    [Serializable] [AutoCreatedObject("Items", ItemsDatabase.LABEL)]
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
        ///     Prefab of the item when dropped
        /// </summary>
        [field: SerializeField] public GameObject DroppedItemPrefab { get; private set; }

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
        protected internal virtual void OnPickedUp(PickupItemContext context)
        {
        }

        /// <summary>
        ///     Event called when item pickup fails
        /// </summary>
        /// <param name="context">Context of the pickup event</param>
        protected internal virtual void OnPickupFailed(PickupItemContext context)
        {
        }

        /// <summary>
        ///     Called when item is dropped
        /// </summary>
        /// <param name="context">Context of the drop event</param>
        protected internal virtual void OnItemDropped(DropItemContext context)
        {
        }

        /// <summary>
        ///     Called when item is transferred
        /// </summary>
        /// <param name="context">Context of the transfer event</param>
        protected internal virtual void OnTransfer(TransferItemContext context)
        {
        }

#region Utility

        /// <summary>
        ///     Spawns item as pickup object, this triggers <see cref="OnItemDropped"/> event and should be used
        ///     from external scripts 
        /// </summary>
        /// <param name="item">Item to spawn</param>
        /// <param name="amount">Amount of items to drop</param>
        /// <param name="position">Position to drop item at</param>
        /// <param name="rotation">Rotation of dropped item</param>
        /// <param name="parent">Parent of dropped item</param>
        /// <typeparam name="TPickupItemType">Type of pickup component to use</typeparam>
        public static void DropItem<TPickupItemType>(
            [NotNull] ItemBase item,
            int amount,
            in Vector3 position,
            in Quaternion rotation,
            [CanBeNull] Transform parent = null)
            where TPickupItemType : PickupItem, new()
        {
            // Spawn pickup
            item.SpawnPickup<TPickupItemType>(amount, position, rotation, parent);
            
            // Call event
            item.OnItemDropped(new DropItemContext(null, item, amount));
        }


        /// <summary>
        ///     Spawns item as pickup object
        /// </summary>
        /// <param name="amount">Amount of items to drop</param>
        /// <param name="position">Position to drop item at</param>
        /// <param name="rotation">Rotation of dropped item</param>
        /// <param name="parent">Parent of dropped item</param>
        /// <typeparam name="TPickupItemType">Type of pickup component to use</typeparam>
        internal void SpawnPickup<TPickupItemType>(
            int amount,
            in Vector3 position,
            in Quaternion rotation,
            [CanBeNull] Transform parent = null)
            where TPickupItemType : PickupItem, new()
        {
            // Create object
            GameObject obj = Instantiate(DroppedItemPrefab);
            Transform objTransform = obj.transform;
            objTransform.position = position;
            objTransform.rotation = rotation;
            objTransform.SetParent(parent);

            // Add pickup component and set data
            if (!obj.TryGetComponent(out TPickupItemType pickupObj))
                pickupObj = obj.AddComponent<TPickupItemType>();

            pickupObj.SetData(this, amount);
        }

#endregion
    }
}