using System;
using JetBrains.Annotations;
using Systems.SimpleInventory.Abstract.Data;
using Systems.SimpleInventory.Abstract.Items;
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
        /// <remarks>
        ///     To operate properly both items should have their data set and
        ///     item data should be of same type.
        ///     <br/><br/>
        ///     Of course, it's possible to implement custom data comparison between different
        ///     data types, but it's not recommended.
        ///     <br/><br/>
        ///     Failsafe for null-data is to consider it as worse item. If both items have null-data,
        ///     they're considered equal.
        /// </remarks>
        public int CompareTo([NotNull] WorldItem other)
        {
            if (ReferenceEquals(Data, other.Data)) return 0;
            if (Data is null) return -1;
            if (other.Data is null) return 1;
            return Data.CompareTo(other.Data);
        }
    }
}