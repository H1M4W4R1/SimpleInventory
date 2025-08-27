namespace Systems.SimpleInventory.Data.Enums
{
    public enum EquipItemResult
    {
        InvalidItem,
        
        CannotBeEquipped,
        AlreadyEquipped,
        EquippedSuccessfully,
        
        CannotBeUnequipped,
        AlreadyUnequipped,
        UnequippedSuccessfully
    }
}