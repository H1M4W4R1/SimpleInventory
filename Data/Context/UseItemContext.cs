using JetBrains.Annotations;
using Systems.SimpleInventory.Components.Inventory;
using Systems.SimpleInventory.Data.Context.Internal;
using Systems.SimpleInventory.Data.Inventory;
using Systems.SimpleInventory.Data.Items.Abstract;
using UnityEngine.Assertions;

namespace Systems.SimpleInventory.Data.Context
{
    /// <summary>
    ///     Context for using item
    /// </summary>
    public readonly ref struct UseItemContext
    {
        /// <summary>
        ///     Inventory slot where item is being used
        /// </summary>
        public readonly InventorySlotContext slot;
        
        /// <summary>
        ///     Item being used
        /// </summary>
        public readonly WorldItem item;

        public UseItemContext([NotNull] InventoryBase inventory, int slotIndex)
        {
            slot = new InventorySlotContext(inventory, slotIndex);
            item = slot.Item;
            Assert.IsTrue(item?.Item is UsableItemBase, "Item is not usable");
        }
    }
}