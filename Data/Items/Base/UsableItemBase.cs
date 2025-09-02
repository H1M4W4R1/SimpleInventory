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
        public virtual bool CanUse(in UseItemContext context) => true;
        
        /// <summary>
        ///     Called when the item is used.
        /// </summary>
        /// <param name="context">Context of the usage</param>
        public abstract void OnUse(in UseItemContext context);

        /// <summary>
        ///     Called when the item usage fails.
        /// </summary>
        /// <param name="context">Context of the usage</param>
        protected internal virtual void OnUseFailed(in UseItemContext context) {}
    }
}