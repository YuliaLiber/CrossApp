
namespace Core.Reports;

public sealed record OrderProductSummary(
    string OrderId,
    string ProductName,
    int Quantity,
    decimal Total
);
