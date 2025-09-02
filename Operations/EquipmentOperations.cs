using Systems.SimpleCore.Operations;

namespace Systems.SimpleInventory.Operations
{
    public static class EquipmentOperations
    {
        public const ushort SYSTEM_EQUIPMENT = 0x0004;
        
        public const int ERROR_ALREADY_EQUIPPED = 1;
        public const int ERROR_NOT_EQUIPPED = 2;
        public const int ERROR_NO_FREE_SLOTS = 3;
        public const int ERROR_INVENTORY_NOT_CREATED = 4;
        public const int ERROR_INVALID_AMOUNT = 5;

        public const int SUCCESS_EQUIPPED = 1;
        public const int SUCCESS_UNEQUIPPED = 2;
        
        public static OperationResult Permitted() => OperationResult.Success(SYSTEM_EQUIPMENT, OperationResult.SUCCESS_PERMITTED);
        
        public static OperationResult AlreadyEquipped() => OperationResult.Error(SYSTEM_EQUIPMENT, ERROR_ALREADY_EQUIPPED);
        public static OperationResult NotEquipped() => OperationResult.Error(SYSTEM_EQUIPMENT, ERROR_NOT_EQUIPPED);
        public static OperationResult NoFreeSlots() => OperationResult.Error(SYSTEM_EQUIPMENT, ERROR_NO_FREE_SLOTS);

        public static OperationResult Equipped() => OperationResult.Success(SYSTEM_EQUIPMENT, SUCCESS_EQUIPPED);
        public static OperationResult Unequipped() => OperationResult.Success(SYSTEM_EQUIPMENT, SUCCESS_UNEQUIPPED);
    }
}