using Systems.SimpleInventory.Components.Inventory;
using Systems.SimpleInventory.Data;
using Systems.SimpleInventory.Examples.Items;
using UnityEngine;

namespace Systems.SimpleInventory.Examples.Inventory
{
    public sealed class ExampleInventory : InventoryBase
    {
        [SerializeField] private ExampleSimpleItem exampleItem;
        
        [ContextMenu("Count Supported Items")]
        public void CountSupportedItems()
        {
            Debug.Log($"Total supported items: {ItemsDatabase.Count}");
        }

        [ContextMenu("Add Example Item")] public void AddExampleItem()
        {
            int toAdd = 5;
            int left = TryAdd(exampleItem, toAdd);
            
            Debug.Log($"Added {toAdd - left} items out of {toAdd}");
        }
        
    }
}