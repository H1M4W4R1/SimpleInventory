using JetBrains.Annotations;
using Systems.SimpleInventory.Components.Equipment;
using Systems.SimpleInventory.Components.Inventory;
using Systems.SimpleInventory.Data.Inventory;
using Systems.SimpleInventory.Data.Items.Base;
using UnityEngine.Assertions;

namespace Systems.SimpleInventory.Data.Context
{
    /// <summary>
    ///     Context for unequipping item
    /// </summary>
    public readonly ref struct UnequipItemContext
    {
        /// <summary>
        ///     Inventory where item is being unequipped
        /// </summary>
        [CanBeNull] public readonly InventoryBase inventory;
        
        /// <summary>
        ///     Item being unequipped
        /// </summary>
        public readonly WorldItem item;
        
        /// <summary>
        ///     Item base for easier handling
        /// </summary>
        public readonly EquippableItemBase itemBase;
        
        /// <summary>
        ///     Equipment where item is being unequipped
        /// </summary>
        public readonly EquipmentBase equipment;
        
        /// <summary>
        ///     If true, item will be added to inventory before unequipping,
        ///     may be drooped if <see cref="inventory"/> is null
        /// </summary>
        public readonly bool addToInventory;
        
        public UnequipItemContext(
            [CanBeNull] InventoryBase inventory,
            [NotNull] EquipmentBase equipment,
            [NotNull] WorldItem item,
            bool addToInventory = true)
        {
            this.inventory = inventory;
            this.equipment = equipment;
            this.item = item;
            itemBase = item.Item as EquippableItemBase;
            this.addToInventory = addToInventory;
            Assert.IsNotNull(itemBase, "Item is not equippable");
        }
    }
}