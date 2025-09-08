using System;
using JetBrains.Annotations;
using Systems.SimpleCore.Automation.Attributes;
using Systems.SimpleCore.Identifiers;
using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Utility.Enums;
using Systems.SimpleInventory.Abstract.Data;
using Systems.SimpleInventory.Components.Items.Pickup;
using Systems.SimpleInventory.Data;
using Systems.SimpleInventory.Data.Context;
using Systems.SimpleInventory.Data.Inventory;
using Systems.SimpleInventory.Operations;
using UnityEngine;

namespace Systems.SimpleInventory.Abstract.Items
{
    /// <summary>
    ///     Basic class for inventory items - should be used as base for all inventory items
    ///     with custom logic.
    /// </summary>
    [Serializable] [AutoCreate("Items", ItemsDatabase.LABEL)]
    public abstract class ItemBase : ScriptableObject, IComparable<ItemBase>, IComparable<Snowflake128>
    {
        /// <summary>
        ///     Identifier of this item
        /// </summary>
        [field: SerializeField] public Snowflake128 Identifier { get; private set; } = Snowflake128.New();

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

        public int CompareTo(Snowflake128 other)
        {
            return Identifier.CompareTo(other);
        }

#region Checks

        /// <summary>
        ///     Checks if item can be picked up
        /// </summary>
        protected internal OperationResult CanPickup(in PickupItemContext context) => InventoryOperations.Permitted();
        
        /// <summary>
        /// Checks if item can be added to inventory
        /// </summary>
        protected internal OperationResult CanAdd(in AddItemContext context) => InventoryOperations.Permitted();

        /// <summary>
        ///     Checks if item can be taken from inventory
        /// </summary>
        protected internal OperationResult CanTake(in TakeItemContext context) => InventoryOperations.Permitted();

        /// <summary>
        ///     Checks if item can be dropped
        /// </summary>
        protected internal OperationResult CanDrop(in DropItemContext context) => InventoryOperations.Permitted();

        /// <summary>
        ///     Checks if item can be transferred
        /// </summary>
        protected internal OperationResult CanTransfer(in TransferItemContext context) => InventoryOperations.Permitted();

#endregion

#region Events

        /// <summary>
        ///     Event called when item is picked up
        /// </summary>
        protected internal virtual void OnPickup(in PickupItemContext context, in OperationResult result, int amountLeft)
        {
        }

        /// <summary>
        ///     Event called when item pickup fails
        /// </summary>
        protected internal virtual void OnPickupFailed(
            in PickupItemContext context,
            in OperationResult result)
        {
        }

        /// <summary>
        ///     Called when item is dropped
        /// </summary>
        protected internal virtual void OnDrop(in DropItemContext context, in OperationResult result)
        {
        }

        /// <summary>
        ///     Called when item drop fails
        /// </summary>
        protected internal virtual void OnDropFailed(in DropItemContext context, in OperationResult result)
        {
        }

        /// <summary>
        ///     Called when item is added to inventory
        /// </summary>
        protected internal virtual void OnAddToInventory(in AddItemContext context, in OperationResult result, int amountLeft)
        {
        }

        /// <summary>
        ///     Called when item addition to inventory fails
        /// </summary>
        protected internal virtual void OnAddToInventoryFailed(
            in AddItemContext context,
            in OperationResult result)
        {
        }

        /// <summary>
        ///     Called when item is taken from inventory
        /// </summary>
        protected internal virtual void OnTakeFromInventory(
            in TakeItemContext context,
            in OperationResult result, int amountLeft)
        {
        }

        /// <summary>
        ///     Called when item removal from inventory fails
        /// </summary>
        protected internal virtual void OnTakeFromInventoryFailed(
            in TakeItemContext context,
            in OperationResult result)
        {
        }

        /// <summary>
        ///     Called when item is transferred
        /// </summary>
        protected internal virtual void OnInventoryTransfer(
            in TransferItemContext context,
            in OperationResult result)
        {
        }

        /// <summary>
        ///     Called when item transfer fails
        /// </summary>
        protected internal virtual void OnInventoryTransferFailed(
            in TransferItemContext context,
            in OperationResult result)
        {
        }

#endregion

#region Utility

        /// <summary>
        ///     Spawns item as pickup object, this triggers <see cref="OnDrop"/> event and should be used
        ///     from external scripts 
        /// </summary>
        /// <param name="itemObj">Item to spawn</param>
        /// <param name="amount">Amount of items to drop</param>
        /// <param name="position">Position to drop item at</param>
        /// <param name="rotation">Rotation of dropped item</param>
        /// <param name="parent">Parent of dropped item</param>
        /// <param name="actionSource">Source of action</param>
        /// <typeparam name="TPickupItemType">Type of pickup component to use</typeparam>
        public static void DropItem<TPickupItemType>(
            [NotNull] WorldItem itemObj,
            int amount,
            in Vector3 position,
            in Quaternion rotation,
            [CanBeNull] Transform parent = null,
            ActionSource actionSource = ActionSource.External)
            where TPickupItemType : PickupItem, new()
        {
            // Spawn pickup
            itemObj.Item.SpawnPickup<TPickupItemType>(itemObj, amount, position, rotation, parent);

            // Call event for external actions
            if (actionSource == ActionSource.Internal) return;
            itemObj.Item.OnDrop(new DropItemContext(null, itemObj, amount), 
                InventoryOperations.ItemsDropped());
        }


        /// <summary>
        ///     Spawns item as pickup object
        /// </summary>
        /// <param name="itemObj">Item object to spawn</param>
        /// <param name="amount">Amount of items to drop</param>
        /// <param name="position">Position to drop item at</param>
        /// <param name="rotation">Rotation of dropped item</param>
        /// <param name="parent">Parent of dropped item</param>
        /// <typeparam name="TPickupItemType">Type of pickup component to use</typeparam>
        /// <remarks>
        ///     Does not call <see cref="OnDrop"/>, see <see cref="DropItem{TPickupItemType}"/>
        ///     if you want to call it after item was dropped.
        /// </remarks>
        internal void SpawnPickup<TPickupItemType>(
            [NotNull] WorldItem itemObj,
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

            pickupObj.SetData(itemObj, amount);
        }

#endregion

        /// <summary>
        ///     Generates world item for this item
        /// </summary>
        /// <param name="itemData">Data for the world item</param>
        /// <returns>New world item</returns>
        [NotNull] public virtual WorldItem GenerateWorldItem([CanBeNull] ItemData itemData) =>
            new(this, itemData);
    }
}