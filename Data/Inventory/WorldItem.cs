using System;
using JetBrains.Annotations;
using Systems.SimpleInventory.Data.Items.Abstract;
using UnityEngine;

namespace Systems.SimpleInventory.Data.Inventory
{
    
    /// <summary>
    ///     Represents item in world space
    /// </summary>
    [Serializable]
    public sealed class WorldItem
    {
        /// <summary>
        ///     Base item of this world item
        /// </summary>
        [NotNull] [field: SerializeReference] public ItemBase Item { get; private set; }
        
        // TODO: Item data :)
        
        public int MaxStack => Item.MaxStack;

        // Block constructor
        internal WorldItem([NotNull] ItemBase item)
        {
            Item = item;
        }
    }
}