using Systems.SimpleCore.Operations;
using Systems.SimpleInventory.Data.Context;

namespace Systems.SimpleInventory.Data.Items.Base
{
    /// <summary>
    ///     Item that can be used.
    /// </summary>
    public abstract class UsableItemBase : ItemBase
    {
        /// <summary>
        ///     Checks if the item can be used.
        /// </summary>
        /// <param name="context">Context of the usage</param>
        /// <returns>True if the item can be used, false otherwise</returns>
        public virtual OperationResult CanUse(in UseItemContext context) => OperationResult.GenericSuccess;
        
        /// <summary>
        ///     Called when the item is used.
        /// </summary>
        public abstract void OnUse(in UseItemContext context, OperationResult result);

        /// <summary>
        ///     Called when the item usage fails.
        /// </summary>
        protected internal virtual void OnUseFailed(in UseItemContext context, OperationResult result) {}
    }
}