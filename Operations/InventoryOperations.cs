using Systems.SimpleCore.Operations;

namespace Systems.SimpleInventory.Operations
{
    public static class InventoryOperations
    {
        public const ushort SYSTEM_INVENTORY = 0x0005;

        public const ushort ERROR_INVENTORY_NOT_CREATED = 1;
        public const ushort ERROR_INVALID_AMOUNT = 2;
        public const ushort ERROR_INVALID_SLOT_INDEX = 3;
        public const ushort ERROR_SLOT_IS_EMPTY = 4;
        public const ushort ERROR_ITEM_NOT_FOUND = 5;
        public const ushort ERROR_NOT_ENOUGH_SPACE = 6;
        public const ushort ERROR_NOT_ENOUGH_ITEMS = 7;

        public const ushort ERROR_ITEM_IS_NULL = 9;
        public const ushort ERROR_ITEM_NOT_USABLE = 10;
        public const ushort ERROR_ITEM_NOT_EQUIPPABLE = 11;

        public const ushort SUCCESS_ITEMS_ADDED = 1;
        public const ushort SUCCESS_ITEMS_TAKEN = 2;
        public const ushort SUCCESS_ITEMS_TRANSFERRED = 3;
        public const ushort SUCCESS_ITEMS_DROPPED = 4;
        public const ushort SUCCESS_ITEMS_PICKED_UP = 5;
        public const ushort SUCCESS_ITEMS_USED = 6;

        public static OperationResult Permitted()
            => OperationResult.Success(SYSTEM_INVENTORY, OperationResult.SUCCESS_PERMITTED);

        public static OperationResult NotEnoughSpace()
            => OperationResult.Error(SYSTEM_INVENTORY, ERROR_NOT_ENOUGH_SPACE);

        public static OperationResult NotEnoughItems()
            => OperationResult.Error(SYSTEM_INVENTORY, ERROR_NOT_ENOUGH_ITEMS);

    
        public static OperationResult ItemIsNull() => OperationResult.Error(SYSTEM_INVENTORY, ERROR_ITEM_IS_NULL);

        public static OperationResult InvalidSlotIndex()
            => OperationResult.Error(SYSTEM_INVENTORY, ERROR_INVALID_SLOT_INDEX);

        public static OperationResult SlotIsEmpty()
            => OperationResult.Error(SYSTEM_INVENTORY, ERROR_SLOT_IS_EMPTY);

        public static OperationResult ItemNotFound()
            => OperationResult.Error(SYSTEM_INVENTORY, ERROR_ITEM_NOT_FOUND);

        public static OperationResult InventoryNotCreated()
            => OperationResult.Error(SYSTEM_INVENTORY, ERROR_INVENTORY_NOT_CREATED);

        public static OperationResult InvalidAmount()
            => OperationResult.Error(SYSTEM_INVENTORY, ERROR_INVALID_AMOUNT);

        
        public static OperationResult ItemNotEquippable()
            => OperationResult.Error(SYSTEM_INVENTORY, ERROR_ITEM_NOT_EQUIPPABLE);
        
        public static OperationResult ItemNotUsable()
            => OperationResult.Error(SYSTEM_INVENTORY, ERROR_ITEM_NOT_USABLE);
        
        public static OperationResult ItemsAdded()
            => OperationResult.Success(SYSTEM_INVENTORY, SUCCESS_ITEMS_ADDED);

        public static OperationResult ItemsTaken()
            => OperationResult.Success(SYSTEM_INVENTORY, SUCCESS_ITEMS_TAKEN);

        public static OperationResult ItemsTransferred()
            => OperationResult.Success(SYSTEM_INVENTORY, SUCCESS_ITEMS_TRANSFERRED);

        public static OperationResult ItemsDropped()
            => OperationResult.Success(SYSTEM_INVENTORY, SUCCESS_ITEMS_DROPPED);

        public static OperationResult ItemsPickedUp()
            => OperationResult.Success(SYSTEM_INVENTORY, SUCCESS_ITEMS_PICKED_UP);

        public static OperationResult UsedSuccessfully()
            => OperationResult.Success(SYSTEM_INVENTORY, SUCCESS_ITEMS_USED);
        
    }
}