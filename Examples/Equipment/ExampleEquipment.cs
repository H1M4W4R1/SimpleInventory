using Systems.SimpleInventory.Components.Equipment;
using Systems.SimpleInventory.Examples.Items.Armour.Abstract;

namespace Systems.SimpleInventory.Examples.Equipment
{
    public sealed class ExampleEquipment : EquipmentBase
    {
        public override void BuildEquipmentSlots()
        {
            // Create slots
            AddEquipmentSlotFor<HelmetItemBase>();
            AddEquipmentSlotFor<ChestplateItemBase>();
            AddEquipmentSlotFor<LeggingsItemBase>();
            AddEquipmentSlotFor<BootsItemBase>();
        }
    }
}