using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Systems.SimpleInventory.Components.Equipment;
using Systems.SimpleInventory.Components.Inventory;
using Systems.SimpleInventory.Data.Items;
using Systems.SimpleInventory.Data.Native.Context;
using UnityEngine.Assertions;

namespace Systems.SimpleInventory.Data.Context
{
    public readonly ref struct EquipItemContext
    {
        public readonly InventorySlotContext slot;
        public readonly EquippableItemBase item;
        public readonly EquipmentBase equipment;
        public readonly bool allowReplace;

        public EquipItemContext(
            [NotNull] EquipmentBase equipment,
            [NotNull] InventoryBase inventory,
            int slotIndex,
            bool allowReplace = false)
        {
            this.equipment = equipment;
            slot = new InventorySlotContext(inventory, slotIndex);
            item = slot.Item as EquippableItemBase;
            this.allowReplace = allowReplace;
            Assert.AreNotEqual(item, null, "Item is not equippable");
        }
    }
}