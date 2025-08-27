using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Systems.SimpleInventory.Components.Equipment;
using Systems.SimpleInventory.Components.Inventory;
using Systems.SimpleInventory.Data.Items;
using Systems.SimpleInventory.Data.Native.Context;
using UnityEngine.Assertions;

namespace Systems.SimpleInventory.Data.Context
{
    public readonly ref struct UnequipItemContext
    {
        public readonly InventoryBase inventory;
        public readonly EquippableItemBase item;
        public readonly EquipmentBase equipment;
        public readonly bool addToInventory;
        
        public UnequipItemContext(
            [NotNull] InventoryBase inventory,
            [NotNull] EquipmentBase equipment,
            [NotNull] EquippableItemBase item,
            bool addToInventory = true)
        {
            this.inventory = inventory;
            this.equipment = equipment;
            this.item = item;
            this.addToInventory = addToInventory;
        }
    }
}