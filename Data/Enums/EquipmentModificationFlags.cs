using System;

namespace Systems.SimpleInventory.Data.Enums
{
    [Flags]
    public enum EquipmentModificationFlags
    {
        None = 0,
        
        /// <summary>
        ///     Item equip/unequip conditions will be ignored. Also known as "force equip/unequip".
        /// </summary>
        IgnoreConditions = 1 << 0,
        
        /// <summary>
        ///     Items will be added and removed from inventory when equipping/unequipping is performed.
        /// </summary>
        ModifyInventory = 1 << 1,
        
        /// <summary>
        ///     Operates only when equipping item, not handled during unequip.
        ///     Allows replacing currently equipped item with another one.
        /// </summary>
        AllowItemSwap = 1 << 2,
        
    }
}