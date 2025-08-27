using Systems.SimpleInventory.Data.Context;

namespace Systems.SimpleInventory.Data.Items
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
        ///     Checks if the item can be equipped.
        /// </summary>
        /// <param name="context">Context of action</param>
        /// <returns>True if the item can be equipped, false otherwise</returns>
        public virtual bool CanEquip(in EquipItemContext context) => true;

        /// <summary>
        ///     Checks if the item can be unequipped.
        /// </summary>
        /// <param name="context">Context of action</param>
        /// <returns>True if the item can be unequipped, false otherwise</returns>
        public virtual bool CanUnequip(in EquipItemContext context) => true;

        /// <summary>
        ///     Called when the item is equipped.
        /// </summary>
        /// <param name="context">Context of action</param>
        public abstract void OnEquip(in EquipItemContext context);

        /// <summary>
        ///     Called when the item is unequipped.
        /// </summary>
        /// <param name="context">Context of action</param>
        public abstract void OnUnequip(in EquipItemContext context);
    }
}