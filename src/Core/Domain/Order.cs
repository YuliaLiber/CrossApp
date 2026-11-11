
using Core.Dto;

namespace Core.Domain;

public sealed class Order
{
    private readonly List<OrderLine> _lines = [];

    public string Id { get; }
    public string CustomerId { get; }
    public OrderStatus Status { get; private set; }
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();
    public decimal Total => _lines.Sum(x => x.Price * x.Quantity);

    private Order(string id, string customerId)
    {
        Id = id;
        CustomerId = customerId;
        Status = OrderStatus.Draft;
    }

    public static Order Create(string id, string customerId)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException(
                "ID замовлення не може бути порожнім");

        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException(
                "ID клієнта не може бути порожнім");

        return new Order(id.Trim(), customerId.Trim());
    }

    public void AddLine(
        string productId,
        string name,
        decimal price,
        int quantity)
    {
        if (Status != OrderStatus.Draft)
            throw new DomainException(
                "Товар можна додавати тільки до замовлення у стані Draft");

        _lines.Add(
            OrderLine.Create(productId, name, price, quantity));
    }

    public void Confirm()
    {
        if (_lines.Count == 0)
            throw new DomainException(
                "Порожнє замовлення не можна підтвердити");

        Status = Status switch
        {
            OrderStatus.Draft => OrderStatus.Confirmed,

            OrderStatus.Confirmed =>
                throw new DomainException(
                    "Замовлення вже підтверджене"),

            OrderStatus.Cancelled =>
                throw new DomainException(
                    "Скасоване замовлення не можна підтвердити"),

            _ => throw new DomainException(
                "Невідомий статус замовлення")
        };
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Cancelled)
            throw new DomainException(
                "Замовлення вже скасоване");

        Status = OrderStatus.Cancelled;
    }

    public OrderDto ToDto() =>
        new(
            Id,
            CustomerId,
            Status.ToString(),
            _lines.Select(x => x.ToDto()).ToList());

    public static Order FromDto(OrderDto dto)
    {
        if (dto is null)
            throw new ArgumentNullException(nameof(dto));

        if (dto.Lines is null)
            throw new ArgumentException(
                "Список товарів не може бути null");

        if (!Enum.TryParse(
                dto.Status,
                true,
                out OrderStatus status))
            throw new ArgumentException(
                $"Невідомий статус: {dto.Status}");

        Order order = Create(dto.Id, dto.CustomerId);

        foreach (var line in dto.Lines)
        {
            order.AddLine(
                line.ProductId,
                line.Name,
                line.Price,
                line.Quantity);
        }

        if (status == OrderStatus.Confirmed)
            order.Confirm();
        else if (status == OrderStatus.Cancelled)
            order.Cancel();

        return order;
    }

    public override string ToString() =>
        $"{Id} | клієнт: {CustomerId} | " +
        $"рядків: {_lines.Count} | " +
        $"сума: {Total:F2} | статус: {Status}";
}
