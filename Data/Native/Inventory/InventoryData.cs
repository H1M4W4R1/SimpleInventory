using System;
using System.Runtime.CompilerServices;
using Systems.SimpleInventory.Data.Native.Item;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Systems.SimpleInventory.Data.Native.Inventory
{
    /// <summary>
    ///     Struct used to store all inventory items data
    /// </summary>
    [BurstCompile]
    public struct InventoryData : IDisposable
    {
        public UnsafeList<InventorySlotData> inventorySlots;
        public readonly int inventorySpace;

        public ref InventorySlotData this[int index] => ref inventorySlots.ElementAt(index);

        /// <summary>
        ///     Gets slot data from inventory
        /// </summary>
        /// <param name="index">Index of slot to get</param>
        /// <param name="slot">Slot data</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetSlot(int index, ref InventorySlotData? slot)
        {
            // Skip if index is out of bounds
            if (index < 0 || index >= inventorySlots.Length) return;
            if (slot == null) return;
            
            slot = inventorySlots.ElementAt(index);
        }
       
        /// <summary>
        ///     Adds items to inventory
        /// </summary>
        /// <param name="itemIdentifier">Item identifier to add</param>
        /// <param name="amountToAdd">Amount of items to add</param>
        /// <param name="itemMaxStack">Max stack of item to add</param>
        /// <returns>Amount of items that could not be added</returns>
        [BurstCompile]
        public int TryAdd(in ItemID itemIdentifier, int amountToAdd, int itemMaxStack)
        {
            // Prevent execution if inventory is not created
            if (!inventorySlots.IsCreated) return amountToAdd;
            
            // Prevent execution if count is invalid
            if (amountToAdd <= 0) return amountToAdd;

            // Iterate through inventory slots
            // and attempt to add items
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                ref InventorySlotData slot = ref inventorySlots.ElementAt(i);
                
                if (slot.itemInfo.itemID == itemIdentifier) // Handle existing item
                {
                    int nSpaceLeft = slot.itemInfo.maxStack - slot.currentStack;
                    int nToAdd = math.min(amountToAdd, nSpaceLeft);
                    
                    slot.currentStack += nToAdd;
                    amountToAdd -= nToAdd;
                }
                else if (slot.IsEmpty) // Handle empty slot
                {
                    // Create item slot
                    InventorySlotData itemSlot = new(itemIdentifier, itemMaxStack, math.min(amountToAdd, itemMaxStack));
                    inventorySlots[i] = itemSlot;
                    amountToAdd -= itemSlot.currentStack;
                }
                
                if (amountToAdd == 0) break;
            }
            
            // Return remaining amount
            return amountToAdd;
        }

        /// <summary>
        ///     Tries to take a certain amount of items from inventory
        /// </summary>
        /// <param name="itemIdentifier">Item ID to take</param>
        /// <param name="amountToTake">Amount of items to take</param>
        /// <returns>True if items were taken, false otherwise</returns>
        [BurstCompile]
        public bool TryTake(in ItemID itemIdentifier, int amountToTake)
        {
            // Prevent execution if inventory is not created
            if (!inventorySlots.IsCreated) return false;
            
            // Prevent execution if count is invalid
            if (amountToTake <= 0) return false;
            
            int currentItemCount = 0;
            UnsafeList<int> itemSlots = new(inventorySpace, Allocator.TempJob);
            
            // Compute all slots that contain item
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (inventorySlots[i].itemInfo.itemID != itemIdentifier) continue;
                currentItemCount += inventorySlots[i].currentStack;
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
                // Access slot reference
                ref InventorySlotData slot = ref inventorySlots.ElementAt(itemSlots[i]);
                
                // Perform take operation
                int nToTake = math.min(amountToTake, slot.currentStack);
                slot.currentStack -= nToTake;
                amountToTake -= nToTake;

                // If slot is empty, remove it
                if (slot.currentStack == 0)
                    inventorySlots[itemSlots[i]] = InventorySlotData.Empty;
                
                // Return true if enough items were taken
                if (amountToTake == 0) break;
            }

            itemSlots.Dispose();
            return true;
        }
        
        /// <summary>
        ///     Checks if inventory has enough items
        /// </summary>
        /// <param name="itemIdentifier">Item ID to check</param>
        /// <param name="count">Expected item count</param>
        /// <returns>True if inventory has enough items, false otherwise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(in ItemID itemIdentifier, int count) => Count(itemIdentifier) >= count;

        /// <summary>
        ///     Counts the total number of items in inventory
        /// </summary>
        /// <param name="itemIdentifier">Item ID to count</param>
        /// <returns>Total number of items</returns>
        [BurstCompile]
        public int Count(in ItemID itemIdentifier)
        {
            int totalItemCount = 0;
            
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (inventorySlots[i].itemInfo.itemID == itemIdentifier) 
                    totalItemCount += inventorySlots[i].currentStack;
            }

            return totalItemCount;
        }
        
        /// <summary>
        ///     Gets item ID from inventory
        /// </summary>
        /// <param name="index">Index of item to get</param>
        /// <returns>Item ID</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemID GetItemAt(int index)
        {
            if (index < 0 || index >= inventorySlots.Length) return ItemID.Invalid;
            return inventorySlots[index].itemInfo.itemID;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InventoryData(int inventorySpace)
        {
            this.inventorySpace = inventorySpace;
            inventorySlots = new UnsafeList<InventorySlotData>(inventorySpace, Allocator.Persistent);
            
            // Assign empty slots
            for (int i = 0; i < inventorySpace; i++)
                inventorySlots.Add(InventorySlotData.Empty);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            inventorySlots.Dispose();
        }
    }
}