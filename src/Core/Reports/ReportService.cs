
using Core.Domain;

namespace Core.Reports;

public static class ReportService
{
    private static OrderReportLine ToLine(Order o) =>
        new(o.Id, o.CustomerId, o.Status.ToString(), o.Total);

    public static IReadOnlyList<OrderReportLine> ExpensiveOrders(
        IEnumerable<Order> source, decimal threshold)
    {
        return DeferredExpensiveOrders(source, threshold)
            .OrderByDescending(o => o.Total)
            .ThenBy(o => o.OrderId)
            .ToList();
    }

    public static IReadOnlyList<CustomerSummary> TopCustomers(
        IEnumerable<Order> source, int n, decimal minimum = 0m)
    {
        return source
            .WithMinimumTotal(minimum)
            .GroupBy(o => o.CustomerId)
            .Select(g => new CustomerSummary(
                g.Key, g.Count(), g.Sum(o => o.Total)))
            .OrderByDescending(c => c.Total)
            .ThenBy(c => c.CustomerId)
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
            .OrderBy(g => g.Status)
            .ToList();
    }

    public static TotalsSummary Totals(IEnumerable<Order> source)
    {
        var list = source.ToList();

        if (list.Count == 0)
            return new TotalsSummary(0, 0, 0, 0);

        return new TotalsSummary(
            list.Count,
            list.Sum(o => o.Total),
            list.Average(o => o.Total),
            list.Max(o => o.Total));
    }

    // Додаткове 1: Join
    public static IReadOnlyList<OrderProductSummary> JoinOrdersProducts(
        IEnumerable<Order> orders, IEnumerable<Product> products)
    {
        return orders
            .SelectMany(o => o.Lines, (o, line) => new
            {
                o.Id,
                line.ProductId,
                line.Quantity,
                line.Price
            })
            .Join(products,
                line => line.ProductId,
                p => p.Id,
                (line, p) => new OrderProductSummary(
                    line.Id,
                    p.Name,
                    line.Quantity,
                    line.Price * line.Quantity))
            .OrderBy(x => x.OrderId)
            .ThenBy(x => x.ProductName)
            .ToList();
    }

    // Додаткове 2: власний метод розширення
    public static IReadOnlyList<OrderReportLine> MinimumTotalOrders(
        IEnumerable<Order> source, decimal minimum)
    {
        return source
            .WithMinimumTotal(minimum)
            .OrderBy(o => o.Id)
            .Select(ToLine)
            .ToList();
    }

   

    // Додаткове 3: Query Syntax з group into
    public static IReadOnlyList<GroupSummary> ByStatusQuery(
        IEnumerable<Order> source)
    {
        var query =
            from o in source
            group o by o.Status into g
            orderby g.Key
            select new GroupSummary(
                g.Key.ToString(),
                g.Count(),
                g.Sum(o => o.Total));

        return query.ToList();
    }

    // Додаткове 4: варіант із проміжним ToList
    public static IReadOnlyList<OrderReportLine> ExpensiveOrdersWithToList(
        IEnumerable<Order> source, decimal threshold)
    {
        return DeferredExpensiveOrders(source, threshold)
            .ToList()
            .OrderByDescending(o => o.Total)
            .ThenBy(o => o.OrderId)
            .ToList();
    }

    // Демонстрація відкладеного виконання
    public static IEnumerable<OrderReportLine> DeferredExpensiveOrders(
        IEnumerable<Order> source, decimal threshold)
    {
        return source
            .Where(o => o.Total > threshold)
            .Select(ToLine);
    }
}
