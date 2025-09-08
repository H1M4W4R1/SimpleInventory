using System;
using JetBrains.Annotations;
using Systems.SimpleInventory.Abstract.Items;
using Systems.SimpleInventory.Data.Inventory;
using UnityEngine;
using UnityEngine.Assertions;

namespace Systems.SimpleInventory.Abstract.Equipment
{
    public abstract class EquipmentSlotBase<TItemType> : EquipmentSlotBase
        where TItemType : EquippableItemBase
    {
        /// <summary>
        ///     Cached currently equipped item
        /// </summary>
        [SerializeReference] [HideInInspector] private WorldItem currentlyEquippedItem;

        public override WorldItem CurrentlyEquippedItem => currentlyEquippedItem;

        /// <summary>
        ///     Checks if the item is valid for this slot
        /// </summary>
        public override bool IsItemValid(WorldItem item) => item?.Item is TItemType;

        /// <summary>
        ///     Checks if the item is valid for this slot
        /// </summary>
        public override bool IsItemValid<TCheckType>()
        {
            return typeof(TItemType).IsAssignableFrom(typeof(TCheckType));
        }

        /// <summary>
        ///     Equips the item in this slot
        /// </summary>
        /// <param name="item">Item to equip</param>
        internal override void EquipItem(WorldItem item)
        {
            Assert.IsTrue(IsItemValid(item), "Item is not valid for this slot");
            currentlyEquippedItem = item;
        }

        /// <summary>
        ///     Unequips the item in this slot
        /// </summary>
        /// <returns>True if item was unequipped, false otherwise</returns>
        internal override bool UnequipItem()
        {
            if (currentlyEquippedItem is null) return false;
            currentlyEquippedItem = null;
            return true;
        }
    }

    /// <summary>
    ///     Equipment slot
    /// </summary>
    [Serializable] public abstract class EquipmentSlotBase
    {
        /// <summary>
        ///     Equips the item in this slot
        /// </summary>
        /// <param name="item">Item to equip</param>
        internal abstract void EquipItem([CanBeNull] WorldItem item);

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
        public abstract bool IsItemValid([CanBeNull] WorldItem item);

        /// <summary>
        ///     Checks if the item is valid for this slot
        /// </summary>
        /// <typeparam name="TItemType">Type of item to check</typeparam>
        /// <returns>True if item is valid for this slot, false otherwise</returns>
        public abstract bool IsItemValid<TItemType>()
            where TItemType : EquippableItemBase;

        /// <summary>
        ///     Item currently equipped in this slot
        /// </summary>
        public abstract WorldItem CurrentlyEquippedItem { get; }
    }
}