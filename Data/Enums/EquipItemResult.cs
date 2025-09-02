namespace Systems.SimpleInventory.Data.Enums
{
    public enum EquipItemResult
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
        ///     When item cannot be equipped
        /// </summary>
        NotAllowed,
        
        /// <summary>
        ///     When item is already equipped
        /// </summary>
        AlreadyEquipped,
        
        /// <summary>
        ///     When there are no free slots to equip item to
        /// </summary>
        NoFreeSlots,
        
        /// <summary>
        ///     When item is equipped successfully
        /// </summary>
        EquippedSuccessfully,
    }
}