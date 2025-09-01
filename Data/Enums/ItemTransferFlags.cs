using System;

namespace Systems.SimpleInventory.Data.Enums
{
    [Flags]
    public enum ItemTransferFlags
    {
        None = 0,
        
        /// <summary>
        ///     Items will be swapped when occupied by other item
        /// </summary>
        SwapIfOccupiedByAnother = 1 << 0,
        
        /// <summary>
        ///     Items will be swapped when occupied by same item
        ///     Overrides <see cref="AllowPartialTransfer"/>
        /// </summary>
        SwapIfOccupiedBySame = 1 << 1,
        
        /// <summary>
        ///     When occupied by same item partial amount will be transferred
        /// </summary>
        AllowPartialTransfer = 1 << 2,
    }
}