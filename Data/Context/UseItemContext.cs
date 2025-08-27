using JetBrains.Annotations;
using Systems.SimpleInventory.Components.Inventory;
using Systems.SimpleInventory.Data.Items;
using UnityEngine.Assertions;

namespace Systems.SimpleInventory.Data.Context
{
    public readonly ref struct UseItemContext
    {
        public readonly InventorySlotContext slot;
        public readonly UsableItemBase item;

        public UseItemContext([NotNull] InventoryBase inventory, int slotIndex)
        {
            this.slot = new InventorySlotContext(inventory, slotIndex);
            item = slot.Item as UsableItemBase;
            Assert.AreNotEqual(item, null, "Item is not usable");
        }
    }
}