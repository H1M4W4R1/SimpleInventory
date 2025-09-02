using Systems.SimpleCore.Operations;
using Systems.SimpleInventory.Data.Context;
using Systems.SimpleInventory.Operations;

namespace Systems.SimpleInventory.Data.Items.Base
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
        public bool IsEquipped(in EquipItemContext context) =>
            context.equipment.IsEquipped(context);
        
        /// <summary>
        ///     Checks if the item is equipped.
        /// </summary>
        /// <param name="context">Context to check in</param>
        /// <returns>True if the item is equipped, false otherwise</returns>
        public bool IsEquipped(in UnequipItemContext context) =>
            context.equipment.IsEquipped(context);

        /// <summary>
        ///     Checks if the item can be equipped.
        /// </summary>
        /// <param name="context">Context of action</param>
        /// <returns>True if the item can be equipped, false otherwise</returns>
        public virtual OperationResult CanEquip(in EquipItemContext context) => InventoryOperations.Permitted();

        /// <summary>
        ///     Checks if the item can be unequipped.
        /// </summary>
        /// <param name="context">Context of action</param>
        /// <returns>True if the item can be unequipped, false otherwise</returns>
        public virtual OperationResult CanUnequip(in UnequipItemContext context) => InventoryOperations.Permitted();

        /// <summary>
        ///     Called when the item is equipped.
        /// </summary>
        protected internal virtual void OnEquip(in EquipItemContext context, in OperationResult result){}

        /// <summary>
        ///     Called when the item is already equipped.
        /// </summary>
        protected internal virtual void OnAlreadyEquipped(in EquipItemContext context, in OperationResult result){}
        
        /// <summary>
        ///     Called when the item cannot be equipped.
        /// </summary>
        protected internal virtual void OnCannotBeEquipped(in EquipItemContext context, in OperationResult result){}
        
        /// <summary>
        ///     Called when the item is unequipped.
        /// </summary>
        protected internal  virtual void OnUnequip(in UnequipItemContext context, in OperationResult result){}
        
        /// <summary>
        ///     Called when item is already unequipped.
        /// </summary>
        protected internal virtual void OnAlreadyUnequipped(in UnequipItemContext context, in OperationResult result){}
        
        /// <summary>
        ///     Called when item cannot be unequipped.
        /// </summary>
        protected internal virtual void OnCannotBeUnequipped(in UnequipItemContext context, in OperationResult result){}
    }
}