using System;
using Systems.SimpleInventory.Abstract.Data;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Systems.SimpleInventory.Examples.Items.Food.Data
{
    [Serializable]
    public sealed class FoodData : ItemData
    {
        /// <summary>
        ///     Amount of health restored by eating this food
        /// </summary>
        [field: SerializeField] public int HealthRestore { get; private set; }

        public FoodData(int minHealthRestore, int maxHealthRestore)
        {
            HealthRestore = Random.Range(minHealthRestore, maxHealthRestore + 1);
        }

        public override int CompareTo(ItemData other)
        {
            if (other is not FoodData foodData) return 0;
            return HealthRestore.CompareTo(foodData.HealthRestore);
        }
    }
}