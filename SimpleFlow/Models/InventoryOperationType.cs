namespace SimpleFlow.Models;

/// <summary>
/// Тип складской операции.
/// </summary>
public enum InventoryOperationType
{
    // Увеличивает QuantityOnHand;
    // Пересчитывает AverageCost по средневзвешенной стоимости.
    Receipt,

    // Проверяет QuantityAvailable;
    // Уменьшает QuantityOnHand;
    // Не меняет среднюю стоимость.
    Issue,

    // Списывает товар с исходного склада;
    // Приходует на целевой склад;
    // Использует среднюю стоимость исходного склада.
    Transfer,

    // Изменяет остаток до указанного или на указанное количество — режим нужно определить отдельно.
    Adjustment,

    // Увеличивает QuantityReserved;
    // Проверяет доступное количество.
    Reservation,

    // Уменьшает QuantityReserved.
    ReservationRelease
}