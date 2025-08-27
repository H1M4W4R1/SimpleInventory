using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Systems.SimpleInventory.Components.Equipment;
using Systems.SimpleInventory.Data;
using Systems.SimpleInventory.Data.Context;
using Systems.SimpleInventory.Data.Enums;
using Systems.SimpleInventory.Data.Inventory;
using Systems.SimpleInventory.Data.Items;
using Systems.SimpleInventory.Data.Native.Inventory;
using Systems.SimpleInventory.Data.Native.Item;
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
        ///     Size of inventory
        /// </summary>
        [field: SerializeField] public int InventorySize { get; private set; } = 2048;

        /// <summary>
        ///     Cache variable for inventory data
        /// </summary>
        private InventoryData _inventoryData;

        // TODO: Convert into savable data and load from saved data
        // TODO: Drop

#region Item Access

        /// <summary>
        ///     Low-level access to inventory data
        /// </summary>
        /// <returns>Reference to inventory data</returns>
        public ref InventoryData GetNativeInventoryData() => ref _inventoryData;

        /// <summary>
        ///     Get item information at specified slot
        /// </summary>
        /// <param name="slotIndex">Slot index</param>
        /// <returns>Found item data or invalid item data if slot is out of bounds or empty</returns>
        public ItemData GetNativeItemData(int slotIndex)
        {
            InventorySlotData? slotData = null;
            _inventoryData.GetSlot(slotIndex, ref slotData);

            if (slotData == null) return ItemData.Invalid;
            return slotData.Value.itemInfo;
        }

        /// <summary>
        ///     Gets item at specified slot
        /// </summary>
        /// <param name="slotIndex">Index of slot</param>
        /// <returns>Found item or null if slot is out of bounds or empty</returns>
        [CanBeNull] public ItemBase GetItemAt(int slotIndex)
        {
            ItemData itemData = GetNativeItemData(slotIndex);
            return itemData.itemID == ItemID.Invalid ? null : ItemsDatabase.GetItem(itemData.itemID);
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
            for (int i = 0; i < InventorySize; i++)
            {
                // Check if item is valid
                ItemData itemData = GetNativeItemData(i);
                if (itemData.itemID == ItemID.Invalid) continue;

                // Check if item is of desired type and return reference
                if (ItemsDatabase.GetItem(itemData.itemID) is TItemType item)
                    return new InventoryItemReference<TItemType>(i, item);
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
            for (int i = 0; i < InventorySize; i++)
            {
                // Check if item is valid
                ItemData itemData = GetNativeItemData(i);
                if (itemData.itemID == ItemID.Invalid) continue;

                // Check if item is of desired type and add to cache
                if (ItemsDatabase.GetItem(itemData.itemID) is TItemType item)
                    items.Add(new InventoryItemReference<TItemType>(i, item));
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
        /// <returns>Result of the equip operation</returns>
        public EquipItemResult EquipItem(
            int slotIndex,
            [NotNull] EquipmentBase toEquipment,
            bool removeFromInventory = true)
        {
            // Get item at slot
            ItemData itemData = GetNativeItemData(slotIndex);
            if (itemData.itemID == ItemID.Invalid) return EquipItemResult.InvalidItem;

            // Get item
            ItemBase item = ItemsDatabase.GetItem(itemData.itemID);

            // Check if item is equippable
            if (item is not EquippableItemBase) return EquipItemResult.InvalidItem;

            // Create context
            EquipItemContext context = new(toEquipment, this, slotIndex, removeFromInventory: removeFromInventory);

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
        public EquipItemResult UnequipItem([NotNull] ItemBase item, [NotNull] EquipmentBase fromEquipment)
        {
            // Check if item is equippable
            if (item is not EquippableItemBase equippableItem) return EquipItemResult.InvalidItem;

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
        public EquipItemResult UnequipItem(int slotIndex, [NotNull] EquipmentBase fromEquipment)
        {
            // Get item at slot
            ItemData itemData = GetNativeItemData(slotIndex);
            if (itemData.itemID == ItemID.Invalid) return EquipItemResult.InvalidItem;

            // Get item
            ItemBase item = ItemsDatabase.GetItem(itemData.itemID);

            // Check if item is equippable
            if (item is not EquippableItemBase equippableItem) return EquipItemResult.InvalidItem;

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
        /// <typeparam name="TItemType">Item to equip</typeparam>
        /// <returns>Result of the equip operation</returns>
        public EquipItemResult EquipAnyItem<TItemType>(
            [NotNull] EquipmentBase toEquipment,
            bool removeFromInventory = true)
            where TItemType : EquippableItemBase
        {
            // Get first item
            InventoryItemReference<TItemType> itemReference = GetFirstItemOfType<TItemType>();
            if (itemReference.item is null) return EquipItemResult.InvalidItem;

            return EquipItem(itemReference.slotIndex, toEquipment, removeFromInventory);
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
            // Get item at slot
            ItemData itemData = GetNativeItemData(slotIndex);
            if (itemData.itemID == ItemID.Invalid) return UseItemResult.InvalidItem;

            // Get item
            ItemBase item = ItemsDatabase.GetItem(itemData.itemID);

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
                // Compare items
                if (items[i].item.CompareTo(bestItem.item) > 0) bestItem = items[i];
            }

            // Use best item
            return UseItem(bestItem.slotIndex);
        }

#endregion

#region Item transfer

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
            if (sourceSlot < 0 || sourceSlot >= _inventoryData.inventorySlots.Length) return false;
            if (targetSlot < 0 || targetSlot >= targetInventory._inventoryData.inventorySlots.Length) return false;

            // Get slots
            ref InventorySlotData sourceSlotData = ref _inventoryData[sourceSlot];
            ref InventorySlotData targetSlotData = ref targetInventory._inventoryData[targetSlot];

            // Handle target slot having same item id
            if (targetSlotData.itemInfo.itemID == sourceSlotData.itemInfo.itemID)
            {
                // Handle stack transfer
                int spaceLeft = targetSlotData.SpaceLeft;
                if (spaceLeft < sourceSlotData.currentStack && !allowPartialTransfer) return false;

                // Transfer stack (partially too) and complete
                int amountToTransfer = math.min(sourceSlotData.currentStack, spaceLeft);
                targetSlotData.currentStack += amountToTransfer;
                sourceSlotData.currentStack -= amountToTransfer;
                return true;
            }

            // Handle target slot being occupied (different item ID)
            if (targetSlotData.itemInfo.itemID != ItemID.Invalid)
            {
                // Handle swap
                if (!swapIfOccupied) return false;

                // Swap items
                (sourceSlotData, targetSlotData) = (targetSlotData, sourceSlotData);
                return true;
            }

            // Handle target slot being empty
            targetSlotData = sourceSlotData;
            sourceSlotData = InventorySlotData.Empty;
            return true;
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
            if (slotIndex1 < 0 || slotIndex1 >= _inventoryData.inventorySlots.Length) return false;
            if (slotIndex2 < 0 || slotIndex2 >= _inventoryData.inventorySlots.Length) return false;

            // Get slots
            ref InventorySlotData slot1 = ref _inventoryData[slotIndex1];
            ref InventorySlotData slot2 = ref _inventoryData[slotIndex2];

            // Swap slots
            (slot1, slot2) = (slot2, slot1);
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
            TItemType item = ItemsDatabase.GetItem<TItemType>();
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
            TItemType item = ItemsDatabase.GetItem<TItemType>();
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
            for (int i = 0; i < InventorySize; i++)
            {
                ItemData itemData = GetNativeItemData(i);
                if (itemData.itemID == ItemID.Invalid)
                    freeSpace += itemBase.MaxStack;
                else if (itemData.itemID == itemBase.Identifier)
                    freeSpace += _inventoryData.inventorySlots[i].SpaceLeft;
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
            TItemType item = ItemsDatabase.GetItem<TItemType>();
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
            TItemType item = ItemsDatabase.GetItem<TItemType>();
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
            TItemType item = ItemsDatabase.GetItem<TItemType>();
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
            TItemType item = ItemsDatabase.GetItem<TItemType>();
            if (item is null) return 0;
            return Count(item);
        }
        
        /// <summary>
        ///     Tries to add items to inventory, if inventory is full, tries to drop them
        /// </summary>
        /// <param name="item">Item to add</param>
        /// <param name="amount">Amount of item to add</param>
        public void TryAddDrop([NotNull] ItemBase item,
            int amount) => TryAdd(item, amount); // TODO: Implement drop logic

        /// <summary>
        ///     Tries to add items to inventory
        /// </summary>
        /// <param name="item">Item to add</param>
        /// <param name="amount">Amount of item to add</param>
        /// <returns>Amount of items that could not be added</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public int TryAdd([NotNull] ItemBase item, int amount)
            => _inventoryData.TryAdd(item.Identifier, amount, item.MaxStack);

        /// <summary>
        ///     Tries to remove items from inventory
        /// </summary>
        /// <param name="item">Item to remove</param>
        /// <param name="amount">Amount of item to remove</param>
        /// <returns>True if items were removed, false otherwise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool TryTake([NotNull] ItemBase item, int amount)
            => _inventoryData.TryTake(item.Identifier, amount);

        /// <summary>
        ///     Checks if inventory has enough items
        /// </summary>
        /// <param name="item">Item to check</param>
        /// <param name="amount">Amount of item to expect</param>
        /// <returns>True if inventory has enough items, false otherwise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool Has([NotNull] ItemBase item, int amount)
            => _inventoryData.Has(item.Identifier, amount);

        /// <summary>
        ///     Counts items in inventory
        /// </summary>
        /// <param name="item">Item to count</param>
        /// <returns>Count of items</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public int Count([NotNull] ItemBase item)
            => _inventoryData.Count(item.Identifier);

        /// <summary>
        ///     Clears specified slot
        /// </summary>
        /// <param name="slotIndex">Index of slot to clear</param>
        internal void ClearSlot(int slotIndex)
        {
            _inventoryData[slotIndex] = InventorySlotData.Empty;
        }

#endregion

#region Unity Lifecycle

        protected void Awake()
        {
            _inventoryData = new InventoryData(InventorySize);
        }

        protected void OnDestroy()
        {
            _inventoryData.Dispose();
        }

#endregion

#region Events

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

#endregion

        // TODO: ItemDropped event
    }
}