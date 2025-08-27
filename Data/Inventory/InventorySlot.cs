using System;
using JetBrains.Annotations;
using Systems.SimpleInventory.Data.Items;
using UnityEngine;

namespace Systems.SimpleInventory.Data.Inventory
{
    /// <summary>
    ///     Represents a single slot in an inventory
    /// </summary>
    [Serializable]
    public sealed class InventorySlot
    {
        /// <summary>
        ///     Item in the slot
        /// </summary>
        [field: SerializeReference] public ItemBase Item { get; internal set; }
        
        /// <summary>
        ///     Amount of items in the slot
        /// </summary>
        [field: SerializeField] public int Amount { get; internal set; }
        
        /// <summary>
        ///     Space left in the slot
        /// </summary>
        public int SpaceLeft => Item.MaxStack - Amount;
        
        /// <summary>
        ///     Max stack size of the item
        /// </summary>
        public int MaxStack => Item.MaxStack;

        /// <summary>
        ///     Swaps the contents of two item slots
        /// </summary>
        public static void Swap([NotNull] InventorySlot a, [NotNull] InventorySlot b)
        {
            // Cache values
            ItemBase tempItem = a.Item;
            int tempAmount = a.Amount;
            
            // Swap values
            a.Item = b.Item;
            a.Amount = b.Amount;
            b.Item = tempItem;
            b.Amount = tempAmount;
        }
    }
}