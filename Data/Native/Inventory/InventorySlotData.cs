using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Systems.SimpleInventory.Data.Items;
using Systems.SimpleInventory.Data.Native.Item;
using Unity.Burst;

namespace Systems.SimpleInventory.Data.Native.Inventory
{
    /// <summary>
    ///     Data for a single inventory slot.
    /// </summary>
    public struct InventorySlotData : IComparable<ItemID>, IComparable<ItemBase>
    {
        /// <summary>
        ///     Representation of an empty slot
        /// </summary>
        public static readonly InventorySlotData Empty = new(ItemID.Invalid, 0);
        
        /// <summary>
        ///     Item placed in the slot
        /// </summary>
        public readonly ItemData itemInfo;
        
        /// <summary>
        ///     Current stack count
        /// </summary>
        public int currentStack;

        /// <summary>
        ///     Checks if the slot is full.
        /// </summary>
        public bool IsFull => currentStack >= itemInfo.maxStack;
        
        /// <summary>
        ///     Checks space left in slot
        /// </summary>
        public int SpaceLeft => itemInfo.maxStack - currentStack;
        
        /// <summary>
        ///     Check if slot is empty (not assigned)
        /// </summary>
        public bool IsEmpty => itemInfo.itemID == ItemID.Invalid;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InventorySlotData([NotNull] ItemBase item, int currentStack = 0)
        {
            itemInfo = new(item.Identifier, item.MaxStack);
            this.currentStack = currentStack;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InventorySlotData(in ItemID itemID, int maxStack, int currentStack = 0)
        {
            itemInfo = new(itemID, maxStack);
            this.currentStack = currentStack;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(ItemID other) => itemInfo.itemID.CompareTo(other);

        [BurstDiscard]
        public int CompareTo([NotNull] ItemBase other) => itemInfo.itemID.CompareTo(other.Identifier);
    }
}