using System.Collections.Generic;
using JetBrains.Annotations;
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
    public static class ItemsDatabase
    {
        public const string ADDRESSABLE_LABEL = "SimpleInventory.Items";
        private static readonly List<ItemBase> _items = new();


        /// <summary>
        ///     If true this means that all items have been loaded
        /// </summary>
        private static bool _isLoaded;

        /// <summary>
        ///     If true this means that items are currently being loaded
        /// </summary>
        private static bool _isLoading;

        /// <summary>
        ///     Total number of items in database
        /// </summary>
        public static int Count
        {
            get
            {
                EnsureLoaded();
                return _items.Count;
            }
        }

        /// <summary>
        ///     Ensures that all items are loaded
        /// </summary>
        internal static void EnsureLoaded()
        {
            if (!_isLoaded) Load();
        }

        /// <summary>
        ///     Loads all items from Resources folder
        /// </summary>
        private static void Load()
        {
            // Prevent multiple loads
            if (_isLoading) return;
            _isLoading = true;

            // Load items
            AsyncOperationHandle<IList<ItemBase>> request = Addressables.LoadAssetsAsync<ItemBase>(
                new[] {ADDRESSABLE_LABEL}, OnItemLoaded,
                Addressables.MergeMode.Union);
            request.WaitForCompletion();

            OnItemsLoadComplete(request);
        }

        private static void OnItemsLoadComplete(AsyncOperationHandle<IList<ItemBase>> obj)
        {
            // Sort after loading to ensure binary search works correctly
            _items.Sort();
            _isLoaded = true;
            _isLoading = false;
        }

        private static void OnItemLoaded<TObject>(TObject obj)
        {
            if (obj is not ItemBase item) return;
            _items.Add(item);
        }


        /// <summary>
        ///     Gets first item of specified type
        /// </summary>
        /// <typeparam name="TItemType">Item type to get </typeparam>
        /// <returns>First item of specified type or null if no item of specified type is found</returns>
        [CanBeNull] public static TItemType GetItem<TItemType>()
            where TItemType : ItemBase
        {
            EnsureLoaded();

            // Loop through all items
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i] is TItemType item) return item;
            }

            Assert.IsNotNull(null, "Item not found in database");
            return null;
        }

        /// <summary>
        ///     Gets all items of specified type
        /// </summary>
        /// <typeparam name="TItemType">Type of item to get</typeparam>
        /// <returns>Read-only list of items of specified type</returns>
        [NotNull] public static IReadOnlyList<TItemType> GetAllItems<TItemType>()
            where TItemType : ItemBase
        {
            EnsureLoaded();

            List<TItemType> items = new();

            // Loop through all items
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i] is TItemType item) items.Add(item);
            }

            return items;
        }

        /// <summary>
        ///     Gets item by identifier
        /// </summary>
        /// <param name="identifier">Identifier of item to get</param>
        /// <returns>Item with given identifier or null if not found</returns>
        [CanBeNull] public static ItemBase GetItem(ItemID identifier)
        {
            EnsureLoaded();

            int low = 0;
            int high = _items.Count - 1;

            while (low <= high)
            {
                int mid = (low + high) >> 1;
                ItemBase midItem = _items[mid];

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