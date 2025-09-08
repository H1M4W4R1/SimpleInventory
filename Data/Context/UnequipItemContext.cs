using JetBrains.Annotations;
using Systems.SimpleInventory.Abstract.Items;
using Systems.SimpleInventory.Components.Equipment;
using Systems.SimpleInventory.Components.Inventory;
using Systems.SimpleInventory.Data.Enums;
using Systems.SimpleInventory.Data.Inventory;
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
        ///     Flags for modifying action
        /// </summary>
        public readonly EquipmentModificationFlags flags;
        
        public UnequipItemContext(
            [CanBeNull] InventoryBase inventory,
            [NotNull] EquipmentBase equipment,
            [NotNull] WorldItem item,
            EquipmentModificationFlags flags = EquipmentModificationFlags.None)
        {
            this.inventory = inventory;
            this.equipment = equipment;
            this.item = item;
            itemBase = item.Item as EquippableItemBase;
            this.flags = flags;
            Assert.IsNotNull(itemBase, "Item is not equippable");
        }
    }
}