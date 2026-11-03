
namespace Core.Reports;

public sealed record OrderReportLine(
    string OrderId,
    string CustomerId,
    string Status,
    decimal Total
);
