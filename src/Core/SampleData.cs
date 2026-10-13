using Core.Domain;

namespace Core;

public static class SampleData
{
    public static IReadOnlyList<Order> Orders()
    {
        var furniture = new[]
        {
            ("F-001", "Тумба приліжкова Nord", 1890m),
            ("F-002", "Комод Lora 4 шухляди", 4690m),
            ("F-003", "Журнальний столик Loft Oak", 2790m),
            ("F-004", "Крісло Mellow Beige", 6490m),
            ("F-005", "Полиця настінна Line 120", 1590m)
        };

        List<Order> orders = [];

        for (int i = 1; i <= 15; i++)
        {
            Order order = Order.Create(
                $"O-{100 + i}",
                $"C-{100 + i}");

            var product =
                furniture[(i - 1) % furniture.Length];

            order.AddLine(
                product.Item1,
                product.Item2,
                product.Item3,
                i % 2 + 1);

            if (i % 3 == 0)
                order.Confirm();

            orders.Add(order);
        }

        return orders;
    }
}