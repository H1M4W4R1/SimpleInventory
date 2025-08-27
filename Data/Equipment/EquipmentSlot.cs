using System;
using JetBrains.Annotations;
using Systems.SimpleInventory.Data.Items;
using UnityEngine;
using UnityEngine.Assertions;

namespace Systems.SimpleInventory.Data.Equipment
{
    [Serializable]
    internal sealed class EquipmentSlot<TItemType> : EquipmentSlot
        where TItemType : EquippableItemBase
    {
        /// <summary>
        ///     Cached currently equipped item
        /// </summary>
        [SerializeReference] [HideInInspector] private TItemType _currentlyEquippedItem;

        public override EquippableItemBase CurrentlyEquippedItem => _currentlyEquippedItem;

        /// <summary>
        ///     Checks if the item is valid for this slot
        /// </summary>
        public override bool IsItemValid(ItemBase item) => item is TItemType;

        /// <summary>
        ///     Equips the item in this slot
        /// </summary>
        /// <param name="item">Item to equip</param>
        internal override void EquipItem(ItemBase item)
        {
            Assert.IsTrue(IsItemValid(item), "Item is not valid for this slot");
            _currentlyEquippedItem = item as TItemType;
        }

        /// <summary>
        ///     Unequips the item in this slot
        /// </summary>
        /// <returns>True if item was unequipped, false otherwise</returns>
        internal override bool UnequipItem()
        {
            if (_currentlyEquippedItem is null) return false;
            _currentlyEquippedItem = null;
            return true;
        }
    }

    /// <summary>
    ///     Equipment slot
    /// </summary>
    [Serializable]
    internal abstract class EquipmentSlot
    {
        /// <summary>
        ///     Equips the item in this slot
        /// </summary>
        /// <param name="item">Item to equip</param>
        internal abstract void EquipItem([CanBeNull] ItemBase item);

        /// <summary>
        ///     Unequips the item in this slot
        /// </summary>
        /// <returns>True if item was unequipped, false otherwise</returns>
        internal abstract bool UnequipItem();

        /// <summary>
        ///     Checks if the item is valid for this slot
        /// </summary>
        /// <param name="item">Item to check</param>
        /// <returns>True if item is valid for this slot, false otherwise</returns>
        public abstract bool IsItemValid([CanBeNull] ItemBase item);

        /// <summary>
        ///     Item currently equipped in this slot
        /// </summary>
        public abstract EquippableItemBase CurrentlyEquippedItem { get; }
    }
}