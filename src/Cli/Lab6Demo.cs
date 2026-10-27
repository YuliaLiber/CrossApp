using Core.Abstractions;
using Core.Domain;
using Core.Events;
using Core.Services;
using Core.Storage;

public static class Lab6Demo
{
    public static void Run()
    {
        Console.WriteLine("=== Лабораторна робота №6 ===");

        IOrderStore store = new InMemoryOrderStore();
        OrderService service = new OrderService(store);

        string logPath = Path.Combine(
            AppContext.BaseDirectory,
            "logs",
            "app.log"
        );

        Directory.CreateDirectory(
            Path.GetDirectoryName(logPath)!
        );

        void OnOrderChanged(
            object? sender,
            OrderChangedEventArgs e)
        {
            string line =
                $"{e.At:HH:mm:ss} " +
                $"{e.Kind,-15} " +
                $"{e.OrderId} " +
                $"позицій: {e.LinesCount} " +
                $"сума: {e.Total}";

            Console.WriteLine(line);

            File.AppendAllText(
                logPath,
                line + Environment.NewLine
            );
        }

        service.Changed += OnOrderChanged;

        int changes = 0;

        service.Changed += (_, _) => changes++;

        Console.WriteLine("\n=== Створення замовлення ===");

        Order order = service.CreateOrder(
            "O-601",
            "C-601"
        );

        service.AddLine(
            order.Id,
            "P-101",
            "Стіл",
            3500m,
            1
        );

        service.AddLine(
            order.Id,
            "P-102",
            "Стілець",
            1200m,
            2
        );

        service.Confirm(order.Id);

        Console.WriteLine(
            $"\nПодій отримано: {changes}"
        );

        Console.WriteLine("\n=== Відписка ===");

        service.Changed -= OnOrderChanged;

        service.CreateOrder("O-602", "C-602");

        Console.WriteLine(
            $"Подій отримано: {changes}"
        );

        Console.WriteLine("\n=== Сценарій відмови ===");

        int before = changes;

        try
        {
            service.Confirm("O-NOT-FOUND");
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"{ex.GetType().Name}: {ex.Message}"
            );
        }

        Console.WriteLine(
            $"Кількість нових подій: {changes - before}"
        );

        Console.WriteLine("\n=== Пошук через Func ===");

        IReadOnlyList<Order> found = service.Search(
            o => o.Total >= 3000m
        );

        foreach (Order item in found)
        {
            Console.WriteLine(
                $"{item.Id} | Сума: {item.Total}"
            );
        }

        Console.WriteLine("\n=== Готово ===");
        Console.WriteLine($"Лог: {logPath}");
    }
}