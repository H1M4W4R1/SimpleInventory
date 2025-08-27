namespace Systems.SimpleInventory.Data.Enums
{
    public enum EquipItemResult
    {
        InvalidItem,
        
        CannotBeEquipped,
        AlreadyEquipped,
        EquippedSuccessfully,
        NoFreeSlots,
        
        CannotBeUnequipped,
        UnequippedSuccessfully,
        NoSpaceInInventory,
        NotEquipped
       
    }
}