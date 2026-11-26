namespace Infrastructure.Persistence;

public sealed class OrderRow
{
    public string Id { get; set; } = "";
    public string CustomerId { get; set; } = "";
    public string Status { get; set; } = "Draft";
    public DateTimeOffset CreatedAt { get; set; }

    public List<OrderLineRow> Lines { get; set; } = [];
}
