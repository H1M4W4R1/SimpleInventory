using System;
using Systems.SimpleCore.Operations;
using Systems.SimpleInventory.Data.Context;
using Systems.SimpleInventory.Data.Inventory;
using Systems.SimpleInventory.Data.Items.Base;
using Systems.SimpleInventory.Data.Items.Data;
using Systems.SimpleInventory.Examples.Items.Food.Data;
using UnityEngine;

namespace Systems.SimpleInventory.Examples.Items.Food.Abstract
{
    public abstract class ExampleFoodBase : UsableItemBase, IComparable<ExampleFoodBase>
    {
        [field: SerializeField] public int MinHealthRestore { get; private set; }
        [field: SerializeField] public int MaxHealthRestore { get; private set; }

        public sealed override void OnUse(in UseItemContext context, OperationResult result)
        {
            // Get item data
            ItemData itemData = context.itemInstance.Data;
            if (itemData is not FoodData foodData)
            {
                Debug.LogError($"Item {context.itemInstance} is not food - item data is not valid");
                return;
            }
            
            Debug.Log($"Healed player for {foodData.HealthRestore} using {name} food");
        }

        public int CompareTo(ExampleFoodBase other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (other is null) return 1;
            return MaxHealthRestore.CompareTo(other.MaxHealthRestore);
        }

        public override WorldItem GenerateWorldItem(ItemData itemData)
        {
            // Override item data
            itemData = new FoodData(MinHealthRestore, MaxHealthRestore);
            return base.GenerateWorldItem(itemData);
        }
    }
}