using JetBrains.Annotations;
using Systems.SimpleCore.Storage;
using Systems.SimpleInventory.Data.Items.Abstract;
using Systems.SimpleInventory.Data.Native.Item;

namespace Systems.SimpleInventory.Data
{
    /// <summary>
    ///     Database of all items in game
    /// </summary>
    public sealed class ItemsDatabase : AddressableDatabase<ItemsDatabase, ItemBase>
    {
        public const string LABEL = "SimpleInventory.Items";
        [NotNull] protected override string AddressableLabel => LABEL;
        
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