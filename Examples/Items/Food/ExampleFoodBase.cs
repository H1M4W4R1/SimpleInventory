using System;
using Systems.SimpleInventory.Data.Context;
using Systems.SimpleInventory.Data.Items;
using Systems.SimpleInventory.Data.Items.Abstract;
using UnityEngine;

namespace Systems.SimpleInventory.Examples.Items.Food
{
    public abstract class ExampleFoodBase : UsableItemBase, IComparable<ExampleFoodBase>
    {
        [field: SerializeField] public int HealthRestore { get; private set; }

        public sealed override void OnUse(in UseItemContext context)
        {
            Debug.Log($"Healed player for {HealthRestore} using {name} food");
        }

        public int CompareTo(ExampleFoodBase other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (other is null) return 1;
            return HealthRestore.CompareTo(other.HealthRestore);
        }
    }
}