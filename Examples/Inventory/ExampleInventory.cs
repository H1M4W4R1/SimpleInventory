using Sirenix.OdinInspector;
using Systems.SimpleInventory.Components.Inventory;
using Systems.SimpleInventory.Data;
using Systems.SimpleInventory.Examples.Items.Armour;
using Systems.SimpleInventory.Examples.Items.Food;
using UnityEngine;

namespace Systems.SimpleInventory.Examples.Inventory
{
    public sealed class ExampleInventory : InventoryBase
    {
        private void Start()
        {
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
        
        // TODO: Equipment handling
    }
}