using JetBrains.Annotations;
using Systems.SimpleInventory.Components.Equipment;
using Systems.SimpleInventory.Components.Inventory;
using Systems.SimpleInventory.Data.Context.Internal;
using Systems.SimpleInventory.Data.Enums;
using Systems.SimpleInventory.Data.Inventory;
using Systems.SimpleInventory.Data.Items.Base;
using UnityEngine.Assertions;

namespace Systems.SimpleInventory.Data.Context
{
    /// <summary>
    ///     Context for equipping item
    /// </summary>
    public readonly ref struct EquipItemContext
    {
        /// <summary>
        ///     Slot that contains item to equip
        /// </summary>
        public readonly InventorySlotContext slot;

        /// <summary>
        ///     Item being equipped
        /// </summary>
        public readonly WorldItem item;

        /// <summary>
        ///     Reference to item base for easier handling
        /// </summary>
        public readonly EquippableItemBase itemBase;

        /// <summary>
        ///     Equipment where item is being equipped
        /// </summary>
        public readonly EquipmentBase equipment;

        /// <summary>
        ///     Flags for modifying action
        /// </summary>
        public readonly EquipmentModificationFlags flags;

        /// <summary>
        ///     Result of equipping item
        /// </summary>
        public readonly EquipItemResult reason;

        public EquipItemContext WithReason(EquipItemResult newReason)
        {
            return new EquipItemContext(equipment, slot, item, itemBase, flags, newReason);
        }

        public EquipItemContext(
            [NotNull] EquipmentBase equipment,
            [NotNull] InventoryBase inventory,
            int slotIndex,
            EquipmentModificationFlags flags,
            EquipItemResult reason = EquipItemResult.EquippedSuccessfully)
        {
            this.equipment = equipment;
            slot = new InventorySlotContext(inventory, slotIndex);
            item = slot.Item;
            itemBase = item?.Item as EquippableItemBase;
            this.flags = flags;
            this.reason = reason;
            Assert.IsNotNull(itemBase, "Item is not equippable");
        }

        private EquipItemContext(
            [NotNull] EquipmentBase equipment,
            InventorySlotContext slot,
            WorldItem item,
            EquippableItemBase itemBase,
            EquipmentModificationFlags flags,
            EquipItemResult reason = EquipItemResult.EquippedSuccessfully)
        {
            this.equipment = equipment;
            this.slot = slot;
            this.item = item;
            this.itemBase = itemBase;
            this.flags = flags;
            this.reason = reason;
        }
    }
}