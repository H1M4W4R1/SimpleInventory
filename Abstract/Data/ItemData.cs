using System;

namespace Systems.SimpleInventory.Abstract.Data
{
    /// <summary>
    ///     Object used to store item data, to be extended to collect
    ///     data for specific item types.
    /// </summary>
    [Serializable]
    public abstract class ItemData : IComparable<ItemData>
    {
        /// <summary>
        ///     Method used to compare item data to determine which one is better.
        ///     If result is ambiguous, returns 0.
        /// </summary>
        /// <param name="other">Other item data to compare to</param>
        public virtual int CompareTo(ItemData other) => 0;
    }
}