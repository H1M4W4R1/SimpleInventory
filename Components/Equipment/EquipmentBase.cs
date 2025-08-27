using Systems.SimpleInventory.Data.Context;
using Systems.SimpleInventory.Data.Enums;
using UnityEngine;

namespace Systems.SimpleInventory.Components.Equipment
{
    /// <summary>
    ///     Equipment that can equip/unequip items.
    /// </summary>
    public abstract class EquipmentBase : MonoBehaviour
    {
        // TODO: Equipped items cache / slots?
        // TODO: Prepare equipment slots in Awake/Destroy?
        // TODO: Save/Load from data.

        /// <summary>
        ///     Check if item can be unequipped
        /// </summary>
        /// <param name="context">Context of action</param>
        /// <returns>True if item can be unequipped</returns>
        public virtual bool CanUnequip(in EquipItemContext context) => true;

        /// <summary>
        ///     Check if item can be equipped
        /// </summary>
        /// <param name="context">Context of action</param>
        /// <returns>True if item can be equipped</returns>
        public virtual bool CanEquip(in EquipItemContext context) => true;

        /// <summary>
        ///     Equips an item.
        /// </summary>
        /// <param name="context">Context of action</param>
        /// <returns>Result of action</returns>
        public EquipItemResult Equip(in EquipItemContext context)
        {
            // Check if already equipped
            if (context.item.IsEquipped(context))
            {
                OnItemAlreadyEquipped(context);
                return EquipItemResult.AlreadyEquipped;
            }

            // Check if item can be equipped
            if (!context.item.CanEquip(context) || !CanEquip(context))
            {
                OnItemCannotBeEquipped(context);
                return EquipItemResult.CannotBeEquipped;
            }

            // Equip item
            context.item.OnEquip(context);
            OnItemEquipped(context);

            // TODO: Equip item to cache

            return EquipItemResult.EquippedSuccessfully;
        }

        /// <summary>
        ///     Unequips an item.
        /// </summary>
        /// <param name="context">Context of action</param>
        /// <returns>Result of action</returns>
        public EquipItemResult Unequip(in EquipItemContext context)
        {
            // Check if already unequipped
            if (!context.item.IsEquipped(context))
            {
                OnItemAlreadyUnequipped(context);
                return EquipItemResult.AlreadyUnequipped;
            }

            // Check if item can be unequipped
            if (!context.item.CanUnequip(context) || !CanUnequip(context))
            {
                OnItemCannotBeUnequipped(context);
                return EquipItemResult.CannotBeUnequipped;
            }

            // Unequip item
            OnItemUnequipped(context);
            context.item.OnUnequip(context);
            
            // TODO: Unequip item from cache

            return EquipItemResult.EquippedSuccessfully;
        }

        /// <summary>
        ///     Checks if item is equipped.
        /// </summary>
        /// <param name="context">Action context</param>
        /// <returns>True if item is equipped</returns>
        public bool IsEquipped(in EquipItemContext context)
        {
            // TODO: Implement
            return false;
        }

        protected virtual void OnItemEquipped(in EquipItemContext context) { }
        
        protected virtual void OnItemUnequipped(in EquipItemContext context) { }
        
        protected virtual void OnItemAlreadyEquipped(in EquipItemContext context) { }
        
        protected virtual void OnItemAlreadyUnequipped(in EquipItemContext context) { }
        
        protected virtual void OnItemCannotBeEquipped(in EquipItemContext context) { }
        
        protected virtual void OnItemCannotBeUnequipped(in EquipItemContext context) { }
    }
}