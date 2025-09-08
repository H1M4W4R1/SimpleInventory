using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Storage;
using Systems.SimpleCore.Storage.Lists;
using Systems.SimpleCore.Utility.Enums;
using Systems.SimpleInventory.Abstract.Equipment;
using Systems.SimpleInventory.Abstract.Items;
using Systems.SimpleInventory.Components.Inventory;
using Systems.SimpleInventory.Components.Items.Pickup;
using Systems.SimpleInventory.Data.Context;
using Systems.SimpleInventory.Data.Enums;
using Systems.SimpleInventory.Data.Inventory;
using Systems.SimpleInventory.Operations;
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
        internal readonly List<EquipmentSlotBase> equipmentSlots = new();

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
        ///     You should add <see cref="EquipmentSlotBase{TItemType}"/> to <see cref="equipmentSlots"/> using
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
        ///     Adds custom equipment slot.
        /// </summary>
        protected void AddEquipmentSlot(EquipmentSlotBase slot) => equipmentSlots.Add(slot);

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
        ///     Removes equipment slot
        /// </summary>
        /// <param name="slot">Slot to remove</param>
        /// <param name="inventory">Inventory to add item to</param>
        /// <param name="flags">Flags for unequip operation</param>
        protected void RemoveEquipmentSlot(
            [NotNull] EquipmentSlotBase slot,
            [CanBeNull] InventoryBase inventory = null,
            EquipmentModificationFlags flags = EquipmentModificationFlags.ModifyInventory |
                                               EquipmentModificationFlags.IgnoreConditions)
        {
            // Check if slot is empty, if not handle item unequip or drop
            if (slot.CurrentlyEquippedItem is not null)
            {
                if (inventory is not null)
                {
                    // Create context, we must modify inventory to automatically drop item
                    // and ignore conditions to allow unequipping
                    UnequipItemContext context = new(inventory, this,
                        slot.CurrentlyEquippedItem, flags);

                    // Unequip item, will automatically drop item if it can't be added to inventory
                    // This still should trigger unequip events
                    UnequipFromSlot(slot, context);
                }
                else
                {
                    // Drop item
                    Transform objTransform = ReferenceEquals(DropPositionFallback, null)
                        ? transform
                        : DropPositionFallback;
                    ItemBase.DropItem<PickupItemWithDestroy>(slot.CurrentlyEquippedItem,
                        1, objTransform.position, objTransform.rotation);
                }
            }

            // Remove slot
            equipmentSlots.Remove(slot);
        }

        /// <summary>
        ///     Removes FIRST equipment slot of specific type
        /// </summary>
        /// <typeparam name="TSlotType">Type of slot to remove</typeparam>
        protected void RemoveEquipmentSlot<TSlotType>([CanBeNull] InventoryBase inventory = null)
            where TSlotType : EquipmentSlotBase
        {
            // Remove slots that are empty
            for (int i = equipmentSlots.Count - 1; i >= 0; i--)
            {
                EquipmentSlotBase slot = equipmentSlots[i];
                if (slot is not TSlotType) continue;
                if (ReferenceEquals(slot.CurrentlyEquippedItem, null)) continue;
                RemoveEquipmentSlot(slot, inventory);
                return;
            }

            // Remove slots with items
            for (int i = equipmentSlots.Count - 1; i >= 0; i--)
            {
                EquipmentSlotBase slot = equipmentSlots[i];
                if (slot is not TSlotType) continue;
                RemoveEquipmentSlot(slot, inventory);
                return;
            }
        }

        /// <summary>
        ///     Removes ANY FIRST equipment slot for specific item type
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
                EquipmentSlotBase slot = equipmentSlots[i];
                if (!slot.IsItemValid<TItemType>()) continue;
                if (!ReferenceEquals(slot.CurrentlyEquippedItem, null)) continue;
                RemoveEquipmentSlot(slot, inventory);
                return;
            }

            // Remove slots with items
            for (int i = equipmentSlots.Count - 1; i >= 0; i--)
            {
                if (!equipmentSlots[i].IsItemValid<TItemType>()) continue;
                RemoveEquipmentSlot(equipmentSlots[i], inventory);
                return;
            }
        }

        /// <summary>
        ///     Removes ALL equipment slots of specific type
        /// </summary>
        /// <param name="inventory">Inventory to add item to</param>
        /// <typeparam name="TSlotType">Type of slot to remove</typeparam>
        protected void RemoveEquipmentSlots<TSlotType>([CanBeNull] InventoryBase inventory = null)
            where TSlotType : EquipmentSlotBase
        {
            for (int i = equipmentSlots.Count - 1; i >= 0; i--)
            {
                EquipmentSlotBase slot = equipmentSlots[i];
                if (slot is not TSlotType) continue;
                RemoveEquipmentSlot(slot, inventory);
            }
        }

        /// <summary>
        ///     Removes ALL equipment slots for specific item type
        /// </summary>
        /// <param name="inventory">Inventory to add item to</param>
        /// <typeparam name="TItemType">Type of item to remove</typeparam>
        protected void RemoveEquipmentSlotsFor<TItemType>([CanBeNull] InventoryBase inventory = null)
            where TItemType : EquippableItemBase
        {
            for (int i = equipmentSlots.Count - 1; i >= 0; i--)
            {
                if (!equipmentSlots[i].IsItemValid<TItemType>()) continue;
                RemoveEquipmentSlot(equipmentSlots[i], inventory);
            }
        }

        /// <summary>
        ///     Gets first free slot for item.
        /// </summary>
        /// <param name="forItem">Item to find slot for</param>
        /// <returns>First free slot or null if no free slot is found</returns>
        [CanBeNull] internal EquipmentSlotBase GetFreeSlot([NotNull] WorldItem forItem)
        {
            for (int i = 0; i < equipmentSlots.Count; i++)
            {
                EquipmentSlotBase slot = equipmentSlots[i];
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
        [CanBeNull] internal EquipmentSlotBase GetFreeOrSwapSlot([NotNull] WorldItem forItem)
        {
            // Handle free slots
            for (int i = 0; i < equipmentSlots.Count; i++)
            {
                EquipmentSlotBase slot = equipmentSlots[i];
                if (ReferenceEquals(slot.CurrentlyEquippedItem, null) && slot.IsItemValid(forItem)) return slot;
            }

            // Handle swap slots
            for (int i = 0; i < equipmentSlots.Count; i++)
            {
                EquipmentSlotBase slot = equipmentSlots[i];
                if (slot.IsItemValid(forItem)) return slot;
            }

            return null;
        }

        /// <summary>
        ///     Gets first equipped slot for item.
        /// </summary>
        /// <param name="forItem">Item tp find</param>
        /// <returns>First equipped slot or null if no slot is equipped</returns>
        [CanBeNull] internal EquipmentSlotBase GetFirstEquippedSlot([NotNull] WorldItem forItem)
        {
            for (int i = 0; i < equipmentSlots.Count; i++)
            {
                EquipmentSlotBase slot = equipmentSlots[i];
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
        [CanBeNull] internal EquipmentSlotBase GetFirstEquippedSlot([NotNull] ItemBase forItem)
        {
            for (int i = 0; i < equipmentSlots.Count; i++)
            {
                EquipmentSlotBase slot = equipmentSlots[i];
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
        [CanBeNull] internal EquipmentSlotBase GetFirstEquippedSlot<TItemType>()
            where TItemType : EquippableItemBase
        {
            for (int i = 0; i < equipmentSlots.Count; i++)
            {
                EquipmentSlotBase slot = equipmentSlots[i];
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
            EquipmentSlotBase slot = GetFirstEquippedSlot<TItemBase>();
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
            EquipmentSlotBase slot = GetFirstEquippedSlot<TItemBase>();
            return slot?.CurrentlyEquippedItem;
        }

        /// <summary>
        ///     Gets all equipped items for specific item type
        /// </summary>
        /// <typeparam name="TItemBase">Base type of item used to create slot</typeparam>
        /// <returns>List of equipped items</returns>
        public ROListAccess<TItemBase> GetAllEquippedItemsFor<TItemBase>()
            where TItemBase : EquippableItemBase
        {
            RWListAccess<TItemBase> list = RWListAccess<TItemBase>.Create();
            List<TItemBase> items = list.List;

            for (int i = 0; i < equipmentSlots.Count; i++)
            {
                EquipmentSlotBase slot = equipmentSlots[i];
                if (slot.IsItemValid<TItemBase>()) continue;
                if (ReferenceEquals(slot.CurrentlyEquippedItem, null)) continue;
                if (slot.CurrentlyEquippedItem.Item is not TItemBase item) continue;
                items.Add(item);
            }

            return list.ToReadOnly();
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)] internal bool IsEquipped(in EquipItemContext context)
            => IsEquipped(context.item);

        /// <summary>
        ///     Checks if item is equipped.
        /// </summary>
        /// <param name="context">Action context</param>
        /// <returns>True if item is equipped</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] internal bool IsEquipped(in UnequipItemContext context)
            => IsEquipped(context.item);

#endregion

#region Equip and Unequip items

        /// <summary>
        ///     Equips an item.
        /// </summary>
        /// <param name="context">Context of action</param>
        /// <param name="actionSource">Action source</param>
        /// <returns>Result of action</returns>
        internal OperationResult Equip(
            in EquipItemContext context,
            ActionSource actionSource = ActionSource.External)
        {
            EquippableItemBase equippableItemRef = context.itemBase;

            // Check if already equipped
            if (equippableItemRef.IsEquipped(context))
            {
                if (actionSource == ActionSource.Internal) return EquipmentOperations.AlreadyEquipped();
                OnItemEquipWhenAlreadyEquipped(context, EquipmentOperations.AlreadyEquipped());
                return EquipmentOperations.AlreadyEquipped();
            }

            // Check if item can be equipped
            OperationResult canEquipResult = CanEquip(context);
            if (!canEquipResult && (context.flags & EquipmentModificationFlags.IgnoreConditions) == 0)
            {
                if (actionSource == ActionSource.Internal) return canEquipResult;
                OnItemEquipWhenCannotBeEquipped(context, canEquipResult);
                return canEquipResult;
            }

            // Find first empty slot we can equip item to
            EquipmentSlotBase slot = (context.flags & EquipmentModificationFlags.AllowItemSwap) != 0
                ? GetFreeOrSwapSlot(context.item)
                : GetFreeSlot(context.item);
            if (slot == null)
            {
                if (actionSource == ActionSource.Internal) return EquipmentOperations.NoFreeSlots();
                OnItemEquipWhenCannotBeEquipped(context, EquipmentOperations.NoFreeSlots());
                return EquipmentOperations.NoFreeSlots();
            }

            // Sanity check for same item
            if (ReferenceEquals(slot.CurrentlyEquippedItem, context.item))
            {
                if (actionSource == ActionSource.Internal) return EquipmentOperations.AlreadyEquipped();
                OnItemEquipWhenAlreadyEquipped(context, EquipmentOperations.AlreadyEquipped());
                return EquipmentOperations.AlreadyEquipped();
            }

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
            if ((context.flags & EquipmentModificationFlags.ModifyInventory) != 0 &&
                context.slot.inventory is not null)
                context.slot.inventory.Take(context.slot.slotIndex, ActionSource.Internal);

            // Call events
            if (actionSource == ActionSource.Internal) return EquipmentOperations.Equipped();
            OnItemEquippedSuccessfully(context, EquipmentOperations.Equipped());
            return EquipmentOperations.Equipped();
        }

        /// <summary>
        ///     Unequips an item.
        /// </summary>
        /// <param name="context">Context of action</param>
        /// <param name="actionSource">Source of action</param>
        /// <returns>Result of action</returns>
        internal OperationResult Unequip(
            in UnequipItemContext context,
            ActionSource actionSource = ActionSource.External)
        {
            EquippableItemBase equippableItemRef = context.itemBase;

            // Check if already unequipped
            if (!equippableItemRef.IsEquipped(context))
            {
                if (actionSource == ActionSource.Internal) return EquipmentOperations.NotEquipped();
                OnItemUnequipWhenAlreadyUnequipped(context, EquipmentOperations.NotEquipped());
                return EquipmentOperations.NotEquipped();
            }

            // Check if item can be unequipped
            OperationResult canUnequipResult = CanUnequip(context);
            if (!canUnequipResult && (context.flags & EquipmentModificationFlags.IgnoreConditions) == 0)
            {
                if (actionSource == ActionSource.Internal) return canUnequipResult;
                OnItemUnequipWhenCannotBeUnequipped(context, canUnequipResult);
                return canUnequipResult;
            }

            // Get item to unequip
            EquipmentSlotBase slot = GetFirstEquippedSlot(context.item);
            if (slot != null) return UnequipFromSlot(slot, context, actionSource);

            // Item is not equipped at all
            if (actionSource == ActionSource.Internal) return EquipmentOperations.NotEquipped();
            OnItemUnequipWhenCannotBeUnequipped(context, EquipmentOperations.NotEquipped());
            return EquipmentOperations.NotEquipped();
        }

        internal OperationResult UnequipFromSlot(
            [NotNull] EquipmentSlotBase slot,
            in UnequipItemContext context,
            ActionSource actionSource = ActionSource.External)
        {
            // Sanity check
            Assert.AreEqual(slot.CurrentlyEquippedItem, context.item, "Slot item does not match item to unequip");

            // Add item to inventory if needed
            if ((context.flags & EquipmentModificationFlags.ModifyInventory) != 0 && context.inventory is not null)
                context.inventory.TryAddOrDrop(context.item, 1, ActionSource.Internal);
            else if ((context.flags & EquipmentModificationFlags.ModifyInventory) != 0)
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
            if (actionSource == ActionSource.Internal) return EquipmentOperations.Unequipped();
            OnItemUnequippedSucessfully(context, EquipmentOperations.Unequipped());
            return EquipmentOperations.Unequipped();
        }

#endregion

#region Checks

        /// <summary>
        ///     Check if item can be equipped
        /// </summary>
        /// <param name="context">Context of action</param>
        /// <returns>True if item can be equipped</returns>
        protected virtual OperationResult CanEquip(in EquipItemContext context) =>
            context.itemBase.CanEquip(context);

        /// <summary>
        ///     Check if item can be unequipped
        /// </summary>
        /// <param name="context">Context of action</param>
        /// <returns>True if item can be unequipped</returns>
        protected virtual OperationResult CanUnequip(in UnequipItemContext context) =>
            context.itemBase.CanUnequip(context);

#endregion

#region Events

        protected virtual void OnItemEquippedSuccessfully(in EquipItemContext context, in OperationResult result)
        {
            context.itemBase.OnEquipSuccess(context, result);
        }

        protected virtual void OnItemUnequippedSucessfully(in UnequipItemContext context, in OperationResult result)
        {
            context.itemBase.OnUnequipSuccess(context, result);
        }

        protected virtual void OnItemEquipWhenAlreadyEquipped(in EquipItemContext context, in OperationResult result)
        {
            context.itemBase.OnEquipWhenAlreadyEquipped(context, result);
        }

        protected virtual void OnItemUnequipWhenAlreadyUnequipped(in UnequipItemContext context, in OperationResult result)
        {
            context.itemBase.OnUnequipWhenAlreadyUnequipped(context, result);
        }

        protected virtual void OnItemEquipWhenCannotBeEquipped(in EquipItemContext context, in OperationResult result)
        {
            context.itemBase.OnEquipWhenCannotBeEquipped(context, result);
        }

        protected virtual void OnItemUnequipWhenCannotBeUnequipped(in UnequipItemContext context, in OperationResult result)
        {
            context.itemBase.OnUnequipWhenCannotBeUnequipped(context, result);
        }

#endregion
    }
}