using Systems.SimpleCore.Operations;
using Systems.SimpleInventory.Data.Context;
using Systems.SimpleInventory.Operations;

namespace Systems.SimpleInventory.Abstract.Items
{
    /// <summary>
    ///     Item that can be equipped.
    /// </summary>
    public abstract class EquippableItemBase : ItemBase
    {
        /// <summary>
        ///     Checks if the item is equipped.
        /// </summary>
        /// <param name="context">Context to check in</param>
        /// <returns>True if the item is equipped, false otherwise</returns>
        internal bool IsEquipped(in EquipItemContext context) =>
            context.equipment.IsEquipped(context);
        
        /// <summary>
        ///     Checks if the item is equipped.
        /// </summary>
        /// <param name="context">Context to check in</param>
        /// <returns>True if the item is equipped, false otherwise</returns>
        internal bool IsEquipped(in UnequipItemContext context) =>
            context.equipment.IsEquipped(context);

        /// <summary>
        ///     Checks if the item can be equipped.
        /// </summary>
        /// <param name="context">Context of action</param>
        /// <returns>True if the item can be equipped, false otherwise</returns>
        protected internal virtual OperationResult CanEquip(in EquipItemContext context) => InventoryOperations.Permitted();

        /// <summary>
        ///     Checks if the item can be unequipped.
        /// </summary>
        /// <param name="context">Context of action</param>
        /// <returns>True if the item can be unequipped, false otherwise</returns>
        protected internal virtual OperationResult CanUnequip(in UnequipItemContext context) => InventoryOperations.Permitted();

        /// <summary>
        ///     Called when the item is equipped.
        /// </summary>
        protected internal virtual void OnEquipSuccess(in EquipItemContext context, in OperationResult result){}

        /// <summary>
        ///     Called when the item is already equipped.
        /// </summary>
        protected internal virtual void OnEquipWhenAlreadyEquipped(in EquipItemContext context, in OperationResult result){}
        
        /// <summary>
        ///     Called when the item cannot be equipped.
        /// </summary>
        protected internal virtual void OnEquipWhenCannotBeEquipped(in EquipItemContext context, in OperationResult result){}
        
        /// <summary>
        ///     Called when the item is unequipped.
        /// </summary>
        protected internal  virtual void OnUnequipSuccess(in UnequipItemContext context, in OperationResult result){}
        
        /// <summary>
        ///     Called when item is already unequipped.
        /// </summary>
        protected internal virtual void OnUnequipWhenAlreadyUnequipped(in UnequipItemContext context, in OperationResult result){}
        
        /// <summary>
        ///     Called when item cannot be unequipped.
        /// </summary>
        protected internal virtual void OnUnequipWhenCannotBeUnequipped(in UnequipItemContext context, in OperationResult result){}
    }
}