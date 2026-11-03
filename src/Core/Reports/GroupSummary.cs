
namespace Core.Reports;

public sealed record GroupSummary(
    string Status,
    int Count,
    decimal Total
);
