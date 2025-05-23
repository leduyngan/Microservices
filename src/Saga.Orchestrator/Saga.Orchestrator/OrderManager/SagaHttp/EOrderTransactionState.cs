namespace Saga.Orchestrator.OrderManager;

public enum EOrderTransactionState
{
    NotStarted,
    BasketGot,
    BasketGetFailed,
    BasketDeleted,
    OrderCreated,
    OrderCreatedFailed,
    OrderDeleted,
    OrderDeletedFailed,
    OrderGot,
    OrderGetFailed,
    InventoryUpdated,
    InventoryUpdatedFailed,
    RollbackInventory,
    InventoryRollback,
    InventoryRollbackFailed,
}