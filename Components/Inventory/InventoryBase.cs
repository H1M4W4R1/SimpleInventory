using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Utility.Enums;
using Systems.SimpleInventory.Components.Equipment;
using Systems.SimpleInventory.Components.Items.Pickup;
using Systems.SimpleInventory.Data;
using Systems.SimpleInventory.Data.Context;
using Systems.SimpleInventory.Data.Enums;
using Systems.SimpleInventory.Data.Inventory;
using Systems.SimpleInventory.Data.Items.Base;
using Systems.SimpleInventory.Data.Items.Data;
using Systems.SimpleInventory.Operations;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

namespace Systems.SimpleInventory.Components.Inventory
{
    /// <summary>
    ///     Represents inventory that can contain items
    /// </summary>
    public abstract class InventoryBase : MonoBehaviour
    {
        /// <summary>
        ///     Drop position for inventory
        /// </summary>
        [field: SerializeField] private Transform InventoryDropPosition { get; set; }

        /// <summary>
        ///     Size of inventory
        /// </summary>
        [field: SerializeField] public int InventorySize { get; private set; } = 2048;

        /// <summary>
        ///     Cache variable for inventory data
        /// </summary>
        // ReSharper disable once CollectionNeverUpdated.Local
        private readonly List<InventorySlot> _inventoryData = new();

#region Item Access

        /// <summary>
        ///     Gets item at specified slot
        /// </summary>
        /// <param name="slotIndex">Index of slot</param>
        /// <returns>Found item or null if slot is out of bounds or empty</returns>
        [CanBeNull] public WorldItem GetItemAt(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _inventoryData.Count) return null;
            return GetSlotAt(slotIndex).Item;
        }

        /// <summary>
        ///     Gets first item of specified type
        /// </summary>
        /// <typeparam name="TItemType">Type of item to get</typeparam>
        /// <returns>Item or null if no item of specified type is found</returns>
        public InventoryItemReference GetFirstItemOfType<TItemType>()
            where TItemType : ItemBase
        {
            // Loop through all items
            for (int i = 0; i < _inventoryData.Count; i++)
            {
                WorldItem itemData = GetSlotAt(i).Item;
                if (itemData is null) continue;

                // Check if item is of desired type and return reference
                if (itemData.Item is TItemType) return new InventoryItemReference(i, itemData);
            }

            return new InventoryItemReference(-1, null);
        }

        /// <summary>
        ///     Gets all items of specified type
        /// </summary>
        /// <typeparam name="TItemType">Type of item to get</typeparam>
        /// <returns>Read-only list of items of specified type</returns>
        [NotNull] public IReadOnlyList<InventoryItemReference> GetAllItemsOfType<TItemType>()
            where TItemType : ItemBase
        {
            List<InventoryItemReference> items = new();
            for (int i = 0; i < _inventoryData.Count; i++)
            {
                WorldItem itemData = GetSlotAt(i).Item;
                if (itemData is null) continue;

                // Check if item is of desired type and add to cache
                if (itemData.Item is TItemType) items.Add(new InventoryItemReference(i, itemData));
            }

            return items;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] [NotNull]
        protected internal InventorySlot GetSlotAt(int sourceSlotIndex)
        {
            Assert.IsTrue(sourceSlotIndex >= 0 && sourceSlotIndex < _inventoryData.Count);
            return _inventoryData[sourceSlotIndex];
        }

        [CanBeNull] public InventoryItemReference? GetBestItem<TItemType>()
            where TItemType : ItemBase
        {
            // Get all items
            IReadOnlyList<InventoryItemReference> items = GetAllItemsOfType<TItemType>();
            if (items.Count == 0) return null;

            // Find best item
            InventoryItemReference bestItemReference = items[0];
            for (int i = 1; i < items.Count; i++)
            {
                // Compare items
                int comparison = items[i].item.CompareTo(bestItemReference.item);
                if (comparison > 0) bestItemReference = items[i];
            }

            return bestItemReference;
        }

#endregion

#region EquippableItemBase

        /// <summary>
        ///     Equips item from inventory
        /// </summary>
        /// <param name="slotIndex">Index of slot</param>
        /// <param name="toEquipment">Equipment to equip item to</param>
        /// <param name="flags">Flags for equip operation</param>
        /// <param name="actionSource">Source of action</param>
        /// <returns>Result of the equip operation</returns>
        public OperationResult EquipItem(
            int slotIndex,
            [NotNull] EquipmentBase toEquipment,
            EquipmentModificationFlags flags = EquipmentModificationFlags.ModifyInventory |
                                               EquipmentModificationFlags.AllowItemSwap,
            ActionSource actionSource = ActionSource.External)
        {
            // Check if slot is valid
            if (slotIndex < 0 || slotIndex >= _inventoryData.Count) return InventoryOperations.InvalidSlotIndex();

            // Get item at slot
            WorldItem item = GetSlotAt(slotIndex).Item;
            if (item is null) return InventoryOperations.SlotIsEmpty();

            // Check if item is equippable
            if (item.Item is not EquippableItemBase) return InventoryOperations.ItemNotEquippable();

            // Create context
            EquipItemContext context = new(toEquipment, this, slotIndex, flags);
            return toEquipment.Equip(context, actionSource);
        }

        /// <summary>
        ///     Unequips item from equipment
        /// </summary>
        /// <param name="item">Item to unequip</param>
        /// <param name="fromEquipment">Equipment to unequip item from</param>
        /// <param name="flags">Flags for unequip operation</param>
        /// <param name="actionSource">Source of action</param>
        /// <returns>Result of the unequip operation</returns>
        public OperationResult UnequipItem(
            [NotNull] WorldItem item,
            [NotNull] EquipmentBase fromEquipment,
            EquipmentModificationFlags flags = EquipmentModificationFlags.ModifyInventory,
            ActionSource actionSource = ActionSource.External)
        {
            // Create context
            UnequipItemContext context = new(this, fromEquipment,
                item, flags);
            return fromEquipment.Unequip(context, actionSource);
        }

        /// <summary>
        ///     Unequips item from inventory
        /// </summary>
        /// <param name="slotIndex">Index of slot</param>
        /// <param name="fromEquipment">Equipment to unequip item from</param>
        /// <param name="flags">Flags for unequip operation</param>
        /// <param name="actionSource">Source of action</param>
        /// <returns>Result of the unequip operation</returns>
        public OperationResult UnequipItem(
            int slotIndex,
            [NotNull] EquipmentBase fromEquipment,
            EquipmentModificationFlags flags = EquipmentModificationFlags.ModifyInventory,
            ActionSource actionSource = ActionSource.External)
        {
            // Check if slot is valid
            if (slotIndex < 0 || slotIndex >= _inventoryData.Count) return InventoryOperations.InvalidSlotIndex();

            // Get item
            WorldItem item = GetSlotAt(slotIndex).Item;
            if (item is null) return InventoryOperations.SlotIsEmpty();

            // Call to default implementation
            return UnequipItem(item, fromEquipment, flags, actionSource);
        }

        /// <summary>
        ///     Equips any item of specified type
        /// </summary>
        /// <param name="toEquipment">Equipment to equip item to</param>
        /// <param name="flags">Flags for equip operation</param>
        /// <param name="actionSource">Source of action</param>
        /// <typeparam name="TItemType">Item to equip</typeparam>
        /// <returns>Result of the equip operation</returns>
        public OperationResult EquipAnyItem<TItemType>(
            [NotNull] EquipmentBase toEquipment,
            EquipmentModificationFlags flags = EquipmentModificationFlags.ModifyInventory |
                                               EquipmentModificationFlags.AllowItemSwap,
            ActionSource actionSource = ActionSource.External)
            where TItemType : EquippableItemBase
        {
            // Get first item
            InventoryItemReference itemReference = GetFirstItemOfType<TItemType>();
            if (itemReference.item is null) return InventoryOperations.ItemNotFound();

            return EquipItem(itemReference.slotIndex, toEquipment, flags, actionSource);
        }

        /// <summary>
        ///     Equips any item of specified type
        /// </summary>
        /// <param name="toEquipment">Equipment to equip item to</param>
        /// <param name="flags">Flags for unequip operation</param>
        /// <param name="actionSource">Source of action</param>
        /// <typeparam name="TItemType">Item to equip</typeparam>
        /// <returns>Result of the equip operation</returns>
        public OperationResult UnequipAnyItem<TItemType>(
            [NotNull] EquipmentBase toEquipment,
            EquipmentModificationFlags flags = EquipmentModificationFlags.ModifyInventory,
            ActionSource actionSource = ActionSource.External)
            where TItemType : EquippableItemBase
        {
            // Get all items in inventory of specified type
            WorldItem item = toEquipment.GetFirstEquippedItemFor<TItemType>();
            if (item is null) return EquipmentOperations.NotEquipped();

            // Unequip item to inventory
            return UnequipItem(item, toEquipment, flags, actionSource);
        }

        /// <summary>
        ///     Equips best item of specified type
        /// </summary>
        /// <param name="toEquipment">Equipment to equip item to</param>
        /// <param name="flags">Flags for equip operation</param>
        /// <typeparam name="TItemType">Type of item to equip</typeparam>
        /// <returns>Result of the equip operation</returns>
        public OperationResult EquipBestItem<TItemType>(
            [NotNull] EquipmentBase toEquipment,
            EquipmentModificationFlags flags = EquipmentModificationFlags.ModifyInventory |
                                               EquipmentModificationFlags.AllowItemSwap)
            where TItemType : EquippableItemBase, IComparable<TItemType>
        {
            // Get best item
            InventoryItemReference? bestItemReference = GetBestItem<TItemType>();
            if (bestItemReference is null) return InventoryOperations.ItemNotFound();

            // Use best item
            return EquipItem(bestItemReference.Value.slotIndex, toEquipment, flags);
        }

#endregion

#region UsableItemBase

        /// <summary>
        ///     Uses item from inventory
        /// </summary>
        /// <param name="slotIndex">Index of slot</param>
        /// <param name="actionSource">Source of action</param>
        /// <returns>Result of the use operation</returns>
        public OperationResult UseItem(int slotIndex, ActionSource actionSource = ActionSource.External)
        {
            // Check if slot is valid
            if (slotIndex < 0 || slotIndex >= _inventoryData.Count) return InventoryOperations.InvalidSlotIndex();

            // Get item
            WorldItem item = GetSlotAt(slotIndex).Item;

            // Check if item is equippable
            if (item is null) return InventoryOperations.SlotIsEmpty();
            if (item.Item is not UsableItemBase usableItem) return InventoryOperations.ItemNotUsable();

            // Create context
            UseItemContext context = new(this, slotIndex);

            OperationResult useItemResult = usableItem.CanUse(context);
            if (!useItemResult)
            {
                if (actionSource == ActionSource.Internal) return useItemResult;
                OnItemUseFailed(context, useItemResult);
                return useItemResult;
            }

            if (actionSource == ActionSource.Internal) return InventoryOperations.UsedSuccessfully();
            OnItemUsed(context, InventoryOperations.UsedSuccessfully());
            return InventoryOperations.UsedSuccessfully();
        }

        /// <summary>
        ///     Uses any item of specified type
        /// </summary>
        /// <typeparam name="TItemType">Type of item to use</typeparam>
        /// <returns>Result of the use operation</returns>
        public OperationResult UseAnyItem<TItemType>()
            where TItemType : UsableItemBase
        {
            // Get first item
            InventoryItemReference itemReference = GetFirstItemOfType<TItemType>();
            if (itemReference.item is null) return InventoryOperations.ItemNotFound();

            return UseItem(itemReference.slotIndex);
        }

        /// <summary>
        ///     Uses best item of specified type
        /// </summary>
        /// <typeparam name="TItemType">Type of item to use</typeparam>
        /// <returns>Result of the use operation</returns>
        public OperationResult UseBestItem<TItemType>()
            where TItemType : UsableItemBase, IComparable<TItemType>
        {
            // Get best item
            InventoryItemReference? bestItemReference = GetBestItem<TItemType>();
            if (bestItemReference is null) return InventoryOperations.ItemNotFound();

            // Use best item
            return UseItem(bestItemReference.Value.slotIndex);
        }

#endregion

#region Item dropping and pickup

        public OperationResult<int> TryPickupItem([NotNull] PickupItem pickup, int amount,
            ActionSource actionSource = ActionSource.External)
        {
            // Check if we can pickup specified amount of item
            PickupItemContext context = new(pickup, this, amount);
            OperationResult<int> result = CanPickupItem(context).WithData(amount);
            if (!result)
            {
                if (actionSource == ActionSource.Internal) return result;
                OnItemPickupFailed(context, result);
                return result;
            }

            return PickupItem(pickup, amount);
         
        }

        public OperationResult<int> PickupItem([NotNull] PickupItem pickup, int amount,
            ActionSource actionSource = ActionSource.External)
        {
            PickupItemContext context = new(pickup, this, amount);
            
            // Perform
            OperationResult<int> amountLeft = TryAdd(pickup.ItemInstance, amount);
            int pickedUpAmount = pickup.Amount - (int) amountLeft;

            // Call inventory and item picked up events
            if (pickedUpAmount > 0)
            {
                if (actionSource == ActionSource.Internal) 
                    return InventoryOperations.ItemsPickedUp().WithData(pickedUpAmount);
                OnItemPickedUp(context, amountLeft);
                return InventoryOperations.ItemsPickedUp().WithData(pickedUpAmount);
            }

            if (actionSource == ActionSource.Internal) return amountLeft;
            OnItemPickupFailed(context, amountLeft);
            return amountLeft;
        }
        
        /// <summary>
        ///     Drops item as pickup object
        /// </summary>
        /// <param name="item">Item to drop</param>
        /// <param name="amount">Amount of items to drop</param>
        /// <param name="actionSource">Source of action</param>
        /// <typeparam name="TPickupItemType">Type of pickup component to use</typeparam>
        /// <returns>True if item was dropped, false otherwise</returns>
        public OperationResult DropItemAs<TPickupItemType>(
            [NotNull] WorldItem item,
            int amount,
            ActionSource actionSource = ActionSource.External)
            where TPickupItemType : PickupItem, new()
        {
            // Create context
            DropItemContext context = new(this, item, amount);

            // Check if item can be dropped
            OperationResult dropItemResult = CanDropItem(context);
            if (!dropItemResult)
            {
                if (actionSource == ActionSource.Internal) return dropItemResult;
                OnItemDropFailed(context, dropItemResult);
                return dropItemResult;
            }

            // Try to take required items
            OperationResult<int> takeResult = TryTake(item, amount, ActionSource.Internal);
            if (!takeResult) return takeResult;

            // Spawn object
            SpawnItemObject<TPickupItemType>(item, amount, InventoryDropPosition.position,
                InventoryDropPosition.rotation, InventoryDropPosition);

            // Call events
            if (actionSource == ActionSource.Internal) return InventoryOperations.ItemsDropped();
            OnItemDropped(context, InventoryOperations.ItemsDropped());
            return InventoryOperations.ItemsDropped();
        }

        /// <summary>
        ///     Drops item as pickup object
        /// </summary>
        /// <param name="slotIndex">Index of slot</param>
        /// <param name="amount">Amount of items to drop</param>
        /// <param name="actionSource">Source of action</param>
        /// <typeparam name="TPickupItemType">Type of pickup component to use</typeparam>
        /// <returns>True if item was dropped, false otherwise</returns>
        public bool DropItemAs<TPickupItemType>(
            int slotIndex,
            int amount,
            ActionSource actionSource = ActionSource.External)
            where TPickupItemType : PickupItem, new()
        {
            // Check if slot is valid
            if (slotIndex < 0 || slotIndex >= _inventoryData.Count) return false;

            // Get item
            WorldItem itemReference = GetItemAt(slotIndex);
            if (itemReference is null) return false;

            // Fallback to original implementation
            return DropItemAs<TPickupItemType>(itemReference, amount, actionSource);
        }

#endregion

#region Item transfer

        /// <summary>
        ///     Transfers specified amount of items from this inventory to another inventory
        /// </summary>
        /// <param name="targetInventory">Inventory to transfer item to</param>
        /// <param name="sourceItem">Item to transfer from this inventory</param>
        /// <param name="sourceAmount">Amount of items to transfer from this inventory</param>
        /// <param name="targetItem">Item to transfer to this inventory</param>
        /// <param name="targetAmount">Amount of items to transfer to this inventory</param>
        /// <param name="actionSource">Source of action</param>
        /// <returns>True if transfer was successful</returns>
        public OperationResult TransferItems(
            [NotNull] InventoryBase targetInventory,
            [NotNull] WorldItem sourceItem,
            int sourceAmount,
            [CanBeNull] WorldItem targetItem = null,
            int targetAmount = 0,
            ActionSource actionSource = ActionSource.External)
        {
            // Create context
            TransferItemContext context = new(this, targetInventory, sourceItem, targetItem,
                sourceAmount, targetAmount);

            // Check if transfer is allowed
            OperationResult canTransferItem = CanTransferItem(context);
            if (!canTransferItem)
            {
                if (actionSource == ActionSource.Internal) return canTransferItem;
                OnItemTransferFailed(context, canTransferItem);
                return canTransferItem;
            }

            // Transfer items by taking and adding to other inventory
            bool takeResult = TryTake(sourceItem, sourceAmount, ActionSource.Internal);
            if (targetItem != null)
                takeResult |= targetInventory.TryTake(targetItem, targetAmount, ActionSource.Internal);
            Assert.IsTrue(takeResult, "Failed to take items from inventory, this should never happen");

            int addResult = (int) targetInventory.TryAdd(sourceItem, sourceAmount, ActionSource.Internal);
            if (targetItem != null) addResult |= (int) TryAdd(targetItem, targetAmount, ActionSource.Internal);
            Assert.IsTrue(addResult == sourceAmount, "Failed to add items to inventory, this should never happen");

            // Call events
            if (actionSource == ActionSource.Internal) return InventoryOperations.ItemsTransferred();
            OnItemTransferred(context, InventoryOperations.ItemsTransferred());
            targetInventory.OnItemTransferred(context, InventoryOperations.ItemsTransferred());
            return InventoryOperations.ItemsTransferred();
        }

        /// <summary>
        ///     Transfers an item from this inventory to another or same inventory
        /// </summary>
        /// <param name="sourceSlot">Slot index of item to transfer</param>
        /// <param name="targetInventory">Inventory to transfer item to</param>
        /// <param name="targetSlot">Slot index of item to transfer to</param>
        /// <param name="transferFlags">Transfer flags</param>
        /// <param name="actionSource">Source of action</param>
        /// <returns>True if transfer was successful</returns>
        public virtual OperationResult TransferItem(
            int sourceSlot,
            [NotNull] InventoryBase targetInventory,
            int targetSlot,
            ItemTransferFlags transferFlags = ItemTransferFlags.None,
            ActionSource actionSource = ActionSource.External)
        {
            // Ensure slots are valid
            if (sourceSlot < 0 || sourceSlot >= _inventoryData.Count)
                return InventoryOperations.InvalidSlotIndex();
            if (targetSlot < 0 || targetSlot >= targetInventory._inventoryData.Count)
                return InventoryOperations.InvalidSlotIndex();

            // Get slots
            InventorySlot sourceSlotData = GetSlotAt(sourceSlot);
            InventorySlot targetSlotData = targetInventory.GetSlotAt(targetSlot);

            // Create transfer context
            TransferItemContext itemTransferContext = new(
                this, sourceSlot, targetInventory, targetSlot, sourceSlotData.Item,
                targetSlotData.Item, sourceSlotData.Amount, targetSlotData.Amount, transferFlags);

            // Check transfer allowance for both inventories
            OperationResult sourceInventoryOperationResult = CanTransferItem(itemTransferContext);
            OperationResult targetInventoryOperationResult = targetInventory.CanTransferItem(itemTransferContext);
            if (!sourceInventoryOperationResult)
            {
                if (actionSource == ActionSource.Internal) return sourceInventoryOperationResult;
                OnItemTransferFailed(itemTransferContext, sourceInventoryOperationResult);
                return sourceInventoryOperationResult;
            }

            if (!targetInventoryOperationResult)
            {
                if (actionSource == ActionSource.Internal) return targetInventoryOperationResult;
                OnItemTransferFailed(itemTransferContext, targetInventoryOperationResult);
                return targetInventoryOperationResult;
            }

            // Handle transfers correctly
            if (Equals(sourceSlotData.Item, targetSlotData.Item))
                HandleSameItemTransfer(sourceSlotData, targetSlotData, transferFlags);
            else
                HandleItemSwap(sourceSlotData, targetSlotData);

            // Call events
            if (actionSource == ActionSource.Internal) return InventoryOperations.ItemsTransferred();
            OnItemTransferred(itemTransferContext, InventoryOperations.ItemsTransferred());
            targetInventory.OnItemTransferred(itemTransferContext, InventoryOperations.ItemsTransferred());
            return InventoryOperations.ItemsTransferred();
        }

        private void HandleSameItemTransfer(
            [NotNull] InventorySlot sourceSlotData,
            [NotNull] InventorySlot targetSlotData,
            ItemTransferFlags transferFlags)
        {
            // Handle item transfer properly
            if ((transferFlags & ItemTransferFlags.SwapIfOccupiedBySame) != 0)
                HandleSameItemSwap(sourceSlotData, targetSlotData, transferFlags);
            else
                HandleSameItemCombine(sourceSlotData, targetSlotData);
        }

        private void HandleSameItemCombine(
            [NotNull] InventorySlot sourceSlotData,
            [NotNull] InventorySlot targetSlotData)
        {
            // Compute space left for target slot
            int spaceLeft = targetSlotData.SpaceLeft;

            // Transfer stack (partially too) and complete
            int amountToTransfer = math.min(sourceSlotData.Amount, spaceLeft);
            targetSlotData.Amount += amountToTransfer;
            sourceSlotData.Amount -= amountToTransfer;

            // We should ensure that source slot is cleared when it reaches zero
            if (sourceSlotData.Amount == 0) ClearSlot(sourceSlotData);
        }

        private void HandleSameItemSwap(
            [NotNull] InventorySlot sourceSlotData,
            [NotNull] InventorySlot targetSlotData,
            ItemTransferFlags transferFlags)
        {
            Assert.IsTrue((transferFlags & ItemTransferFlags.SwapIfOccupiedBySame) != 0,
                "ItemTransferFlags.SwapIfOccupiedBySame must be set to use HandleSameItemSwap");
            HandleItemSwap(sourceSlotData, targetSlotData);
        }

        private void HandleItemSwap(
            [NotNull] InventorySlot sourceSlotData,
            [NotNull] InventorySlot targetSlotData)
        {
            // Swap items
            InventorySlot.Swap(sourceSlotData, targetSlotData);
        }

#endregion

#region Core item handling (Add/Remove/Check)

        /// <summary>
        ///     Checks if inventory can store specified amount of items
        /// </summary>
        /// <param name="itemBase">Item to check</param>
        /// <param name="amount">Amount that may be stored</param>
        /// <returns>True if inventory can store specified amount of items, false otherwise</returns>
        public bool CanStore([CanBeNull] WorldItem itemBase, int amount) =>
            GetFreeSpaceFor(itemBase) >= amount;

        /// <summary>
        ///     Gets free space for item
        /// </summary>
        /// <param name="itemBase">Item to get free space for</param>
        /// <returns>Free space for item, or max int if item is null</returns>
        public int GetFreeSpaceFor([CanBeNull] WorldItem itemBase)
        {
            // If item is null, return max int
            if (itemBase is null) return int.MaxValue;

            // Count free space for item
            int freeSpace = 0;
            for (int i = 0; i < _inventoryData.Count; i++)
            {
                InventorySlot slot = GetSlotAt(i);
                if (ReferenceEquals(slot.Item, null))
                    freeSpace += itemBase.MaxStack;
                else if (Equals(slot.Item, itemBase)) freeSpace += slot.SpaceLeft;
            }

            return freeSpace;
        }

        /// <summary>
        ///  Try to add item by type
        /// </summary>
        /// <param name="amount">Amount of items to add</param>
        /// <param name="itemData">Data for the world item</param>
        /// <param name="actionSource">Source of action</param>
        /// <typeparam name="TItemType">Type of item to add</typeparam>
        /// <returns>Amount of items that could not be added</returns>
        public OperationResult<int> TryAdd<TItemType>(
            int amount,
            [CanBeNull] ItemData itemData = null,
            ActionSource actionSource = ActionSource.External)
            where TItemType : ItemBase, new()
        {
            TItemType item = ItemsDatabase.GetExact<TItemType>();
            if (item is null) return InventoryOperations.ItemNotFound().WithData(amount);

            // Generate item
            WorldItem worldItem = item.GenerateWorldItem(itemData);
            return TryAdd(worldItem, amount, actionSource);
        }

        /// <summary>
        ///     Tries to remove item by type
        /// </summary>
        /// <param name="amount">Amount of items to remove</param>
        /// <param name="actionSource">Source of action</param>
        /// <typeparam name="TItemType">Item type to remove</typeparam>
        /// <returns>True if items were removed, false otherwise</returns>
        public OperationResult<int> TryTake<TItemType>(
            int amount,
            ActionSource actionSource = ActionSource.External)
            where TItemType : ItemBase, new()
        {
            TItemType item = ItemsDatabase.GetExact<TItemType>();
            if (item is null) return InventoryOperations.ItemNotFound().WithData(amount);
            return TryTake(item, amount, actionSource);
        }

        /// <summary>
        ///     Checks if inventory has enough items
        /// </summary>
        /// <param name="amount">Amount of items to check</param>
        /// <typeparam name="TItemType">Type of item to check</typeparam>
        /// <returns>True if inventory has enough items, false otherwise</returns>
        public bool Has<TItemType>(int amount)
            where TItemType : ItemBase, new()
        {
            TItemType item = ItemsDatabase.GetExact<TItemType>();
            if (item is null) return false;
            return Has(item, amount);
        }

        /// <summary>
        ///     Counts items of specified type
        /// </summary>
        /// <typeparam name="TItemType">Type of item to count</typeparam>
        /// <returns>Count of items of specified type</returns>
        public int Count<TItemType>()
            where TItemType : ItemBase, new()
        {
            TItemType item = ItemsDatabase.GetExact<TItemType>();
            if (item is null) return 0;
            return Count(item);
        }

        /// <summary>
        ///     Tries to add items to inventory
        /// </summary>
        /// <param name="item">Item to add</param>
        /// <param name="amountToAdd">Amount of item to add</param>
        /// <param name="actionSource">Source of action</param>
        /// <returns>Amount of items that could not be added</returns>
        public OperationResult<int> TryAdd(
            [CanBeNull] WorldItem item,
            int amountToAdd,
            ActionSource actionSource = ActionSource.External)
        {
            // Void items are always added
            if (item is null) return InventoryOperations.ItemsAdded().WithData(amountToAdd);

            // Prevent execution if inventory is not created
            if (_inventoryData is null) return InventoryOperations.InventoryNotCreated().WithData(amountToAdd);

            // Prevent execution if count is invalid
            if (amountToAdd <= 0) return InventoryOperations.InvalidAmount().WithData(0);

            // Create context
            AddItemContext context = new(item, this, amountToAdd);

            // Check if item can be added
            OperationResult<int> canAddResult = CanAddItem(context).WithData(amountToAdd);
            if (canAddResult) return Add(item, amountToAdd, actionSource);

            // Call events if not
            if (actionSource == ActionSource.Internal) return canAddResult;
            OnItemAddFailed(context, canAddResult);
            return canAddResult;
        }

        /// <summary>
        ///     Adds items to inventory
        /// </summary>
        /// <param name="item">Item to add</param>
        /// <param name="amountToAdd">Amount of items to add</param>
        /// <param name="actionSource">Source of action</param>
        /// <returns>Amount of items that could not be added</returns>
        protected virtual OperationResult<int> Add(
            [CanBeNull] WorldItem item,
            int amountToAdd,
            ActionSource actionSource = ActionSource.External)
        {
            // Void items are always added
            if (item is null) return InventoryOperations.ItemsAdded().WithData(amountToAdd);

            int originalAmount = amountToAdd;

            // Iterate through inventory slots
            // and attempt to add items if already occupied by same item id
            for (int i = 0; i < _inventoryData.Count; i++)
            {
                InventorySlot slot = GetSlotAt(i);

                // Check if slot is occupied by same item
                if (!Equals(slot.Item, item)) continue;

                // Check if slot has enough space
                int spaceLeft = slot.SpaceLeft;

                // Add items to slot
                int nToAdd = math.min(amountToAdd, spaceLeft);
                slot.Amount += nToAdd;
                amountToAdd -= nToAdd;

                // Check if all items were added
                if (amountToAdd == 0) break;
            }

            // Handle empty slots
            for (int i = 0; i < _inventoryData.Count; i++)
            {
                InventorySlot slot = GetSlotAt(i);

                // Check if slot is empty
                if (!ReferenceEquals(slot.Item, null)) continue;

                // Add items to slot
                int nToAdd = math.min(item.MaxStack, amountToAdd);
                slot.Amount += nToAdd;
                amountToAdd -= nToAdd;
                slot.Item = item;

                // Check if all items were added
                if (amountToAdd <= 0) break;
            }

            // Call events
            if (actionSource == ActionSource.Internal)
                return InventoryOperations.ItemsAdded().WithData(amountToAdd);
            OnItemAdded(new AddItemContext(item, this, originalAmount),
                InventoryOperations.ItemsAdded().WithData(amountToAdd));
            return InventoryOperations.ItemsAdded().WithData(amountToAdd);
        }

        /// <summary>
        ///     Tries to add items to inventory, if not enough space drops items
        /// </summary>
        /// <param name="item">Item to add</param>
        /// <param name="amountToAdd">Amount of items to add</param>
        /// <param name="actionSource">Source of action</param>
        public void TryAddOrDrop(
            [CanBeNull] WorldItem item,
            int amountToAdd,
            ActionSource actionSource = ActionSource.External)
        {
            // Skip if item is null
            if (item is null) return;

            OperationResult<int> remaining = TryAdd(item, amountToAdd, actionSource);
            if ((int) remaining == 0) return;

            // We drop it always as external action as it was failure of add operation
            DropItemAs<PickupItemWithDestroy>(item, (int) remaining);
        }

        /// <summary>
        ///     Tries to remove items from inventory
        /// </summary>
        /// <param name="item">Item to remove</param>
        /// <param name="amountToTake">Amount of item to remove</param>
        /// <param name="actionSource">Action source</param>
        /// <returns>Amount of items that could not be removed</returns>
        public OperationResult<int> TryTake(
            [CanBeNull] ItemBase item,
            int amountToTake,
            ActionSource actionSource = ActionSource.External)
        {
            // Void items are always removed
            if (item is null) return InventoryOperations.ItemsTaken().WithData(0);

            // Prevent execution if inventory is not created
            if (_inventoryData is null) return InventoryOperations.InventoryNotCreated().WithData(amountToTake);

            // Prevent execution if count is invalid
            if (amountToTake <= 0) return InventoryOperations.InvalidAmount().WithData(0);

            // Create context
            TakeItemContext context = new(item, this, amountToTake);

            // Check if item can be taken
            OperationResult<int> canTakeResult = CanTakeItem(context).WithData(amountToTake);
            if (!canTakeResult)
            {
                if (actionSource == ActionSource.Internal) return canTakeResult;
                OnItemTakeFailed(context, canTakeResult);
                return canTakeResult;
            }

            // Take item and verify
            OperationResult<int> amountLeft = Take(item, amountToTake, actionSource);
            Assert.AreEqual((int) amountLeft, 0, "Failed to take items from inventory, this should never happen");
            return amountLeft;
        }

        /// <summary>
        ///     Tries to remove items from inventory
        /// </summary>
        /// <param name="item">Item to remove</param>
        /// <param name="amountToTake">Amount of item to remove</param>
        /// <param name="actionSource">Action source</param>
        /// <returns>True if items were removed, false otherwise</returns>
        public OperationResult<int> TryTake(
            [CanBeNull] WorldItem item,
            int amountToTake,
            ActionSource actionSource = ActionSource.External)
        {
            // Void items are always removed
            if (item is null) return InventoryOperations.ItemsTaken().WithData(0);

            // Prevent execution if inventory is not created
            if (_inventoryData is null) return InventoryOperations.InventoryNotCreated().WithData(amountToTake);

            // Prevent execution if count is invalid
            if (amountToTake <= 0) return InventoryOperations.InvalidAmount().WithData(0);

            // Create context
            TakeItemContext context = new(item, this, amountToTake);

            // Check if item can be taken
            OperationResult<int> canTakeResult = CanTakeItem(context).WithData(amountToTake);
            if (!canTakeResult)
            {
                if (actionSource == ActionSource.Internal) return canTakeResult;
                OnItemTakeFailed(context, canTakeResult);
                return canTakeResult;
            }

            // Take item and verify
            OperationResult<int> amountLeft = Take(item, amountToTake, actionSource);
            Assert.AreEqual((int) amountLeft, 0, "Failed to take items from inventory, this should never happen");
            return amountLeft;
        }

        /// <summary>
        ///     Method to take all items from a specific slot
        /// </summary>
        /// <param name="slotSlotIndex">Slot index to take items from</param>
        /// <param name="actionSource">Source of action</param>
        /// <returns>0</returns>
        public OperationResult<int> Take(int slotSlotIndex, ActionSource actionSource)
        {
            // Get slot
            InventorySlot slot = GetSlotAt(slotSlotIndex);

            // Void items are always removed
            if (slot.Item is null) return InventoryOperations.ItemsTaken().WithData(0);

            // Amount of item in slot
            int amountTaken = slot.Amount;
            WorldItem cachedItem = slot.Item;

            // Create context
            TakeItemContext context = new(cachedItem, this, amountTaken);

            // Clear slot as we have taken the item
            ClearSlot(slot);

            // Call events
            OperationResult<int> opResult = InventoryOperations.ItemsTaken().WithData(0);
            if (actionSource == ActionSource.Internal) return opResult;
            OnItemTaken(context, opResult);
            return opResult;
        }

        /// <summary>
        ///     Take a specific item from inventory
        /// </summary>
        /// <param name="item">Item to take</param>
        /// <param name="amountToTake">Amount to take</param>
        /// <param name="actionSource">Action source</param>
        /// <returns>True if items were taken, false otherwise</returns>
        protected virtual OperationResult<int> Take(
            [CanBeNull] ItemBase item,
            int amountToTake,
            ActionSource actionSource = ActionSource.External)
        {
            // Void items are always removed
            if (item is null) return InventoryOperations.ItemsTaken().WithData(0);

            // Update context with real taken amount
            int amountLeft = Take(item, amountToTake);

            // Create context
            TakeItemContext context = new(item, this, amountToTake - amountLeft);

            OperationResult<int> opResult = InventoryOperations.ItemsTaken().WithData(amountLeft);
            if (actionSource == ActionSource.Internal) return opResult;
            OnItemTaken(context, opResult);
            return opResult;
        }

        /// <summary>
        ///     Take a specific world item from inventory
        /// </summary>
        /// <param name="item">Item to take</param>
        /// <param name="amountToTake">Amount of items to take</param>
        /// <param name="actionSource">Action source</param>
        /// <returns>Amount of items left to take</returns>
        protected virtual OperationResult<int> Take(
            [CanBeNull] WorldItem item,
            int amountToTake,
            ActionSource actionSource = ActionSource.External)
        {
            // Void items are always removed
            if (item is null) return InventoryOperations.ItemsTaken().WithData(0);

            // Update context with real taken amount
            int amountLeft = Take(item, amountToTake);

            // Create context
            TakeItemContext context = new(item, this, amountToTake - amountLeft);

            OperationResult<int> opResult = InventoryOperations.ItemsTaken().WithData(amountLeft);
            if (actionSource == ActionSource.Internal) return opResult;
            OnItemTaken(context, opResult);
            return opResult;
        }

        protected int Take<TItemType>([NotNull] TItemType item, int amountToTake)
        {
            // Ensure proper type
            Assert.IsTrue(item is ItemBase || item is WorldItem, "Invalid item type");

            // Compute all slots that contain item
            for (int i = 0; i < _inventoryData.Count; i++)
            {
                // Acquire data
                InventorySlot slot = GetSlotAt(i);
                WorldItem itemData = slot.Item;
                if (itemData is null) continue;

                // Check for methodology
                if (item is ItemBase itemBase)
                {
                    if (!Equals(itemData.Item, itemBase)) continue;
                }
                else if (item is WorldItem worldItem)
                {
                    if (!Equals(itemData.Item, worldItem)) continue;
                }

                // Perform take operation
                int nToTake = math.min(amountToTake, slot.Amount);
                slot.Amount -= nToTake;
                amountToTake -= nToTake;

                // If slot is empty, remove item reference
                if (slot.Amount == 0) ClearSlot(i);

                // Return true if enough items were taken
                if (amountToTake == 0) break;
            }

            return amountToTake;
        }

        /// <summary>
        ///     Checks if inventory has enough items
        /// </summary>
        /// <param name="item">Item to check</param>
        /// <param name="amount">Amount of item to expect</param>
        /// <returns>True if inventory has enough items, false otherwise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool Has([CanBeNull] WorldItem item, int amount)
            => Count(item) >= amount;

        /// <summary>
        ///     Checks if inventory has enough items
        /// </summary>
        /// <param name="item">Item to check</param>
        /// <param name="amount">Amount of item to expect</param>
        /// <returns>True if inventory has enough items, false otherwise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool Has([CanBeNull] ItemBase item, int amount)
            => Count(item) >= amount;

        /// <summary>
        ///     Counts items in inventory
        /// </summary>
        /// <param name="item">Item to count</param>
        /// <returns>Count of items or 0 if item null</returns>
        public int Count([CanBeNull] WorldItem item)
        {
            if (item is null) return 0;

            int totalItemCount = 0;

            for (int i = 0; i < _inventoryData.Count; i++)
            {
                InventorySlot slot = GetSlotAt(i);
                if (Equals(slot.Item, item)) totalItemCount += slot.Amount;
            }

            return totalItemCount;
        }

        /// <summary>
        ///     Counts items in inventory
        /// </summary>
        /// <param name="item">Item to count</param>
        /// <returns>Count of items or 0 if item null</returns>
        public int Count([CanBeNull] ItemBase item)
        {
            if (item is null) return 0;

            int totalItemCount = 0;

            for (int i = 0; i < _inventoryData.Count; i++)
            {
                InventorySlot slot = GetSlotAt(i);

                WorldItem itemData = slot.Item;
                if (itemData is null) continue;

                if (ReferenceEquals(itemData.Item, item)) totalItemCount += slot.Amount;
            }

            return totalItemCount;
        }

        /// <summary>
        ///     Gets free space at specified slot
        /// </summary>
        public int GetFreeSpaceAt(int slotIndex)
        {
            Assert.IsTrue(slotIndex >= 0 && slotIndex < _inventoryData.Count,
                "Invalid slot index");
            return GetFreeSpaceAt(GetSlotAt(slotIndex));
        }

        /// <summary>
        ///     Gets free space at specified slot
        /// </summary>
        public int GetFreeSpaceAt([NotNull] InventorySlot slot) => slot.SpaceLeft;

        /// <summary>
        ///     Clears specified slot
        /// </summary>
        /// <param name="slotIndex">Index of slot to clear</param>
        protected void ClearSlot(int slotIndex) => ClearSlot(GetSlotAt(slotIndex));

        /// <summary>
        ///     Clears specified slot
        /// </summary>
        protected void ClearSlot([NotNull] InventorySlot slot)
        {
            slot.Item = null;
            slot.Amount = 0;
        }

#endregion

#region Checks

        /// <summary>
        ///     Checks if item can be picked up
        /// </summary>
        public virtual OperationResult CanPickupItem(PickupItemContext checkContext) =>
            checkContext.pickupSource.ItemInstance.Item.CanPickup(checkContext);

        /// <summary>
        ///     Checks if item can be added to inventory
        /// </summary>
        public virtual OperationResult CanAddItem(AddItemContext context)
        {
            if (GetFreeSpaceFor(context.itemInstance) < context.amount)
                return InventoryOperations.NotEnoughSpace();

            return context.itemInstance.Item.CanAdd(context);
        }

        /// <summary>
        ///     Checks if item can be taken from inventory
        /// </summary>
        public virtual OperationResult CanTakeItem(TakeItemContext context)
        {
            // Check depending on context containing exact world item
            if (!ReferenceEquals(context.exactItem, null))
            {
                if (Count(context.exactItem) < context.amount) return InventoryOperations.NotEnoughItems();
            }
            else
            {
                if (Count(context.itemInstance) < context.amount) return InventoryOperations.NotEnoughItems();
            }

            return context.itemInstance.CanTake(context);
        }

        /// <summary>
        ///     Checks if item can be dropped from inventory
        /// </summary>
        public virtual OperationResult CanDropItem(DropItemContext context)
        {
            // Check if items can be taken
            OperationResult canTakeResult = CanTakeItem(new TakeItemContext(
                context.itemInstance, this, context.amount));
            if (!canTakeResult) return canTakeResult;

            return context.itemInstance.Item.CanDrop(context);
        }

        /// <summary>
        ///     Checks if item can be transferred
        /// </summary>
        /// <remarks>
        ///     It's heavily discouraged to override this method
        /// </remarks>
        public virtual OperationResult CanTransferItem(TransferItemContext context)
        {
            // Check per-item transfer conditions (for any transfer type)
            if (context.sourceItem is not null)
            {
                OperationResult canTransferResult = context.sourceItem.Item.CanTransfer(context);
                if (!canTransferResult) return canTransferResult;
            }

            if (context.targetItem is not null)
            {
                OperationResult canTransferResult = context.targetItem.Item.CanTransfer(context);
                if (!canTransferResult) return canTransferResult;
            }

            // Handle separate case for stupid multi-slot transfers
            if (context.IsMultiSlotTransfer)
            {
                // We're swap-transferring items between inventories
                if (!context.sourceInventory.Has(context.sourceItem, context.sourceAmount))
                    return InventoryOperations.NotEnoughItems();
                if (!context.sourceInventory.CanStore(context.targetItem, context.targetAmount))
                    return InventoryOperations.NotEnoughSpace();

                if (!context.targetInventory.Has(context.targetItem, context.targetAmount))
                    return InventoryOperations.NotEnoughItems();
                if (!context.targetInventory.CanStore(context.sourceItem, context.sourceAmount))
                    return InventoryOperations.NotEnoughSpace();

                return InventoryOperations.Permitted();
            }

            // If any slot is empty we can simply swap them, if both are empty then too
            if (context.sourceItem is null || context.targetItem is null) return InventoryOperations.Permitted();

            // Same-item check should be done only on source inventory side
            // to improve performance
            if (!context.IsSource(this)) return InventoryOperations.Permitted();

            // Check if same item, if not then we can easily swap those items unless
            // some other logic is overriding this behaviour.
            if (!Equals(context.sourceItem.Item, context.targetItem.Item)) return InventoryOperations.Permitted();

            // Check if items are designed to be swapped, if so then we can easily
            // perform the swap unless some other logic is overriding this behaviour.
            if ((context.transferFlags & ItemTransferFlags.SwapIfOccupiedBySame) != 0)
                return InventoryOperations.Permitted();

            // Compute sizes
            int sourceAmount = context.sourceAmount;
            int spaceLeft = context.TargetSpaceLeft;

            // Check if enough space for transfer or partial transfer is allowed
            // if no then we can't transfer
            if (spaceLeft < sourceAmount &&
                (context.transferFlags & ItemTransferFlags.AllowPartialTransfer) == 0)
                return InventoryOperations.NotEnoughSpace();

            // We can combine two items of same type
            return InventoryOperations.Permitted();
        }

#endregion

#region Events

        protected void Awake()
        {
            // Initialize inventory data
            for (int i = 0; i < InventorySize; i++) _inventoryData.Add(new InventorySlot());
        }

        /// <summary>
        ///     Called when item is picked up
        /// </summary>
        protected internal virtual void OnItemPickedUp(
            in PickupItemContext context,
            in OperationResult<int> resultAmountLeft)
        {
            context.pickupSource.ItemInstance.Item.OnPickup(context, resultAmountLeft);
        }

        /// <summary>
        ///     Called when item pickup fails
        /// </summary>
        protected internal virtual void OnItemPickupFailed(
            in PickupItemContext context,
            in OperationResult<int> resultAmountExpected)
        {
            context.pickupSource.ItemInstance.Item.OnPickupFailed(context, resultAmountExpected);
        }

        /// <summary>
        ///     Called when item is dropped
        /// </summary>
        protected virtual void OnItemDropped(in DropItemContext context, in OperationResult result)
        {
            context.itemInstance.Item.OnDrop(context, result);
        }

        /// <summary>
        ///     Called when item drop fails
        /// </summary>
        protected virtual void OnItemDropFailed(in DropItemContext context, in OperationResult resultAmountExpected)
        {
            context.itemInstance.Item.OnDropFailed(context, resultAmountExpected);
        }

        /// <summary>
        ///     Called when item is used
        /// </summary>
        protected virtual void OnItemUsed(in UseItemContext context, in OperationResult result)
        {
            context.itemBase.OnUse(context, result);
        }

        /// <summary>
        ///     Called when item use fails
        /// </summary>
        protected virtual void OnItemUseFailed(in UseItemContext context, in OperationResult result)
        {
            context.itemBase.OnUseFailed(context, result);
        }

        /// <summary>
        ///     Called when item is added to inventory
        /// </summary>
        protected virtual void OnItemAdded(in AddItemContext context, in OperationResult<int> resultAmountLeft)
        {
            context.itemInstance.Item.OnAddToInventory(context, resultAmountLeft);
        }

        /// <summary>
        ///     Called when item addition fails
        /// </summary>
        protected virtual void OnItemAddFailed(in AddItemContext context, in OperationResult<int> resultAmountExpected)
        {
            context.itemInstance.Item.OnAddToInventoryFailed(context, resultAmountExpected);
        }

        /// <summary>
        ///     Called when item is taken from inventory
        /// </summary>
        protected virtual void OnItemTaken(in TakeItemContext context, in OperationResult<int> resultAmountLeft)
        {
            context.itemInstance.OnTakeFromInventory(context, resultAmountLeft);
        }

        /// <summary>
        ///     Called when item take fails
        /// </summary>
        protected virtual void OnItemTakeFailed(in TakeItemContext context, in OperationResult<int> resultAmountExpected)
        {
            context.itemInstance.OnTakeFromInventoryFailed(context, resultAmountExpected);
        }

        /// <summary>
        ///     Executed when item is transferred from this inventory
        /// </summary>
        protected virtual void OnItemTransferred(in TransferItemContext context, in OperationResult result)
        {
            if (context.IsSource(this)) context.sourceItem?.Item.OnInventoryTransfer(context, result);
            if (context.IsTarget(this)) context.targetItem?.Item.OnInventoryTransfer(context, result);
        }

        /// <summary>
        ///     Executed when item transfer from this inventory fails
        /// </summary>
        protected virtual void OnItemTransferFailed(in TransferItemContext context, in OperationResult result)
        {
            if (context.IsSource(this)) context.sourceItem?.Item.OnInventoryTransferFailed(context, result);
            if (context.IsTarget(this)) context.targetItem?.Item.OnInventoryTransferFailed(context, result);
        }

#endregion

#region Utility

        /// <summary>
        ///     Spawns item as pickup object
        /// </summary>
        /// <param name="item">Item to drop</param>
        /// <param name="amount">Amount of items to drop</param>
        /// <param name="position">Position to drop item at</param>
        /// <param name="rotation">Rotation of dropped item</param>
        /// <param name="parent">Parent of dropped item</param>
        /// <typeparam name="TPickupItemType">Type of pickup component to use</typeparam>
        internal static void SpawnItemObject<TPickupItemType>(
            [NotNull] WorldItem item,
            int amount,
            in Vector3 position,
            in Quaternion rotation,
            [CanBeNull] Transform parent = null)
            where TPickupItemType : PickupItem, new() =>
            item.Item.SpawnPickup<TPickupItemType>(item, amount, position, rotation, parent);

#endregion
    }
}