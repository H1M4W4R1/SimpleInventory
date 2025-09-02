using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Systems.SimpleInventory.Components.Inventory;
using Systems.SimpleInventory.Components.Items.Pickup;
using Systems.SimpleInventory.Data.Context;
using Systems.SimpleInventory.Data.Enums;
using Systems.SimpleInventory.Data.Equipment;
using Systems.SimpleInventory.Data.Inventory;
using Systems.SimpleInventory.Data.Items.Base;
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

        [field: SerializeField] [Tooltip("Position to drop item at when removing slot with drop enabled")]
        private Transform DropPositionFallback { get; set; }

        // ReSharper disable once CollectionNeverUpdated.Global
        internal readonly List<EquipmentSlot> equipmentSlots = new();

        private void Awake()
        {
            if (_areEquipmentSlotsBuilt) return;
            BuildEquipmentSlots();
            _areEquipmentSlotsBuilt = true;
        }

#region Equipment Slots

        /// <summary>
        ///     Must be called to build equipment slots
        /// </summary>
        /// <remarks>
        ///     You should add <see cref="EquipmentSlot{TItemType}"/> to <see cref="equipmentSlots"/> using
        ///     <see cref="AddEquipmentSlotFor{TItemType}"/> method.
        ///     Clear list before adding any equipment slots to support multiple calls to this method.
        /// </remarks>
        protected abstract void BuildEquipmentSlots();

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
                if (addItemsToInventory && inventory is not null)
                    inventory.TryAddOrDrop(equipmentSlots[i].CurrentlyEquippedItem, 1);
                else if (addItemsToInventory)
                {
                    Transform objTransform = ReferenceEquals(DropPositionFallback, null)
                        ? transform
                        : DropPositionFallback;
                    ItemBase.DropItem<PickupItemWithDestroy>(equipmentSlots[i].CurrentlyEquippedItem,
                        1, objTransform.position, objTransform.rotation);
                }

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
                if (!equipmentSlots[i].IsItemValid<TItemType>()) continue;
                equipmentSlots.RemoveAt(i);
                return;
            }

            // Remove slots with items
            for (int i = equipmentSlots.Count - 1; i >= 0; i--)
            {
                if (!equipmentSlots[i].IsItemValid<TItemType>()) continue;

                // Add item to inventory before removing slot
                if (addItemToInventory && inventory is not null)
                    inventory.TryAddOrDrop(equipmentSlots[i].CurrentlyEquippedItem, 1);
                else if (addItemToInventory)
                {
                    Transform objTransform = ReferenceEquals(DropPositionFallback, null)
                        ? transform
                        : DropPositionFallback;
                    ItemBase.DropItem<PickupItemWithDestroy>(equipmentSlots[i].CurrentlyEquippedItem,
                        1, objTransform.position, objTransform.rotation);
                }

                equipmentSlots.RemoveAt(i);
                return;
            }
        }


        /// <summary>
        ///     Gets first free slot for item.
        /// </summary>
        /// <param name="forItem">Item to find slot for</param>
        /// <returns>First free slot or null if no free slot is found</returns>
        [CanBeNull] internal EquipmentSlot GetFreeSlot([NotNull] WorldItem forItem)
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
        [CanBeNull] internal EquipmentSlot GetFreeOrSwapSlot([NotNull] WorldItem forItem)
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
        [CanBeNull] internal EquipmentSlot GetFirstEquippedSlot([NotNull] WorldItem forItem)
        {
            for (int i = 0; i < equipmentSlots.Count; i++)
            {
                EquipmentSlot slot = equipmentSlots[i];
                if (ReferenceEquals(slot.CurrentlyEquippedItem, null)) continue;
                if (!Equals(slot.CurrentlyEquippedItem, forItem)) continue;
                return slot;
            }

            return null;
        }

        /// <summary>
        ///     Gets first equipped slot for item.
        /// </summary>
        /// <param name="forItem">Item tp find</param>
        /// <returns>First equipped slot or null if no slot is equipped</returns>
        [CanBeNull] internal EquipmentSlot GetFirstEquippedSlot([NotNull] ItemBase forItem)
        {
            for (int i = 0; i < equipmentSlots.Count; i++)
            {
                EquipmentSlot slot = equipmentSlots[i];
                if (ReferenceEquals(slot.CurrentlyEquippedItem, null)) continue;
                if (!ReferenceEquals(slot.CurrentlyEquippedItem.Item, forItem)) continue;
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
                if (ReferenceEquals(slot.CurrentlyEquippedItem, null)) continue;
                if (slot.CurrentlyEquippedItem.Item is not TItemType) continue;
                return slot;
            }

            return null;
        }

#endregion

#region Item access

        /// <summary>
        ///     Gets first equipped item for specific item type
        /// </summary>
        /// <typeparam name="TItemBase">Base type of item used to create slot</typeparam>
        /// <returns>First equipped item or null if no item is equipped</returns>
        [CanBeNull] public TItemBase GetFirstEquippedBaseItemFor<TItemBase>()
            where TItemBase : EquippableItemBase
        {
            EquipmentSlot slot = GetFirstEquippedSlot<TItemBase>();
            if (ReferenceEquals(slot, null)) return null;
            if (ReferenceEquals(slot.CurrentlyEquippedItem, null)) return null;
            return slot.CurrentlyEquippedItem.Item as TItemBase;
        }

        /// <summary>
        ///     Gets first equipped item for specific item type
        /// </summary>
        /// <typeparam name="TItemBase">Base type of item used to create slot</typeparam>
        /// <returns>First equipped item or null if no item is equipped</returns>
        [CanBeNull] public WorldItem GetFirstEquippedItemFor<TItemBase>()
            where TItemBase : EquippableItemBase
        {
            EquipmentSlot slot = GetFirstEquippedSlot<TItemBase>();
            return slot?.CurrentlyEquippedItem;
        }

        /// <summary>
        ///     Gets all equipped items for specific item type
        /// </summary>
        /// <typeparam name="TItemBase">Base type of item used to create slot</typeparam>
        /// <returns>List of equipped items</returns>
        [NotNull] public IReadOnlyList<TItemBase> GetAllEquippedItemsFor<TItemBase>()
            where TItemBase : EquippableItemBase
        {
            List<TItemBase> items = new();

            for (int i = 0; i < equipmentSlots.Count; i++)
            {
                EquipmentSlot slot = equipmentSlots[i];
                if (slot.IsItemValid<TItemBase>()) continue;
                if (ReferenceEquals(slot.CurrentlyEquippedItem, null)) continue;
                if (slot.CurrentlyEquippedItem.Item is not TItemBase item) continue;
                items.Add(item);
            }

            return items;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool IsEquipped([NotNull] in WorldItem item) =>
            GetFirstEquippedSlot(item) != null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEquipped([NotNull] in EquippableItemBase item) =>
            GetFirstEquippedSlot(item) != null;

        /// <summary>
        ///     Checks if item is equipped.
        /// </summary>
        /// <param name="context">Action context</param>
        /// <returns>True if item is equipped</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool IsEquipped(in EquipItemContext context)
            => IsEquipped(context.item);

        /// <summary>
        ///     Checks if item is equipped.
        /// </summary>
        /// <param name="context">Action context</param>
        /// <returns>True if item is equipped</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool IsEquipped(in UnequipItemContext context)
            => IsEquipped(context.item);

#endregion

#region Equip and Unequip items

        /// <summary>
        ///     Equips an item.
        /// </summary>
        /// <param name="context">Context of action</param>
        /// <param name="flags">Flags for modifying action</param>
        /// <returns>Result of action</returns>
        internal EquipItemResult Equip(
            in EquipItemContext context,
            EquipmentModificationFlags flags =
                EquipmentModificationFlags.None)
        {
            EquippableItemBase equippableItemRef = context.itemBase;
            
            // Check if already equipped
            if (equippableItemRef.IsEquipped(context))
            {
                OnItemAlreadyEquipped(context);
                return EquipItemResult.AlreadyEquipped;
            }

            // Check if item can be equipped
            if (!CanEquip(context) && (flags & EquipmentModificationFlags.IgnoreConditions) == 0)
            {
                OnItemCannotBeEquipped(context);
                return EquipItemResult.NotAllowed;
            }

            // Find first empty slot we can equip item to
            EquipmentSlot slot = context.allowReplace
                ? GetFreeOrSwapSlot(context.item)
                : GetFreeSlot(context.item);
            if (slot == null) return EquipItemResult.NoFreeSlots;

            // Sanity check for same item
            if (ReferenceEquals(slot.CurrentlyEquippedItem, context.item)) return EquipItemResult.AlreadyEquipped;

            // Unequip item if was already equipped
            if (!ReferenceEquals(slot.CurrentlyEquippedItem, null))
            {
                if (context.slot.inventory is not null)
                    context.slot.inventory.UnequipItem(
                        slot.CurrentlyEquippedItem, this);
                else
                {
                    Transform objTransform = ReferenceEquals(DropPositionFallback, null)
                        ? transform
                        : DropPositionFallback;
                    ItemBase.DropItem<PickupItemWithDestroy>(slot.CurrentlyEquippedItem,
                        1, objTransform.position, objTransform.rotation);
                }
            }

            // Equip item to slot
            slot.EquipItem(context.item);

            // Take item from inventory silently (if needed)
            if (context.removeFromInventory && context.slot.inventory is not null)
                context.slot.inventory.Take(context.slot.slotIndex, InventoryActionSource.Internal);

            // Call events
            OnItemEquipped(context);
            return EquipItemResult.EquippedSuccessfully;
        }

        /// <summary>
        ///     Unequips an item.
        /// </summary>
        /// <param name="context">Context of action</param>
        /// <param name="flags">Flags for modifying action</param>
        /// <returns>Result of action</returns>
        internal UnequipItemResult Unequip(in UnequipItemContext context,
            EquipmentModificationFlags flags =
                EquipmentModificationFlags.None)
        {
            EquippableItemBase equippableItemRef = context.itemBase;

            // Check if already unequipped
            if (!equippableItemRef.IsEquipped(context))
            {
                OnItemAlreadyUnequipped(context);
                return UnequipItemResult.NotEquipped;
            }

            // Check if item can be unequipped
            if (!CanUnequip(context) && (flags & EquipmentModificationFlags.IgnoreConditions) == 0)
            {
                OnItemCannotBeUnequipped(context);
                return UnequipItemResult.NotAllowed;
            }

            // Get item to unequip
            EquipmentSlot slot = GetFirstEquippedSlot(context.item);
            if (slot == null) return UnequipItemResult.NotEquipped;

            // Add item to inventory if needed
            if (context.addToInventory && context.inventory is not null)
                context.inventory.TryAddOrDrop(context.item, 1, InventoryActionSource.Internal);
            else if (context.addToInventory)
            {
                Transform objTransform = ReferenceEquals(DropPositionFallback, null)
                    ? transform
                    : DropPositionFallback;
                ItemBase.DropItem<PickupItemWithDestroy>(slot.CurrentlyEquippedItem,
                    1, objTransform.position, objTransform.rotation);
            }

            // Unequip item
            Assert.IsTrue(slot.UnequipItem(),
                "Something went wrong while unequipping item, this should never happen");

            // Call events
            OnItemUnequipped(context);
            return UnequipItemResult.UnequippedSuccessfully;
        }

#endregion

#region Checks

        /// <summary>
        ///     Check if item can be equipped
        /// </summary>
        /// <param name="context">Context of action</param>
        /// <returns>True if item can be equipped</returns>
        public virtual bool CanEquip(in EquipItemContext context) =>
            context.itemBase.CanEquip(context);

        /// <summary>
        ///     Check if item can be unequipped
        /// </summary>
        /// <param name="context">Context of action</param>
        /// <returns>True if item can be unequipped</returns>
        public virtual bool CanUnequip(in UnequipItemContext context) =>
            context.itemBase.CanUnequip(context);

#endregion

#region Events

        protected virtual void OnItemEquipped(in EquipItemContext context)
        {
            context.itemBase.OnEquip(context);
        }

        protected virtual void OnItemUnequipped(in UnequipItemContext context)
        {
            context.itemBase.OnUnequip(context);
        }

        protected virtual void OnItemAlreadyEquipped(in EquipItemContext context)
        {
            context.itemBase.OnAlreadyEquipped(context);
        }

        protected virtual void OnItemAlreadyUnequipped(in UnequipItemContext context)
        {
            context.itemBase.OnAlreadyUnequipped(context);
        }

        protected virtual void OnItemCannotBeEquipped(in EquipItemContext context)
        {
            context.itemBase.OnCannotBeEquipped(context);
        }

        protected virtual void OnItemCannotBeUnequipped(in UnequipItemContext context)
        {
            context.itemBase.OnCannotBeUnequipped(context);
        }

#endregion
    }
}