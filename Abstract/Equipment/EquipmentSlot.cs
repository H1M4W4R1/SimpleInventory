using System;
using Systems.SimpleInventory.Abstract.Items;

namespace Systems.SimpleInventory.Abstract.Equipment
{
    /// <summary>
    ///     Default slot implementation
    /// </summary>
    [Serializable] public sealed class EquipmentSlot<TItemType> : EquipmentSlotBase<TItemType>
        where TItemType : EquippableItemBase
    {
    }
}