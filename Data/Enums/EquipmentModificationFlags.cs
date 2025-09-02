using System;

namespace Systems.SimpleInventory.Data.Enums
{
    [Flags]
    public enum EquipmentModificationFlags
    {
        None = 0,
        IgnoreConditions = 1 << 0,
        
    }
}