
namespace Api.Contracts;

// Дані, які клієнт надсилає для створення або зміни замовлення
public sealed record SaveOrderLineRequest(
    string? ProductId,
    string? Name,
    decimal Price,
    int Quantity);

public sealed record SaveOrderRequest(
    string? Customer,
    List<SaveOrderLineRequest?>? Lines);

// Дані, які API повертає клієнту
public sealed record OrderLineDto(
    string ProductId,
    string Name,
    decimal Price,
    int Quantity);

public sealed record OrderDto(
    string Id,
    string Customer,
    string Status,
    decimal Total,
    IReadOnlyList<OrderLineDto> Lines,
    DateTimeOffset CreatedAt);

public sealed record OrderTotalDto(
    string Id,
    decimal Total);

// Для додаткового завдання з пагінацією
public sealed record PagedOrdersDto(
    IReadOnlyList<OrderDto> Items,
    int Total);
