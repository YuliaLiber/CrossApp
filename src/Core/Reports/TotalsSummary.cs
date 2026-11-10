
namespace Core.Reports;

public sealed record TotalsSummary(
    int Items,
    decimal Total,
    decimal Average,
    decimal Max
);
