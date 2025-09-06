using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Storage;
using Systems.SimpleCore.Storage.Lists;
using Systems.SimpleInventory.Components.Inventory;
using Systems.SimpleInventory.Data;
using Systems.SimpleInventory.Data.Context;
using Systems.SimpleInventory.Data.Items.Base;
using Systems.SimpleInventory.Examples.Equipment;
using Systems.SimpleInventory.Examples.Items.Armour;
using Systems.SimpleInventory.Examples.Items.Armour.Abstract;
using Systems.SimpleInventory.Examples.Items.Food;
using Systems.SimpleInventory.Examples.Items.Food.Abstract;
using UnityEngine;

namespace Systems.SimpleInventory.Examples.Inventory
{
    [RequireComponent(typeof(ExampleEquipment))]
    public sealed class ExampleInventory : InventoryBase
    {
        [CanBeNull] private ExampleEquipment _equipment;
        
        private void Start()
        {
            _equipment = GetComponent<ExampleEquipment>();
            
            // Add example items to inventory
            
            // Leather armor
            TryAdd<ExampleLeatherBoots>(1);
            TryAdd<ExampleLeatherPants>(1);
            TryAdd<ExampleLeatherTunic>(1);
            TryAdd<ExampleLeatherCap>(1);
            
            // Steel armor
            TryAdd<ExampleSteelHelmet>(1);
            TryAdd<ExampleSteelChestplate>(1);
            TryAdd<ExampleSteelLeggings>(1);
            TryAdd<ExampleSteelBoots>(1);
            
            // Food 
            TryAdd<ExampleApple>(1);
            TryAdd<ExampleBread>(1);
            
            // Print database item count
            Debug.Log("Database entries count: " + ItemsDatabase.Count);
        }

        [ContextMenu("Use first food")]
        public void UseFirstFood() => UseAnyItem<ExampleFoodBase>();
        
        [ContextMenu("Use best food")]
        public void UseBestFoodExample() => UseBestItem<ExampleFoodBase>();
        
        [ContextMenu("Equip leather armor")]
        public void EquipLeatherArmor()
        {
            if (!_equipment) return;
            EquipAnyItem<ExampleLeatherBoots>(_equipment);
            EquipAnyItem<ExampleLeatherPants>(_equipment);
            EquipAnyItem<ExampleLeatherTunic>(_equipment);
            EquipAnyItem<ExampleLeatherCap>(_equipment);
            
            PrintEquippedArmor();
        }

        [ContextMenu("Equip steel armor")]
        public void EquipSteelArmor()
        {
            if (!_equipment) return;
            EquipAnyItem<ExampleSteelHelmet>(_equipment);
            EquipAnyItem<ExampleSteelChestplate>(_equipment);
            EquipAnyItem<ExampleSteelLeggings>(_equipment);
            EquipAnyItem<ExampleSteelBoots>(_equipment);
            
            PrintEquippedArmor();
        }

        [ContextMenu("Unequip armor")]
        public void UnequipArmor()
        {
            if (!_equipment) return;
            UnequipAnyItem<BootsItemBase>(_equipment);
            UnequipAnyItem<LeggingsItemBase>(_equipment);
            UnequipAnyItem<ChestplateItemBase>(_equipment);
            UnequipAnyItem<HelmetItemBase>(_equipment);

            PrintEquippedArmor();
        }

        private void PrintEquippedArmor()
        {
            if (!_equipment) return;
            HelmetItemBase helmet = _equipment.GetFirstEquippedBaseItemFor<HelmetItemBase>();
            ChestplateItemBase chestplate = _equipment.GetFirstEquippedBaseItemFor<ChestplateItemBase>();
            LeggingsItemBase leggings = _equipment.GetFirstEquippedBaseItemFor<LeggingsItemBase>();
            BootsItemBase boots = _equipment.GetFirstEquippedBaseItemFor<BootsItemBase>();

            StringBuilder sb = new();
            sb.AppendLine($"Helmet: {(helmet ? helmet.name : "None")}");
            sb.AppendLine($"Chestplate: {(chestplate ? chestplate.name : "None")}");
            sb.AppendLine($"Leggings: {(leggings ? leggings.name : "None")}");
            sb.AppendLine($"Boots: {(boots ? boots.name : "None")}");
            Debug.Log(sb.ToString());

        }

        [ContextMenu("Print all equippable items")]
        public void PrintAllEquippableItems()
        {
            ROListAccess<EquippableItemBase> databaseItems = ItemsDatabase.GetAll<EquippableItemBase>();
            IReadOnlyList<EquippableItemBase> listAccess = databaseItems.List;
            
            StringBuilder sb = new();
            
            for (int i = 0; i < listAccess.Count; i++)
            {
                sb.AppendLine($"{listAccess[i].name}");
            } 
            
            Debug.Log(sb.ToString());
            databaseItems.Release();
        }

        protected override void OnItemAdded(in AddItemContext context, in OperationResult<int> resultAmountLeft)
        {
            base.OnItemAdded(in context,resultAmountLeft);
            Debug.Log($"Item added: {context.itemInstance.Item.name}");
        }

        protected override void OnItemAddFailed(in AddItemContext context, in OperationResult<int> resultAmountExpected)
        {
            base.OnItemAddFailed(in context, resultAmountExpected);
            Debug.Log($"Item add failed: {context.itemInstance.Item.name}");
        }

        protected override void OnItemTaken(in TakeItemContext context, in OperationResult<int> resultAmountLeft)
        {
            base.OnItemTaken(in context, resultAmountLeft);
            Debug.Log($"Item taken: {context.itemInstance.name}");
        }

        protected override void OnItemTakeFailed(in TakeItemContext context, in OperationResult<int> resultAmountExpected)
        {
            base.OnItemTakeFailed(in context, resultAmountExpected);
            Debug.Log($"Item take failed: {context.itemInstance.name}");
        }
    }
}