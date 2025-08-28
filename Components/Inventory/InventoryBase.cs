using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Sirenix.Serialization;
using Systems.SimpleInventory.Components.Equipment;
using Systems.SimpleInventory.Components.Items.Pickup;
using Systems.SimpleInventory.Data;
using Systems.SimpleInventory.Data.Context;
using Systems.SimpleInventory.Data.Enums;
using Systems.SimpleInventory.Data.Inventory;
using Systems.SimpleInventory.Data.Items;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
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

        /// <summary>
        ///     Saves inventory data to binary format
        /// </summary>
        /// <returns>Inventory data in binary format</returns>
        public byte[] Save() => SerializationUtility.SerializeValue(_inventoryData, DataFormat.Binary);

        /// <summary>
        ///     Loads inventory data from binary format
        /// </summary>
        /// <param name="data">Inventory data in binary format</param>
        public void Load(byte[] data)
            => _inventoryData.AddRange(
                SerializationUtility.DeserializeValue<List<InventorySlot>>(data, DataFormat.Binary));

#region Item Access

        /// <summary>
        ///     Gets item at specified slot
        /// </summary>
        /// <param name="slotIndex">Index of slot</param>
        /// <returns>Found item or null if slot is out of bounds or empty</returns>
        [CanBeNull] public ItemBase GetItemAt(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _inventoryData.Count) return null;
            return _inventoryData[slotIndex].Item;
        }

        /// <summary>
        ///     Gets first item of specified type
        /// </summary>
        /// <typeparam name="TItemType">Type of item to get</typeparam>
        /// <returns>Item or null if no item of specified type is found</returns>
        public InventoryItemReference<TItemType> GetFirstItemOfType<TItemType>()
            where TItemType : ItemBase
        {
            // Loop through all items
            for (int i = 0; i < _inventoryData.Count; i++)
            {
                ItemBase itemData = _inventoryData[i].Item;

                // Check if item is of desired type and return reference
                if (itemData is TItemType item) return new InventoryItemReference<TItemType>(i, item);
            }

            return new InventoryItemReference<TItemType>(-1, null);
        }

        /// <summary>
        ///     Gets all items of specified type
        /// </summary>
        /// <typeparam name="TItemType">Type of item to get</typeparam>
        /// <returns>Read-only list of items of specified type</returns>
        [NotNull] public IReadOnlyList<InventoryItemReference<TItemType>> GetAllItemsOfType<TItemType>()
            where TItemType : ItemBase
        {
            List<InventoryItemReference<TItemType>> items = new();
            for (int i = 0; i < _inventoryData.Count; i++)
            {
                ItemBase itemData = _inventoryData[i].Item;

                // Check if item is of desired type and add to cache
                if (itemData is TItemType item) items.Add(new InventoryItemReference<TItemType>(i, item));
            }

            return items;
        }

#endregion

#region EquippableItemBase

        /// <summary>
        ///     Equips item from inventory
        /// </summary>
        /// <param name="slotIndex">Index of slot</param>
        /// <param name="toEquipment">Equipment to equip item to</param>
        /// <param name="removeFromInventory">Whether to remove item from inventory</param>
        /// <param name="allowReplace">Whether to allow replacing existing item</param>
        /// <returns>Result of the equip operation</returns>
        public EquipItemResult EquipItem(
            int slotIndex,
            [NotNull] EquipmentBase toEquipment,
            bool removeFromInventory = true,
            bool allowReplace = true)
        {
            // Check if slot is valid
            if (slotIndex < 0 || slotIndex >= _inventoryData.Count) return EquipItemResult.InvalidSlot;

            // Get item at slot
            ItemBase item = _inventoryData[slotIndex].Item;

            // Check if item is equippable
            if (item is not EquippableItemBase) return EquipItemResult.InvalidItem;

            // Create context
            EquipItemContext context = new(toEquipment, this, slotIndex, removeFromInventory: removeFromInventory,
                allowReplace: allowReplace);

            return toEquipment.Equip(context);
        }

        /// <summary>
        ///     Unequips item from equipment
        /// </summary>
        /// <param name="item">Item to unequip</param>
        /// <param name="fromEquipment">Equipment to unequip item from</param>
        /// <returns>Result of the unequip operation</returns>
        /// <remarks>
        ///     Do not use if item is already in inventory (will duplicate items).
        ///     In such case use <see cref="UnequipItem(int, EquipmentBase)"/>
        /// </remarks>
        public UnequipItemResult UnequipItem([NotNull] ItemBase item, [NotNull] EquipmentBase fromEquipment)
        {
            // Check if item is equippable
            if (item is not EquippableItemBase equippableItem) return UnequipItemResult.InvalidItem;

            // Create context
            UnequipItemContext context = new(this, fromEquipment, equippableItem, addToInventory: true);
            return fromEquipment.Unequip(context);
        }

        /// <summary>
        ///     Unequips item from inventory
        /// </summary>
        /// <param name="slotIndex">Index of slot</param>
        /// <param name="fromEquipment">Equipment to unequip item from</param>
        /// <returns>Result of the unequip operation</returns>
        /// <remarks>
        ///     If you are removing item from inventory on <see cref="EquipItem"/>,
        ///     use <see cref="UnequipItem(ItemBase, EquipmentBase)"/>
        /// </remarks>
        public UnequipItemResult UnequipItem(int slotIndex, [NotNull] EquipmentBase fromEquipment)
        {
            // Check if slot is valid
            if (slotIndex < 0 || slotIndex >= _inventoryData.Count) return UnequipItemResult.InvalidSlot;

            // Get item
            ItemBase item = _inventoryData[slotIndex].Item;

            // Check if item is equippable
            if (item is not EquippableItemBase equippableItem) return UnequipItemResult.InvalidItem;

            // Create context
            // We don't add to inventory as it's already here
            UnequipItemContext context = new(this, fromEquipment, equippableItem, addToInventory: false);
            return fromEquipment.Unequip(context);
        }

        /// <summary>
        ///     Equips any item of specified type
        /// </summary>
        /// <param name="toEquipment">Equipment to equip item to</param>
        /// <param name="removeFromInventory">Whether to remove item from inventory</param>
        /// <param name="allowReplace">Whether to allow replacing existing item</param>
        /// <typeparam name="TItemType">Item to equip</typeparam>
        /// <returns>Result of the equip operation</returns>
        public EquipItemResult EquipAnyItem<TItemType>(
            [NotNull] EquipmentBase toEquipment,
            bool removeFromInventory = true,
            bool allowReplace = true)
            where TItemType : EquippableItemBase
        {
            // Get first item
            InventoryItemReference<TItemType> itemReference = GetFirstItemOfType<TItemType>();
            if (itemReference.item is null) return EquipItemResult.InvalidItem;

            return EquipItem(itemReference.slotIndex, toEquipment, removeFromInventory, allowReplace);
        }

        /// <summary>
        ///     Equips any item of specified type
        /// </summary>
        /// <param name="toEquipment">Equipment to equip item to</param>
        /// <param name="removeFromInventory">Whether to remove item from inventory</param>
        /// <typeparam name="TItemType">Item to equip</typeparam>
        /// <returns>Result of the equip operation</returns>
        public UnequipItemResult UnequipAnyItem<TItemType>(
            [NotNull] EquipmentBase toEquipment,
            bool removeFromInventory = true)
            where TItemType : EquippableItemBase
        {
            // Get all items in inventory of specified type
            TItemType item  = toEquipment.GetFirstEquippedItemFor<TItemType>();
            if (ReferenceEquals(item, null)) return UnequipItemResult.InvalidItem;
            
            // Unequip item to inventory
            UnequipItem(item, toEquipment);

            // Item is not equipped
            return UnequipItemResult.NotEquipped;
        }

        /// <summary>
        ///     Equips best item of specified type
        /// </summary>
        /// <param name="toEquipment">Equipment to equip item to</param>
        /// <param name="removeFromInventory">Whether to remove item from inventory</param>
        /// <typeparam name="TItemType">Type of item to equip</typeparam>
        /// <returns>Result of the equip operation</returns>
        public EquipItemResult EquipBestItem<TItemType>(
            [NotNull] EquipmentBase toEquipment,
            bool removeFromInventory = true)
            where TItemType : EquippableItemBase, IComparable<TItemType>
        {
            // Get all items
            IReadOnlyList<InventoryItemReference<TItemType>> items = GetAllItemsOfType<TItemType>();
            if (items.Count == 0) return EquipItemResult.InvalidItem;

            // Find best item
            InventoryItemReference<TItemType> bestItem = items[0];
            for (int i = 1; i < items.Count; i++)
            {
                // Compare items
                if (items[i].item.CompareTo(bestItem.item) > 0) bestItem = items[i];
            }

            // Use best item
            return EquipItem(bestItem.slotIndex, toEquipment, removeFromInventory);
        }

#endregion

#region UsableItemBase

        /// <summary>
        ///     Uses item from inventory
        /// </summary>
        /// <param name="slotIndex">Index of slot</param>
        /// <returns>Result of the use operation</returns>
        public UseItemResult UseItem(int slotIndex)
        {
            // Check if slot is valid
            if (slotIndex < 0 || slotIndex >= _inventoryData.Count) return UseItemResult.InvalidItem;

            // Get item
            ItemBase item = _inventoryData[slotIndex].Item;

            // Check if item is equippable
            if (item is not UsableItemBase usableItem) return UseItemResult.InvalidItem;

            // Create context
            UseItemContext context = new(this, slotIndex);

            if (!usableItem.CanUse(context)) return UseItemResult.CannotBeUsed;
            usableItem.OnUse(context);
            return UseItemResult.UsedSuccessfully;
        }

        /// <summary>
        ///     Uses any item of specified type
        /// </summary>
        /// <typeparam name="TItemType">Type of item to use</typeparam>
        /// <returns>Result of the use operation</returns>
        public UseItemResult UseAnyItem<TItemType>()
            where TItemType : UsableItemBase
        {
            // Get first item
            InventoryItemReference<TItemType> itemReference = GetFirstItemOfType<TItemType>();
            if (itemReference.item is null) return UseItemResult.InvalidItem;

            return UseItem(itemReference.slotIndex);
        }

        /// <summary>
        ///     Uses best item of specified type
        /// </summary>
        /// <typeparam name="TItemType">Type of item to use</typeparam>
        /// <returns>Result of the use operation</returns>
        public UseItemResult UseBestItem<TItemType>()
            where TItemType : UsableItemBase, IComparable<TItemType>
        {
            // Get all items
            IReadOnlyList<InventoryItemReference<TItemType>> items = GetAllItemsOfType<TItemType>();
            if (items.Count == 0) return UseItemResult.InvalidItem;

            // Find best item
            InventoryItemReference<TItemType> bestItem = items[0];
            for (int i = 1; i < items.Count; i++)
            {
                // Compare items, conversion to interface prevents fuck-ups from compiler taking ItemBase.CompareTo
                // instead of TItemType.CompareTo
                IComparable<TItemType> itemCompare = items[i].item;
                if (itemCompare.CompareTo(bestItem.item) > 0) bestItem = items[i];
            }

            // Use best item
            return UseItem(bestItem.slotIndex);
        }

#endregion

#region Item transfer

        /// <summary>
        ///     Drops item as pickup object
        /// </summary>
        /// <param name="item">Item to drop</param>
        /// <param name="amount">Amount of items to drop</param>
        /// <typeparam name="TPickupItemType">Type of pickup component to use</typeparam>
        /// <returns>True if item was dropped, false otherwise</returns>
        public bool DropItemAs<TPickupItemType>(
            [NotNull] ItemBase item,
            int amount)
            where TPickupItemType : PickupItem, new()
        {
            // Try to take required items
            if (!TryTake(item, amount)) return false;

            // Spawn object
            SpawnItemObject<TPickupItemType>(item, amount, InventoryDropPosition.position,
                InventoryDropPosition.rotation, InventoryDropPosition);

            // Create context
            DropItemContext context = new(this, item, amount);

            // Call events
            OnItemDropped(context);
            item.OnItemDropped(context);
            return true;
        }

        /// <summary>
        ///     Drops item as pickup object
        /// </summary>
        /// <param name="slotIndex">Index of slot</param>
        /// <param name="amount">Amount of items to drop</param>
        /// <typeparam name="TPickupItemType">Type of pickup component to use</typeparam>
        /// <returns>True if item was dropped, false otherwise</returns>
        public bool DropItemAs<TPickupItemType>(
            int slotIndex,
            int amount)
            where TPickupItemType : PickupItem, new()
        {
            // Check if slot is valid
            if (slotIndex < 0 || slotIndex >= _inventoryData.Count) return false;

            // Get item
            ItemBase itemReference = GetItemAt(slotIndex);
            if (itemReference is null) return false;

            // Spawn object
            SpawnItemObject<TPickupItemType>(itemReference, amount, InventoryDropPosition.position,
                InventoryDropPosition.rotation, InventoryDropPosition);

            // Clear slot data
            ClearSlot(slotIndex);

            // Create context
            DropItemContext context = new(this, itemReference, amount);

            // Call events
            OnItemDropped(context);
            itemReference.OnItemDropped(context);
            return true;
        }


        /// <summary>
        ///     Transfers specified amount of items from this inventory to another inventory
        /// </summary>
        /// <param name="itemBase">Item to transfer</param>
        /// <param name="targetInventory">Inventory to transfer item to</param>
        /// <param name="amount">Amount of items to transfer</param>
        /// <returns>True if transfer was successful</returns>
        public bool TransferItems([NotNull] ItemBase itemBase, [NotNull] InventoryBase targetInventory, int amount)
        {
            // Check if this inventory has enough items
            if (!Has(itemBase, amount)) return false;

            // Check if other inventory has enough space
            if (!targetInventory.CanStore(itemBase, amount)) return false;

            // Transfer items by taking and adding to other inventory
            bool takeResult = TryTake(itemBase, amount);
            Assert.IsTrue(takeResult, "Failed to take items from inventory, this should never happen");

            int addResult = targetInventory.TryAdd(itemBase, amount);
            Assert.IsTrue(addResult == amount, "Failed to add items to inventory, this should never happen");

            // Create context
            TransferItemContext context = new(this, targetInventory, itemBase, amount);

            // Call events
            itemBase.OnTransfer(context);
            return true;
        }

        /// <summary>
        ///     Transfers an item from this inventory to another inventory
        /// </summary>
        /// <param name="sourceSlot">Slot index of item to transfer</param>
        /// <param name="targetInventory">Inventory to transfer item to</param>
        /// <param name="targetSlot">Slot index of item to transfer to</param>
        /// <param name="allowPartialTransfer">
        ///     If true items will be stacked together when slot is already occupied by same item
        /// </param>
        /// <param name="swapIfOccupied">
        ///     If true items will be swapped if target slot is occupied by different item
        /// </param>
        /// <returns>True if transfer was successful</returns>
        public bool TransferItem(
            int sourceSlot,
            [NotNull] InventoryBase targetInventory,
            int targetSlot,
            bool allowPartialTransfer = true,
            bool swapIfOccupied = true)
        {
            // Ensure slots are valid
            if (sourceSlot < 0 || sourceSlot >= _inventoryData.Count) return false;
            if (targetSlot < 0 || targetSlot >= targetInventory._inventoryData.Count) return false;


            // Get slots
            InventorySlot sourceSlotData = _inventoryData[sourceSlot];
            InventorySlot targetSlotData = targetInventory._inventoryData[targetSlot];

            // Handle target slot having same item id
            if (ReferenceEquals(sourceSlotData.Item, targetSlotData.Item))
            {
                // Handle stack transfer
                int spaceLeft = targetSlotData.SpaceLeft;
                if (spaceLeft < sourceSlotData.Amount && !allowPartialTransfer) return false;

                // Create context - one-way transfer
                TransferItemContext sourceTransferContext = new(this, targetInventory,
                    sourceSlotData.Item,
                    sourceSlotData.Amount);

                // Transfer stack (partially too) and complete
                int amountToTransfer = math.min(sourceSlotData.Amount, spaceLeft);
                targetSlotData.Amount += amountToTransfer;
                sourceSlotData.Amount -= amountToTransfer;

                // Call events
                sourceTransferContext.item.OnTransfer(sourceTransferContext);
                return true;
            }

            // Handle target slot being occupied (different item ID)
            if (!ReferenceEquals(targetSlotData.Item, null))
            {
                // Handle swap
                if (!swapIfOccupied) return false;

                // Create transfer context - two-way transfer
                TransferItemContext sourceTransferContext = new(this, targetInventory,
                    sourceSlotData.Item,
                    sourceSlotData.Amount);
                TransferItemContext targetTransferContext = new(targetInventory, this,
                    targetSlotData.Item,
                    targetSlotData.Amount);

                // Swap items
                InventorySlot.Swap(sourceSlotData, targetSlotData);

                // Call events
                sourceTransferContext.item.OnTransfer(sourceTransferContext);
                targetTransferContext.item.OnTransfer(targetTransferContext);
                return true;
            }
            else
            {
                // Create context - one-way transfer
                TransferItemContext sourceTransferContext = new(this, targetInventory,
                    sourceSlotData.Item,
                    sourceSlotData.Amount);

                // Handle target slot being empty
                targetSlotData.Item = sourceSlotData.Item;
                targetSlotData.Amount = sourceSlotData.Amount;
                sourceSlotData.Item = null;
                sourceSlotData.Amount = 0;

                // Call events
                sourceTransferContext.item.OnTransfer(sourceTransferContext);
                return true;
            }
        }

        /// <summary>
        ///     Swaps items in inventory, does not stack items.
        ///     For stack transfer use <see cref="TransferItem"/> with same inventory.
        /// </summary>
        /// <param name="slotIndex1">Index of first slot</param>
        /// <param name="slotIndex2">Index of second slot</param>
        /// <returns>True if swap was successful, false otherwise</returns>
        public bool SwapItems(int slotIndex1, int slotIndex2)
        {
            // Ensure slots are valid
            if (slotIndex1 < 0 || slotIndex1 >= _inventoryData.Count) return false;
            if (slotIndex2 < 0 || slotIndex2 >= _inventoryData.Count) return false;

            // Get slots
            InventorySlot slot1 = _inventoryData[slotIndex1];
            InventorySlot slot2 = _inventoryData[slotIndex2];

            // Swap slots
            InventorySlot.Swap(slot1, slot2);
            return true;
        }

#endregion

#region Core item handling (Add/Remove/Check)

        /// <summary>
        ///     Checks if inventory can store specified amount of items
        /// </summary>
        /// <param name="amount">Amount that may be stored</param>
        /// <typeparam name="TItemType">Type of item to check</typeparam>
        /// <returns>True if inventory can store specified amount of items, false otherwise</returns>
        public bool CanStore<TItemType>(int amount)
            where TItemType : ItemBase, new()
        {
            TItemType item = ItemsDatabase.GetExact<TItemType>();
            if (item is null) return false;
            return CanStore(item, amount);
        }

        /// <summary>
        ///     Checks if inventory can store specified amount of items
        /// </summary>
        /// <param name="itemBase">Item to check</param>
        /// <param name="amount">Amount that may be stored</param>
        /// <returns>True if inventory can store specified amount of items, false otherwise</returns>
        public bool CanStore([NotNull] ItemBase itemBase, int amount) => GetFreeSpaceFor(itemBase) >= amount;

        /// <summary>
        ///     Gets free space for item
        /// </summary>
        /// <typeparam name="TItemType">Type of item to get free space for</typeparam>
        /// <returns>Free space for item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public int GetFreeSpaceFor<TItemType>()
            where TItemType : ItemBase, new()
        {
            TItemType item = ItemsDatabase.GetExact<TItemType>();
            if (item is null) return 0;
            return GetFreeSpaceFor(item);
        }

        /// <summary>
        ///     Gets free space for item
        /// </summary>
        /// <param name="itemBase">Item to get free space for</param>
        /// <returns>Free space for item</returns>
        public int GetFreeSpaceFor([NotNull] ItemBase itemBase)
        {
            // Count free space for item
            int freeSpace = 0;
            for (int i = 0; i < _inventoryData.Count; i++)
            {
                InventorySlot slot = _inventoryData[i];
                if (ReferenceEquals(slot.Item, null))
                    freeSpace += itemBase.MaxStack;
                else if (ReferenceEquals(slot.Item, itemBase)) freeSpace += slot.SpaceLeft;
            }

            return freeSpace;
        }

        /// <summary>
        ///  Try to add item by type
        /// </summary>
        /// <param name="amount">Amount of items to add</param>
        /// <typeparam name="TItemType">Type of item to add</typeparam>
        /// <returns>Amount of items that could not be added</returns>
        public int TryAdd<TItemType>(int amount)
            where TItemType : ItemBase, new()
        {
            TItemType item = ItemsDatabase.GetExact<TItemType>();
            if (item is null) return amount;
            return TryAdd(item, amount);
        }

        /// <summary>
        ///     Tries to remove item by type
        /// </summary>
        /// <param name="amount">Amount of items to remove</param>
        /// <typeparam name="TItemType">Item type to remove</typeparam>
        /// <returns>True if items were removed, false otherwise</returns>
        public bool TryTake<TItemType>(int amount)
            where TItemType : ItemBase, new()
        {
            TItemType item = ItemsDatabase.GetExact<TItemType>();
            if (item is null) return false;
            return TryTake(item, amount);
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
        /// <returns>Amount of items that could not be added</returns>
        public int TryAdd([NotNull] ItemBase item, int amountToAdd)
        {
            // Prevent execution if inventory is not created
            if (_inventoryData is null) return amountToAdd;

            // Prevent execution if count is invalid
            if (amountToAdd <= 0) return amountToAdd;

            // Iterate through inventory slots
            // and attempt to add items if already occupied by same item id
            for (int i = 0; i < _inventoryData.Count; i++)
            {
                InventorySlot slot = _inventoryData[i];

                // Check if slot is occupied by same item
                if (!ReferenceEquals(slot.Item, item)) continue;

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
                InventorySlot slot = _inventoryData[i];

                // Check if slot is empty
                if (!ReferenceEquals(slot.Item, null)) continue;

                // Add items to slot
                int nToAdd = item.MaxStack;
                slot.Amount += nToAdd;
                amountToAdd -= nToAdd;
                slot.Item = item;

                // Check if all items were added
                if (amountToAdd == 0) break;
            }

            // Return remaining amount
            return amountToAdd;
        }

        /// <summary>
        ///     Tries to add items to inventory, if not enough space drops items
        /// </summary>
        /// <param name="item">Item to add</param>
        /// <param name="amountToAdd">Amount of items to add</param>
        public void TryAddOrDrop([NotNull] ItemBase item, int amountToAdd)
        {
            int remaining = TryAdd(item, amountToAdd);
            if (remaining == 0) return;

            DropItemAs<PickupItemWithDestroy>(item, remaining);
        }

        /// <summary>
        ///     Tries to remove items from inventory
        /// </summary>
        /// <param name="item">Item to remove</param>
        /// <param name="amountToTake">Amount of item to remove</param>
        /// <returns>True if items were removed, false otherwise</returns>
        public bool TryTake([NotNull] ItemBase item, int amountToTake)
        {
            // Prevent execution if inventory is not created
            if (_inventoryData is null) return false;

            // Prevent execution if count is invalid
            if (amountToTake <= 0) return false;

            int currentItemCount = 0;
            UnsafeList<int> itemSlots = new(32, Allocator.TempJob);

            // Compute all slots that contain item
            for (int i = 0; i < _inventoryData.Count; i++)
            {
                if (!ReferenceEquals(_inventoryData[i].Item, item)) continue;
                currentItemCount += _inventoryData[i].Amount;
                itemSlots.Add(i);
                if (currentItemCount >= amountToTake) break;
            }

            // Return false if not enough items
            if (currentItemCount < amountToTake)
            {
                itemSlots.Dispose();
                return false;
            }

            // Take enough items
            for (int i = 0; i < itemSlots.Length; i++)
            {
                InventorySlot slot = _inventoryData[itemSlots[i]];

                // Perform take operation
                int nToTake = math.min(amountToTake, slot.Amount);
                slot.Amount -= nToTake;
                amountToTake -= nToTake;

                // If slot is empty, remove item reference
                if (slot.Amount == 0) slot.Item = null;

                // Return true if enough items were taken
                if (amountToTake == 0) break;
            }

            itemSlots.Dispose();
            return true;
        }

        /// <summary>
        ///     Checks if inventory has enough items
        /// </summary>
        /// <param name="item">Item to check</param>
        /// <param name="amount">Amount of item to expect</param>
        /// <returns>True if inventory has enough items, false otherwise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool Has([NotNull] ItemBase item, int amount)
            => Count(item) >= amount;

        /// <summary>
        ///     Counts items in inventory
        /// </summary>
        /// <param name="item">Item to count</param>
        /// <returns>Count of items</returns>
        public int Count([NotNull] ItemBase item)
        {
            int totalItemCount = 0;

            for (int i = 0; i < _inventoryData.Count; i++)
            {
                if (ReferenceEquals(_inventoryData[i].Item, item)) totalItemCount += _inventoryData[i].Amount;
            }

            return totalItemCount;
        }

        /// <summary>
        ///     Clears specified slot
        /// </summary>
        /// <param name="slotIndex">Index of slot to clear</param>
        internal void ClearSlot(int slotIndex)
        {
            _inventoryData[slotIndex] = new InventorySlot();
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
        /// <param name="context">Context of the pickup event</param>
        protected internal virtual void OnItemPickedUp(in PickupItemContext context)
        {
        }

        /// <summary>
        ///     Called when item pickup fails
        /// </summary>
        /// <param name="context">Context of the pickup event</param>
        protected internal virtual void OnItemPickupFailed(in PickupItemContext context)
        {
        }

        /// <summary>
        ///     Called when item is dropped
        /// </summary>
        /// <param name="context">Context of the drop event</param>
        protected virtual void OnItemDropped(in DropItemContext context)
        {
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
            [NotNull] ItemBase item,
            int amount,
            in Vector3 position,
            in Quaternion rotation,
            [CanBeNull] Transform parent = null)
            where TPickupItemType : PickupItem, new() =>
            item.SpawnPickup<TPickupItemType>(amount, position, rotation, parent);

#endregion
    }
}