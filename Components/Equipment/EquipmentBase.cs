using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Systems.SimpleInventory.Components.Inventory;
using Systems.SimpleInventory.Data.Context;
using Systems.SimpleInventory.Data.Enums;
using Systems.SimpleInventory.Data.Equipment;
using Systems.SimpleInventory.Data.Items;
using UnityEngine;
using UnityEngine.Assertions;

namespace Systems.SimpleInventory.Components.Equipment
{
    /// <summary>
    ///     Equipment that can equip/unequip items.
    /// </summary>
    public abstract class EquipmentBase : MonoBehaviour
    {
        private bool _areEquipmentSlotsBuilt;

        // ReSharper disable once CollectionNeverUpdated.Global
        internal readonly List<EquipmentSlot> equipmentSlots = new();

        // TODO: Save/Load from data.

        /// <summary>
        ///     Must be called to build equipment slots
        /// </summary>
        /// <remarks>
        ///     You should add <see cref="EquipmentSlot{TItemType}"/> to <see cref="equipmentSlots"/> using
        ///     <see cref="AddEquipmentSlotFor{TItemType}"/> method.
        ///     Clear list before adding any equipment slots to support multiple calls to this method.
        /// </remarks>
        public abstract void BuildEquipmentSlots();

        /// <summary>
        ///     Adds equipment slot for specific item type.
        /// </summary>
        /// <typeparam name="TItemType">Item type</typeparam>
        protected void AddEquipmentSlotFor<TItemType>()
            where TItemType : EquippableItemBase => equipmentSlots.Add(new EquipmentSlot<TItemType>());

        /// <summary>
        ///     Removes all equipment slots (does not recover items)
        /// </summary>
        protected void ClearEquipmentSlots(
            [CanBeNull] InventoryBase inventory = null,
            bool addItemsToInventory = true)
        {
            for (int i = equipmentSlots.Count - 1; i >= 0; i--)
            {
                // Add item to inventory before removing slot
                if (addItemsToInventory && inventory is not null) // TODO: Drop if inventory null?
                    inventory.TryAddDrop(equipmentSlots[i].CurrentlyEquippedItem, 1);

                // Remove slot
                equipmentSlots.RemoveAt(i);
            }
        }

        /// <summary>
        ///     Removes equipment slot for specific item type
        /// </summary>
        /// <param name="inventory">Inventory to add item to</param>
        /// <param name="addItemToInventory">If true, item will be added to inventory before removing slot</param>
        /// <typeparam name="TItemType">Item type</typeparam>
        protected void RemoveEquipmentSlotFor<TItemType>(
            [CanBeNull] InventoryBase inventory,
            bool addItemToInventory = true)
            where TItemType : EquippableItemBase
        {
            // Remove slots that are empty
            for (int i = equipmentSlots.Count - 1; i >= 0; i--)
            {
                if (equipmentSlots[i] is not EquipmentSlot<TItemType>) continue;
                equipmentSlots.RemoveAt(i);
                return;
            }

            // Remove slots with items
            for (int i = equipmentSlots.Count - 1; i >= 0; i--)
            {
                if (equipmentSlots[i] is not EquipmentSlot<TItemType>) continue;

                // Add item to inventory before removing slot
                if (addItemToInventory && inventory is not null) // TODO: Drop if inventory null?
                    inventory.TryAddDrop(equipmentSlots[i].CurrentlyEquippedItem, 1);

                equipmentSlots.RemoveAt(i);
                return;
            }
        }

        private void Awake()
        {
            if (_areEquipmentSlotsBuilt) return;
            BuildEquipmentSlots();
            _areEquipmentSlotsBuilt = true;
        }

        /// <summary>
        ///     Check if item can be unequipped
        /// </summary>
        /// <param name="context">Context of action</param>
        /// <returns>True if item can be unequipped</returns>
        public virtual bool CanUnequip(in UnequipItemContext context) => true;

        /// <summary>
        ///     Check if item can be equipped
        /// </summary>
        /// <param name="context">Context of action</param>
        /// <returns>True if item can be equipped</returns>
        public virtual bool CanEquip(in EquipItemContext context) => true;

        /// <summary>
        ///     Equips an item.
        /// </summary>
        /// <param name="context">Context of action</param>
        /// <returns>Result of action</returns>
        internal EquipItemResult Equip(in EquipItemContext context)
        {
            // Check if already equipped
            if (context.item.IsEquipped(context))
            {
                OnItemAlreadyEquipped(context);
                return EquipItemResult.AlreadyEquipped;
            }

            // Check if item can be equipped
            if (!context.item.CanEquip(context) || !CanEquip(context))
            {
                OnItemCannotBeEquipped(context);
                return EquipItemResult.NotAllowed;
            }

            // Find first empty slot we can equip item to
            EquipmentSlot slot = context.allowReplace
                ? GetFreeOrSwapSlot(context.item)
                : GetFreeSlot(context.item);
            if (slot == null) return EquipItemResult.NoFreeSlots;

            // Equip item to slot
            slot.EquipItem(context.item);

            // Take item from inventory
            if (context.removeFromInventory) context.slot.inventory.ClearSlot(context.slot.slotIndex);

            // Call events
            context.item.OnEquip(context);
            OnItemEquipped(context);

            return EquipItemResult.EquippedSuccessfully;
        }

        /// <summary>
        ///     Unequips an item.
        /// </summary>
        /// <param name="context">Context of action</param>
        /// <returns>Result of action</returns>
        internal UnequipItemResult Unequip(in UnequipItemContext context)
        {
            // Check if already unequipped
            if (!context.item.IsEquipped(context))
            {
                OnItemAlreadyUnequipped(context);
                return UnequipItemResult.NotEquipped;
            }

            // Check if item can be unequipped
            if (!context.item.CanUnequip(context) || !CanUnequip(context))
            {
                OnItemCannotBeUnequipped(context);
                return UnequipItemResult.NotAllowed;
            }

            // Get item to unequip
            EquipmentSlot slot = GetFirstEquippedSlot(context.item);
            if (slot == null) return UnequipItemResult.NotEquipped;

            // Add item to inventory if needed
            if (context.addToInventory)
            {
                // Check if inventory can store item
                if (!context.inventory.CanStore(context.item, 1)) return UnequipItemResult.NoSpaceInInventory;

                Assert.AreEqual(0, context.inventory.TryAdd(context.item, 1),
                    "Something went wrong while adding item to inventory, this should never happen");
            }

            // Unequip item
            Assert.IsTrue(slot.UnequipItem(),
                "Something went wrong while unequipping item, this should never happen");

            // Call events
            OnItemUnequipped(context);
            context.item.OnUnequip(context);

            return UnequipItemResult.UnequippedSuccessfully;
        }

        /// <summary>
        ///     Gets first free slot for item.
        /// </summary>
        /// <param name="forItem">Item to find slot for</param>
        /// <returns>First free slot or null if no free slot is found</returns>
        [CanBeNull] internal EquipmentSlot GetFreeSlot([NotNull] EquippableItemBase forItem)
        {
            for (int i = 0; i < equipmentSlots.Count; i++)
            {
                EquipmentSlot slot = equipmentSlots[i];
                if (ReferenceEquals(slot.CurrentlyEquippedItem, null) && slot.IsItemValid(forItem)) return slot;
            }

            return null;
        }

        /// <summary>
        ///     Gets first free slot for item.
        /// </summary>
        /// <param name="forItem">Item to find slot for</param>
        /// <returns>First free slot or null if no free slot is found</returns>
        /// <remarks>
        ///     Prioritizes free slots over swap slots.
        /// </remarks>
        [CanBeNull] internal EquipmentSlot GetFreeOrSwapSlot([NotNull] EquippableItemBase forItem)
        {
            // Handle free slots
            for (int i = 0; i < equipmentSlots.Count; i++)
            {
                EquipmentSlot slot = equipmentSlots[i];
                if (ReferenceEquals(slot.CurrentlyEquippedItem, null) && slot.IsItemValid(forItem)) return slot;
            }

            // Handle swap slots
            for (int i = 0; i < equipmentSlots.Count; i++)
            {
                EquipmentSlot slot = equipmentSlots[i];
                if (slot.IsItemValid(forItem)) return slot;
            }

            return null;
        }

        /// <summary>
        ///     Gets first equipped slot for item.
        /// </summary>
        /// <param name="forItem">Item tp find</param>
        /// <returns>First equipped slot or null if no slot is equipped</returns>
        [CanBeNull] internal EquipmentSlot GetFirstEquippedSlot([NotNull] EquippableItemBase forItem)
        {
            for (int i = 0; i < equipmentSlots.Count; i++)
            {
                EquipmentSlot slot = equipmentSlots[i];
                if (ReferenceEquals(slot.CurrentlyEquippedItem, null)) continue;
                if (!ReferenceEquals(slot.CurrentlyEquippedItem, forItem)) continue;
                return slot;
            }

            return null;
        }

        /// <summary>
        ///     Gets first equipped slot for item of specified type
        /// </summary>
        /// <typeparam name="TItemType">Item type</typeparam>
        /// <returns>Equipped slot or null if no slot is equipped</returns>
        [CanBeNull] internal EquipmentSlot GetFirstEquippedSlot<TItemType>()
            where TItemType : EquippableItemBase
        {
            for (int i = 0; i < equipmentSlots.Count; i++)
            {
                EquipmentSlot slot = equipmentSlots[i];
                if (slot.CurrentlyEquippedItem is not TItemType) continue;
                return slot;
            }

            return null;
        }

        /// <summary>
        ///     Checks if item is equipped.
        /// </summary>
        /// <param name="context">Action context</param>
        /// <returns>True if item is equipped</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool IsEquipped(in EquipItemContext context)
            => GetFirstEquippedSlot(context.item) != null;

        /// <summary>
        ///     Checks if item is equipped.
        /// </summary>
        /// <param name="context">Action context</param>
        /// <returns>True if item is equipped</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool IsEquipped(in UnequipItemContext context)
            => GetFirstEquippedSlot(context.item) != null;

        protected virtual void OnItemEquipped(in EquipItemContext context)
        {
        }

        protected virtual void OnItemUnequipped(in UnequipItemContext context)
        {
        }

        protected virtual void OnItemAlreadyEquipped(in EquipItemContext context)
        {
        }

        protected virtual void OnItemAlreadyUnequipped(in UnequipItemContext context)
        {
        }

        protected virtual void OnItemCannotBeEquipped(in EquipItemContext context)
        {
        }

        protected virtual void OnItemCannotBeUnequipped(in UnequipItemContext context)
        {
        }
    }
}