using System;
using Systems.SimpleInventory.Data.Items.Base;

namespace Systems.SimpleInventory.Data.Equipment
{
    /// <summary>
    ///     Default slot implementation
    /// </summary>
    [Serializable] public sealed class EquipmentSlot<TItemType> : EquipmentSlotBase<TItemType>
        where TItemType : EquippableItemBase
    {
    }
}