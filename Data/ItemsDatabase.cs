using JetBrains.Annotations;
using Systems.SimpleCore.Storage;
using Systems.SimpleCore.Storage.Databases;
using Systems.SimpleInventory.Data.Items.Base;

namespace Systems.SimpleInventory.Data
{
    /// <summary>
    ///     Database of all items in game
    /// </summary>
    public sealed class ItemsDatabase : AddressableDatabase<ItemsDatabase, ItemBase>
    {
        public const string LABEL = "SimpleInventory.Items";
        [NotNull] protected override string AddressableLabel => LABEL;
        
    }
}