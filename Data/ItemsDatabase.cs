using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleCore.Storage;
using Systems.SimpleInventory.Data.Items;
using Systems.SimpleInventory.Data.Native.Item;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Systems.SimpleInventory.Data
{
    /// <summary>
    ///     Database of all items in game
    /// </summary>
    public class ItemsDatabase : AddressableDatabase<ItemsDatabase, ItemBase>
    {
        [NotNull] protected override string AddressableLabel => "SimpleInventory.Items";
        
        /// <summary>
        ///     Gets item by identifier
        /// </summary>
        /// <param name="identifier">Identifier of item to get</param>
        /// <returns>Item with given identifier or null if not found</returns>
        [CanBeNull] public static ItemBase GetItem(ItemID identifier)
        {
            _instance.EnsureLoaded();

            int low = 0;
            int high = internalDataStorage.Count - 1;

            while (low <= high)
            {
                int mid = (low + high) >> 1;
                ItemBase midItem = internalDataStorage[mid];

                int cmp = midItem.CompareTo(identifier);
                if (cmp == 0) return midItem;
                if (cmp < 0)
                    low = mid + 1;
                else
                    high = mid - 1;
            }

            return null;
        }

        
    }
}