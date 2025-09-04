<div align="center">
  <h1>Simple Inventory</h1>
</div>

# About

Simple Inventory is a Simple Kit package that provides:

- Inventory management with stacking, querying, add/take, and transfer
- Equipment system with typed slots, equip/unequip flows and inventory integration
- World pickups and drop helpers
- Rich operation results for clear error/success handling

*For requirements check .asmdef*

# Usage

## Items and databases

Items are ScriptableObjects based on `ItemBase` with optional specializations:

- `UsableItemBase` for consumables with `CanUse`/`OnUse`
- `EquippableItemBase` for equipment with `CanEquip/CanUnequip` and equipment hooks

Items are resolved via `ItemsDatabase.GetExact<T>()` and instantiated to `WorldItem` with `GenerateWorldItem(itemData)`.

## Inventory

Create an inventory by extending `InventoryBase` and add it to a GameObject. Use provided APIs to add, take, query and transfer items.

```csharp
using Systems.SimpleCore.Utility.Enums;           // ActionSource
using Systems.SimpleInventory.Components.Inventory;
using Systems.SimpleInventory.Data.Items.Base;
using Systems.SimpleInventory.Data.Items.Data;

public sealed class PlayerInventory : InventoryBase
{
}

// Adding items by type
OperationResult<int> addLeft = playerInventory.TryAdd<HealthPotion>(amount: 5, itemData: new ItemData());
bool added = addLeft;                   // success?
int notAdded = (int)addLeft;            // leftover

// Taking items by type
OperationResult<int> takeLeft = playerInventory.TryTake<HealthPotion>(amount: 3);

// Counting and checks
bool hasTwo = playerInventory.Has<HealthPotion>(2);
int count = playerInventory.Count<HealthPotion>();

// Adding or dropping leftovers
playerInventory.TryAddOrDrop(ItemsDatabase.GetExact<Arrow>().GenerateWorldItem(null), 100);
```

Finding and using items:

```csharp
// First/best item of type
var firstPotion = playerInventory.GetFirstItemOfType<HealthPotion>();
var bestSword = playerInventory.GetBestItem<Sword>();

// Using an item by slot or best-of-type
playerInventory.UseItem(firstPotion.slotIndex);
playerInventory.UseBestItem<HealthPotion>();
```

Transfers and stacking rules:

```csharp
using Systems.SimpleInventory.Data.Enums; // ItemTransferFlags

// Simple transfer (swaps stacks if same item)
// When targetInventory is same as source inventory then it's internal transfer
playerInventory.TransferItem(sourceSlot: 0, targetInventory: playerInventory, targetSlot: 5);

// Slot to slot or cross-inventory swap or combine
playerInventory.TransferItem(0, chestInventory, 2, ItemTransferFlags.AllowPartialTransfer);
```

There's also `TransferItems` method that allows to transfer multiple stacks at once.

Pickups and dropping:

```csharp
using Systems.SimpleInventory.Components.Items.Pickup;

// Pickup (validates and adds)
var picked = playerInventory.TryPickupItem(pickupComponent, amount: pickupComponent.Amount);

// Drop specific world item as pickup at inventory drop transform
playerInventory.DropItemAs<PickupItemWithDestroy>(worldSword, amount: 1);
```

## Equipment

Define equipment by extending `EquipmentBase`, building typed slots, and use inventory methods to equip/unequip.

```csharp
using Systems.SimpleInventory.Components.Equipment;
using Systems.SimpleInventory.Data.Items.Base;

public sealed class PlayerEquipment : EquipmentBase
{
    protected override void BuildEquipmentSlots()
    {
        equipmentSlots.Clear();
        AddEquipmentSlotFor<Helmet>();
        AddEquipmentSlotFor<Sword>();
    }
}

// Equip from inventory
playerInventory.EquipAnyItem<Sword>(playerEquipment);

// Unequip to inventory
playerInventory.UnequipAnyItem<Sword>(playerEquipment);

// Equip best sword by IComparable ranking
playerInventory.EquipBestItem<Sword>(playerEquipment);
```

Equipment checks and events are routed through item definitions, with common outcomes from `EquipmentOperations` and `InventoryOperations`.

## Custom rules and events

Override inventory checks to enforce rules:

- `CanAddItem(AddItemContext)` – capacity and item‑specific constraints
- `CanTakeItem(TakeItemContext)` – availability and item rules
- `CanDropItem(DropItemContext)` – pre‑drop checks
- `CanTransferItem(TransferItemContext)` – swap/combine/partial rules

Override events to hook gameplay/UI:

- Added/Taken/PickedUp/Dropped/Transferred, including failure variants
- Use success/failure

Items and equipment also expose hook methods that receive the same contexts.

## Operations

Use operation helpers from `InventoryOperations` and `EquipmentOperations` built on `OperationResult` from SimpleCore, e.g.:

- Inventory: `ItemsAdded()`, `ItemsTaken()`, `ItemsTransferred()`, `ItemsDropped()`, `ItemsPickedUp()`, `NotEnoughSpace()`, `NotEnoughItems()`, `InvalidAmount()`
- Equipment: `Equipped()`, `Unequipped()`, `AlreadyEquipped()`, `NotEquipped()`, `NoFreeSlots()`, `ItemNotEquippable()`

# Notes

- Use `ActionSource.Internal` for silent system actions to avoid double event emissions.
- `InventorySize` defines slot count; each slot holds one `WorldItem` stack up to `ItemBase.MaxStack`.
- `TryAdd`/`TryTake` return the amount left as `OperationResult<int>`; implicit bool indicates success.
OdinInspector is required