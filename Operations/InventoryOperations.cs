using Systems.SimpleCore.Operations;

namespace Systems.SimpleInventory.Operations
{
    public static class InventoryOperations
    {
        public const int NOT_ENOUGH_SPACE = 1;
        public const int NOT_ENOUGH_ITEMS = 2;
        public const int NOT_ALLOWED_TO_SWAP = 3;
        public const int INVALID_ITEM_TYPE = 4;
        public const int ITEM_IS_NULL = 5;
        public const int INVALID_SLOT_INDEX = 6;
        public const int SLOT_IS_EMPTY = 7;
        public const int ITEM_NOT_FOUND = 8;

        public const int ALREADY_EQUIPPED = 9;
        public const int NOT_EQUIPPED = 10;
        public const int CANNOT_EQUIP = 11;
        public const int CANNOT_UNEQUIP = 12;
        public const int NO_FREE_SLOTS = 13;
        public const int INVENTORY_NOT_CREATED = 14;
        public const int INVALID_AMOUNT = 15;

        public static OperationResult NotEnoughSpace() => new(NOT_ENOUGH_SPACE);
        public static OperationResult NotEnoughItems() => new(NOT_ENOUGH_ITEMS);
        public static OperationResult InvalidItemType() => new(INVALID_ITEM_TYPE);
        public static OperationResult ItemIsNull() => new(ITEM_IS_NULL);
        public static OperationResult InvalidSlotIndex() => new(INVALID_SLOT_INDEX);
        public static OperationResult SlotIsEmpty() => new(SLOT_IS_EMPTY);
        public static OperationResult ItemNotFound() => new(SLOT_IS_EMPTY);
        public static OperationResult InventoryNotCreated() => new(INVENTORY_NOT_CREATED);
        public static OperationResult InvalidAmount() => new(INVALID_AMOUNT);
        
        public static OperationResult ItemsAdded() => OperationResult.GenericSuccess;
        public static OperationResult ItemsTaken() => OperationResult.GenericSuccess;
        public static OperationResult ItemsTransferred() => OperationResult.GenericSuccess;
        public static OperationResult ItemsDropped() => OperationResult.GenericSuccess;
        
        public static OperationResult Permitted() => OperationResult.GenericSuccess;
#region Equipment

        public static OperationResult AlreadyEquipped() => new(ALREADY_EQUIPPED);
        public static OperationResult NotEquipped() => new(NOT_EQUIPPED);
        public static OperationResult CannotEquip() => new(CANNOT_EQUIP);
        public static OperationResult CannotUnequip() => new(CANNOT_UNEQUIP);
        public static OperationResult NoFreeSlots() => new(NO_FREE_SLOTS);

        public static OperationResult Equipped() => OperationResult.GenericSuccess;
        public static OperationResult Unequipped() => OperationResult.GenericSuccess;

#endregion

#region Usables

        public static OperationResult UsedSuccessfully() => OperationResult.GenericSuccess;

#endregion
    }
}