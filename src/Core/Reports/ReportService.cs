
using Core.Domain;

namespace Core.Reports;

public static class ReportService
{
    public static IReadOnlyList<OrderReportLine> ExpensiveOrders(
        IEnumerable<Order> source, decimal threshold)
    {
        return source
            .Where(o => o.Total > threshold)
            .OrderByDescending(o => o.Total)
            .ThenBy(o => o.Id, StringComparer.Ordinal)
            .Select(o => new OrderReportLine(
                o.Id,
                o.CustomerId,
                o.Status.ToString(),
                o.Total))
            .ToList();
    }

    public static IReadOnlyList<CustomerSummary> TopCustomers(
        IEnumerable<Order> source, int n)
    {
        return source
            .GroupBy(o => o.CustomerId)
            .Select(g => new CustomerSummary(
                g.Key,
                g.Count(),
                g.Sum(o => o.Total)))
            .OrderByDescending(c => c.Total)
            .ThenBy(c => c.CustomerId, StringComparer.Ordinal)
            .Take(n)
            .ToList();
    }

    public static IReadOnlyList<GroupSummary> ByStatus(
        IEnumerable<Order> source)
    {
        return source
            .GroupBy(o => o.Status)
            .Select(g => new GroupSummary(
                g.Key.ToString(),
                g.Count(),
                g.Sum(o => o.Total)))
            .OrderBy(g => g.Status, StringComparer.Ordinal)
            .ToList();
    }

    public static (
        int Items,
        decimal Total,
        decimal Average,
        decimal Max) Totals(IEnumerable<Order> source)
    {
        var list = source.ToList();

        if (list.Count == 0)
            return (0, 0m, 0m, 0m);

        return (
            list.Count,
            list.Sum(o => o.Total),
            list.Average(o => o.Total),
            list.Max(o => o.Total)
        );
    }


public static IReadOnlyList<OrderProductSummary> JoinOrdersProducts(
    IEnumerable<Order> orders,
    IEnumerable<Product> products)
{
    return orders
        .SelectMany(o => o.Lines, (order, line) => new
        {
            OrderId = order.Id,
            line.ProductId,
            line.Quantity,
            line.Price
        })
        .Join(
            products,
            line => line.ProductId,
            product => product.Id,
            (line, product) => new OrderProductSummary(
                line.OrderId,
                product.Name,
                line.Quantity,
                line.Price * line.Quantity))
        .OrderBy(x => x.OrderId)
        .ThenBy(x => x.ProductName)
        .ToList();
}


public static IReadOnlyList<OrderReportLine> MinimumTotalOrders(
    IEnumerable<Order> source, decimal minimum)
{
    return source
        .WithMinimumTotal(minimum)
        .OrderBy(o => o.Id)
        .Select(o => new OrderReportLine(
            o.Id,
            o.CustomerId,
            o.Status.ToString(),
            o.Total))
        .ToList();
}

public static IReadOnlyList<OrderReportLine> ConfirmedOrdersQuery(
    IEnumerable<Order> source)
{
    var query =
        from order in source
        where order.Status == OrderStatus.Confirmed
        orderby order.Total descending, order.Id
        select new OrderReportLine(
            order.Id,
            order.CustomerId,
            order.Status.ToString(),
            order.Total);

    return query.ToList();
}

}
