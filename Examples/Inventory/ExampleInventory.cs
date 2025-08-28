using System.Text;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Systems.SimpleInventory.Components.Inventory;
using Systems.SimpleInventory.Data;
using Systems.SimpleInventory.Examples.Equipment;
using Systems.SimpleInventory.Examples.Items.Abstract;
using Systems.SimpleInventory.Examples.Items.Armour;
using Systems.SimpleInventory.Examples.Items.Food;
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
            Debug.Log("Database item count: " + ItemsDatabase.Count);
        }

        [Button("Use first food")]
        public void UseFirstFood() => UseAnyItem<ExampleFoodBase>();
        
        [Button("Use best food")]
        public void UseBestFoodExample() => UseBestItem<ExampleFoodBase>();
        
        [Button("Equip leather armor")]
        public void EquipLeatherArmor()
        {
            if (!_equipment) return;
            EquipAnyItem<ExampleLeatherBoots>(_equipment);
            EquipAnyItem<ExampleLeatherPants>(_equipment);
            EquipAnyItem<ExampleLeatherTunic>(_equipment);
            EquipAnyItem<ExampleLeatherCap>(_equipment);
            
            PrintEquippedArmor();
        }

        [Button("Equip steel armor")]
        public void EquipSteelArmor()
        {
            if (!_equipment) return;
            EquipAnyItem<ExampleSteelHelmet>(_equipment);
            EquipAnyItem<ExampleSteelChestplate>(_equipment);
            EquipAnyItem<ExampleSteelLeggings>(_equipment);
            EquipAnyItem<ExampleSteelBoots>(_equipment);
            
            PrintEquippedArmor();
        }

        [Button("Unequip armor")]
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
            HelmetItemBase helmet = _equipment.GetFirstEquippedItemFor<HelmetItemBase>();
            ChestplateItemBase chestplate = _equipment.GetFirstEquippedItemFor<ChestplateItemBase>();
            LeggingsItemBase leggings = _equipment.GetFirstEquippedItemFor<LeggingsItemBase>();
            BootsItemBase boots = _equipment.GetFirstEquippedItemFor<BootsItemBase>();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Helmet: {(helmet ? helmet.name : "None")}");
            sb.AppendLine($"Chestplate: {(chestplate ? chestplate.name : "None")}");
            sb.AppendLine($"Leggings: {(leggings ? leggings.name : "None")}");
            sb.AppendLine($"Boots: {(boots ? boots.name : "None")}");
            Debug.Log(sb.ToString());

        }
    }
}