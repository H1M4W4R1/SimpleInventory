using System;
using JetBrains.Annotations;
using Systems.SimpleInventory.Data.Items.Base;
using Systems.SimpleInventory.Data.Items.Data;
using UnityEngine;

namespace Systems.SimpleInventory.Data.Inventory
{
    
    /// <summary>
    ///     Represents item in world space
    /// </summary>
    [Serializable]
    public sealed class WorldItem : IComparable<WorldItem>
    {
        /// <summary>
        ///     Base item of this world item
        /// </summary>
        [NotNull] [field: SerializeReference] public ItemBase Item { get; private set; }
        
        /// <summary>
        ///     Item data, used to store world-level item information such as prefix
        ///     levels, implicit values etc.
        /// </summary>
        [field: SerializeReference] [CanBeNull] public ItemData Data { get; private set; }
        
        public int MaxStack => Item.MaxStack;

        // Block constructor
        internal WorldItem([NotNull] ItemBase item, [CanBeNull] ItemData data)
        {
            Item = item;
            Data = data;
        }

        /// <summary>
        ///     Compares this world item to another world item
        /// </summary>
        public int CompareTo([NotNull] WorldItem other)
        {
            if (ReferenceEquals(Data, other.Data)) return 0;
            if (Data is null) return -1;
            if (other.Data is null) return 1;
            return Data.CompareTo(other.Data);
        }
    }
}