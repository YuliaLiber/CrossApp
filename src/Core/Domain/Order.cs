using Core.Dto;

namespace Core.Domain;

public sealed class Order
{
    private readonly List<OrderLine> _lines = [];

    public string Id { get; }
    public string CustomerId { get; }

    public OrderStatus Status { get; private set; }

    public IReadOnlyList<OrderLine> Lines =>
        _lines.AsReadOnly();

    public decimal Total =>
        _lines.Sum(line => line.Price * line.Quantity);

    private Order(
        string id,
        string customerId)
    {
        Id = id;
        CustomerId = customerId;
        Status = OrderStatus.Draft;
    }

    public static Order Create(
        string id,
        string customerId)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException(
                "Ідентифікатор замовлення не може бути порожнім",
                nameof(id));

        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException(
                "Ідентифікатор клієнта не може бути порожнім",
                nameof(customerId));

        return new Order(
            id.Trim(),
            customerId.Trim());
    }

    public void AddLine(
        string productId,
        string name,
        decimal price,
        int quantity)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException(
                $"До замовлення {Id} зі статусом {Status} рядки додавати не можна");

        OrderLine line = OrderLine.Create(
            productId,
            name,
            price,
            quantity);

        _lines.Add(line);
    }

    public void Confirm()
    {
        if (_lines.Count == 0)
            throw new InvalidOperationException(
                $"Замовлення {Id} порожнє, його не можна підтвердити");

        ChangeStatus(OrderStatus.Confirmed);
    }

    public void Cancel()
    {
        ChangeStatus(OrderStatus.Cancelled);
    }

    private void ChangeStatus(OrderStatus newStatus)
    {
        bool allowed =
            (Status, newStatus) switch
            {
                (OrderStatus.Draft,
                 OrderStatus.Confirmed) => true,

                (OrderStatus.Draft,
                 OrderStatus.Cancelled) => true,

                (OrderStatus.Confirmed,
                 OrderStatus.Cancelled) => true,

                _ => false
            };

        if (!allowed)
            throw new InvalidOperationException(
                $"Перехід зі статусу {Status} у {newStatus} заборонений");

        Status = newStatus;
    }

    public OrderDto ToDto()
    {
        List<OrderLineDto> lineDtos =
            _lines
                .Select(line => line.ToDto())
                .ToList();

        return new OrderDto(
            Id,
            CustomerId,
            Status.ToString(),
            lineDtos);
    }

    public static Order FromDto(OrderDto dto)
    {
        if (dto is null)
            throw new ArgumentNullException(nameof(dto));

        if (dto.Lines is null)
            throw new ArgumentException(
                "Список рядків замовлення не може бути null",
                nameof(dto));

        if (!Enum.TryParse(
                dto.Status,
                true,
                out OrderStatus status))
            throw new ArgumentException(
                $"Невідомий статус замовлення: {dto.Status}",
                nameof(dto));

        Order order = Create(
            dto.Id,
            dto.CustomerId);

        foreach (OrderLineDto line in dto.Lines)
        {
            order.AddLine(
                line.ProductId,
                line.Name,
                line.Price,
                line.Quantity);
        }

        switch (status)
        {
            case OrderStatus.Draft:
                break;

            case OrderStatus.Confirmed:
                order.Confirm();
                break;

            case OrderStatus.Cancelled:
                order.Cancel();
                break;
        }

        return order;
    }

    public override string ToString()
    {
        string statusText =
            Status switch
            {
                OrderStatus.Draft =>
                    "чернетка",

                OrderStatus.Confirmed =>
                    "підтверджене",

                OrderStatus.Cancelled =>
                    "скасоване",

                _ =>
                    "невідомо"
            };

        return $"{Id} | клієнт: {CustomerId} | " +
               $"рядків: {_lines.Count} | " +
               $"сума: {Total:F2} | " +
               $"статус: {statusText}";
    }
}