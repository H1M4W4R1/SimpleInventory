namespace Systems.SimpleInventory.Data.Enums
{
    public enum UnequipItemResult
    {
        Unknown,
        
        /// <summary>
        ///     When slot index is invalid
        /// </summary>
        InvalidSlot,
        
        /// <summary>
        ///     When item is invalid (most likely not equippable)
        /// </summary>
        InvalidItem,
        
        /// <summary>
        ///     When item equip is impossible to execute
        /// </summary>
        NotAllowed,
        
        /// <summary>
        ///     When inventory lacks space for item
        /// </summary>
        NoSpaceInInventory,
        
        /// <summary>
        ///     When item is not equipped
        /// </summary>
        NotEquipped,
        
        /// <summary>
        ///     On success
        /// </summary>
        UnequippedSuccessfully,
   
    }
}