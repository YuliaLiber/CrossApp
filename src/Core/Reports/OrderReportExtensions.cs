
using Core.Domain;

namespace Core.Reports;

public static class OrderReportExtensions
{
    public static IEnumerable<Order> WithMinimumTotal(
        this IEnumerable<Order> orders,
        decimal minimum)
    {
        return orders.Where(o => o.Total >= minimum);
    }
}
