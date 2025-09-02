using JetBrains.Annotations;
using Systems.SimpleInventory.Components.Equipment;
using Systems.SimpleInventory.Components.Inventory;
using Systems.SimpleInventory.Data.Context.Internal;
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
        ///     Allow replacing already equipped item
        /// </summary>
        public readonly bool allowReplace;
        
        /// <summary>
        ///     Remove item from inventory after equipping
        /// </summary>
        public readonly bool removeFromInventory;

        public EquipItemContext(
            [NotNull] EquipmentBase equipment,
            [NotNull] InventoryBase inventory,
            int slotIndex,
            bool allowReplace = false,
            bool removeFromInventory = true)
        {
            this.equipment = equipment;
            slot = new InventorySlotContext(inventory, slotIndex);
            item = slot.Item;
            itemBase = item?.Item as EquippableItemBase;
            this.allowReplace = allowReplace;
            this.removeFromInventory = removeFromInventory;
            Assert.IsNotNull(itemBase, "Item is not equippable");
        }
    }
}