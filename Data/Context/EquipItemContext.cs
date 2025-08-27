using JetBrains.Annotations;
using Systems.SimpleInventory.Components.Equipment;
using Systems.SimpleInventory.Components.Inventory;
using Systems.SimpleInventory.Data.Items;
using UnityEngine.Assertions;

namespace Systems.SimpleInventory.Data.Context
{
    public readonly ref struct EquipItemContext
    {
        public readonly InventorySlotContext slot;
        public readonly EquippableItemBase item;
        public readonly EquipmentBase equipment;
        public readonly bool allowReplace;
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
            item = slot.Item as EquippableItemBase;
            this.allowReplace = allowReplace;
            this.removeFromInventory = removeFromInventory;
            Assert.AreNotEqual(item, null, "Item is not equippable");
        }
    }
}