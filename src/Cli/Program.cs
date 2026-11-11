using System.Text;
using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;
using Core.Import;
using Core.Services;
using Core.Storage;
using Core.Events;
using Core.Reports;
using System.Diagnostics;

if (args.Length > 0 && args[0] == "--lab8")
    return await Lab8Runner.RunAsync(args);

string? reportCommand = args.FirstOrDefault(a =>
    a is "report-top" or "report-high" or
         "report-groups" or "report-sum" or
         "report-deferred" or "report-empty" or
         "report-join" or "report-extension" or
         "report-query" or "report-benchmark");

if (reportCommand != null)
{
    Console.WriteLine("=== Лабораторна робота №7 ===");

    var items = SampleData.ReportOrders();

    void PrintGroups(IReadOnlyList<GroupSummary> groups)
    {
        foreach (var g in groups)
            Console.WriteLine(
                $"{g.Status,-10} | Замовлень: {g.Count,2} | " +
                $"Сума: {g.Total:F2} грн");
    }

    switch (reportCommand)
    {
        case "report-top":
        {
            Console.WriteLine("\n--- Топ-5 клієнтів ---");

            foreach (var c in ReportService.TopCustomers(items, 5))
                Console.WriteLine(
                    $"{c.CustomerId,-10} | Замовлень: {c.OrdersCount,2} | " +
                    $"Сума: {c.Total:F2} грн");

            break;
        }

        case "report-high":
        {
            Console.WriteLine("\n--- Замовлення дорожчі за 5000 грн ---");

            foreach (var o in ReportService.ExpensiveOrders(items, 5000m))
                Console.WriteLine(
                    $"{o.OrderId,-8} | {o.CustomerId,-8} | " +
                    $"{o.Status,-10} | {o.Total:F2} грн");

            break;
        }

        case "report-groups":
        {
            Console.WriteLine("\n--- Групування за статусом ---");

            PrintGroups(ReportService.ByStatus(items));
            break;
        }

        case "report-sum":
        {
            Console.WriteLine("\n--- Загальна статистика ---");

            var t = ReportService.Totals(items);

            Console.WriteLine($"Кількість: {t.Items}");
            Console.WriteLine($"Загальна сума: {t.Total:F2} грн");
            Console.WriteLine($"Середня сума: {t.Average:F2} грн");
            Console.WriteLine($"Максимальна сума: {t.Max:F2} грн");

            break;
        }

        case "report-deferred":
        {
            Console.WriteLine("\n--- Відкладене виконання LINQ ---");

            var source = items.ToList();
            var query = ReportService.DeferredExpensiveOrders(source, 5000m);

            Console.WriteLine($"До додавання: {query.Count()}");

            var order = Order.Create("O-999", "C-999");
            order.AddLine("F-999", "Новий стіл", 6000m, 1);
            source.Add(order);

            Console.WriteLine($"Після додавання: {query.Count()}");

            var snapshot = query.ToList();
            source.Clear();

            Console.WriteLine($"Після очищення джерела: {query.Count()}");
            Console.WriteLine($"Збережений результат ToList(): {snapshot.Count}");

            break;
        }

        case "report-empty":
        {
            Console.WriteLine("\n--- Перевірка порожнього набору ---");

            var empty = new List<Order>();
            var stats = ReportService.Totals(empty);

            Console.WriteLine($"Топ клієнтів: {ReportService.TopCustomers(empty, 5).Count}");
            Console.WriteLine($"Дорогих замовлень: {ReportService.ExpensiveOrders(empty, 5000m).Count}");
            Console.WriteLine($"Груп за статусом: {ReportService.ByStatus(empty).Count}");
            Console.WriteLine($"Кількість: {stats.Items}");
            Console.WriteLine($"Сума: {stats.Total:F2}");
            Console.WriteLine($"Середнє: {stats.Average:F2}");
            Console.WriteLine($"Максимум: {stats.Max:F2}");

            break;
        }

        case "report-join":
        {
            Console.WriteLine("\n--- Об'єднання замовлень і товарів через Join ---");

            var products = SampleData.ReportProducts();
            var joined = ReportService.JoinOrdersProducts(items, products);

            foreach (var p in joined)
                Console.WriteLine(
                    $"{p.OrderId,-8} | {p.ProductName,-28} | " +
                    $"{p.Quantity} шт. | {p.Total:F2} грн");

            Console.WriteLine($"\nЗнайдено позицій: {joined.Count}");
            break;
        }

        case "report-extension":
        {
            Console.WriteLine("\n--- Власний метод розширення LINQ ---");

            var orders = ReportService.MinimumTotalOrders(items, 5000m);

            foreach (var o in orders)
                Console.WriteLine(
                    $"{o.OrderId,-8} | {o.CustomerId,-8} | {o.Total:F2} грн");

            Console.WriteLine($"\nЗнайдено замовлень: {orders.Count}");
            break;
        }

        case "report-query":
        {
            Console.WriteLine("\n--- LINQ Query Syntax: групування ---");

            PrintGroups(ReportService.ByStatusQuery(items));
            break;
        }

        case "report-benchmark":
        {
            Console.WriteLine("\n--- Порівняння ToList() у LINQ ---");

            const int size = 100000;
            var bigOrders = new List<Order>(size);

            for (int i = 1; i <= size; i++)
            {
                var order = Order.Create($"B-{i}", $"C-{i % 100}");
                order.AddLine("F-001", "Тумба", 1890m, (i - 1) % 5 + 1);
                bigOrders.Add(order);
            }

            var watch = Stopwatch.StartNew();

            var without = ReportService.ExpensiveOrders(bigOrders, 5000m);
            watch.Stop();
            double withoutTime = watch.Elapsed.TotalMilliseconds;

            watch.Restart();

            var with = ReportService.ExpensiveOrdersWithToList(bigOrders, 5000m);
            watch.Stop();
            double withTime = watch.Elapsed.TotalMilliseconds;

            Console.WriteLine($"Згенеровано записів: {bigOrders.Count}");
            Console.WriteLine(
                $"Без проміжного ToList(): {without.Count} записів, {withoutTime:F2} мс");
            Console.WriteLine(
                $"З проміжним ToList(): {with.Count} записів, {withTime:F2} мс");

            break;
        }
    }

    return 0;
}



if (args.Contains("--lab6"))
{
    Console.WriteLine("=== Лабораторна робота №6 ===");

    string dir = Path.Combine(AppContext.BaseDirectory, "logs");
    Directory.CreateDirectory(dir);

    string logPath = Path.Combine(dir, "app.log");
    string sinkPath = Path.Combine(dir, "sink.log");

    File.WriteAllText(logPath, "");
    File.WriteAllText(sinkPath, "");

    IOrderStore store = new InMemoryOrderStore();
    INotificationSink sink = new FileSink(sinkPath);
    OrderService service = new OrderService(store, sink);


    void OnChanged(object? sender, OrderChangedEventArgs e)
    {
        string text = $"{e.At:HH:mm:ss} | {e.Kind,-15} | " +
                      $"{e.OrderId} | Позицій: {e.LinesCount} | " +
                      $"Сума: {e.Total:0.00} грн";

        Console.WriteLine(text);
        File.AppendAllText(logPath, text + Environment.NewLine);
    }

    service.Changed += OnChanged;


    int changes = 0;
    service.Changed += (_, _) => changes++;

    Console.WriteLine("\n--- Створення замовлення ---");

    Order order = service.CreateOrder("O-601", "C-601");
    service.AddLine(order.Id, "P-101", "Стіл", 3500m, 1);
    service.AddLine(order.Id, "P-102", "Стілець", 1200m, 2);
    service.Confirm(order.Id);

    Console.WriteLine($"\nКількість подій: {changes}");

    Console.WriteLine("\n--- Перевірка інваріанта ---");

int beforeInvalid = changes;

try
{
    service.AddLine("O-601", "P-103", "Шафа", 2000m, 0);
}
catch (Exception ex)
{
    Console.WriteLine($"Помилка: {ex.Message}");
}

Console.WriteLine($"Нових подій: {changes - beforeInvalid}");


    Console.WriteLine("\n--- Відписка ---");

    service.Changed -= OnChanged;
    service.CreateOrder("O-602", "C-602");

    Console.WriteLine($"Подій після відписки: {changes}");
    Console.WriteLine($"Записів у app.log: {File.ReadAllLines(logPath).Length}");


    Console.WriteLine("\n--- Сценарій відмови ---");

    int before = changes;

    try
    {
        service.Confirm("O-NOT-FOUND");
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"Помилка: {ex.Message}");
    }

    Console.WriteLine($"Нових подій: {changes - before}");


    Console.WriteLine("\n--- Пошук через Func ---");

    foreach (Order o in service.FindAll(o => o.Total >= 3000m))
        Console.WriteLine($"{o.Id} | Сума: {o.Total:0.00} грн");

   
    Console.WriteLine("\n--- Додаткове 1: ручний DI ---");

    Console.WriteLine(
        $"Повідомлень у sink.log: {File.ReadAllLines(sinkPath).Length}");

    INotificationSink consoleSink = new ConsoleSink();
    OrderService otherService = new OrderService(
        new InMemoryOrderStore(), consoleSink);

    otherService.CreateOrder("O-603", "C-603");


    Console.WriteLine("\n--- Додаткове 2: Action<string> ---");

    string csvPath = Path.Combine(
        AppContext.BaseDirectory, "orders-lab6.csv");

    File.WriteAllLines(csvPath, new[]
    {
        "O-701;C-701",
        "неправильний рядок",
        "O-702;C-702",
        "O-703;"
    });

    List<Order> ImportOrders(string file, Action<string> onSkipped)
    {
        List<Order> result = new();

        foreach (string line in File.ReadLines(file))
        {
            string[] p = line.Split(';');

            if (p.Length != 2 ||
                string.IsNullOrWhiteSpace(p[0]) ||
                string.IsNullOrWhiteSpace(p[1]))
            {
                onSkipped($"Пропущено: {line}");
                continue;
            }

            result.Add(Order.Create(p[0].Trim(), p[1].Trim()));
        }

        return result;
    }

    int skipped = 0;

    var imported = ImportOrders(csvPath, message =>
    {
        skipped++;
        Console.WriteLine(message);
    });

    Console.WriteLine($"Імпортовано: {imported.Count}");
    Console.WriteLine($"Пропущено: {skipped}");

    Console.WriteLine("\n=== Лабораторну завершено ===");

    return 0;
}

static void TryDo(string title, Action action)
{
    try
    {
        action();

        Console.WriteLine(
            $"{title}: виняток НЕ спрацював");
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            $"{title}: {ex.GetType().Name} — {ex.Message}");
    }
}


if (args.Length > 0 && args[0] == "--lab4")
{
    Console.WriteLine("=== Лабораторна робота №4 ===");
    Console.WriteLine();

    Console.WriteLine("=== Сценарій 1: успіх ===");

    Order order = Order.Create(
        "O-001",
        "C-001");


    order.AddLine(
        "F-001",
        "Тумба приліжкова Nord",
        1890.00m,
        2);

    order.AddLine(
        "F-002",
        "Комод Lora 4 шухляди",
        4690.00m,
        1);

    Console.WriteLine(order);

    foreach (OrderLine line in order.Lines)
    {
        Console.WriteLine($"  {line}");
    }

    order.Confirm();

    Console.WriteLine(
        $"Після підтвердження: {order}");

    Console.WriteLine();


    OrderDto dto = order.ToDto();

    Order restoredOrder =
        Order.FromDto(dto);

    Console.WriteLine(
        $"Відновлено з DTO: {restoredOrder}");

    Console.WriteLine();


    Console.WriteLine(
        "=== Сценарій 2: порушення інваріантів ===");

    Order testOrder = Order.Create(
        "O-002",
        "C-002");


    TryDo(
        "Підтвердження порожнього замовлення",
        () => testOrder.Confirm());


    TryDo(
        "Нульова кількість товару",
        () => testOrder.AddLine(
            "F-003",
            "Журнальний столик Loft Oak",
            2790.00m,
            0));


    TryDo(
        "Від'ємна ціна",
        () => testOrder.AddLine(
            "F-004",
            "Крісло Mellow Beige",
            -100.00m,
            1));


    TryDo(
        "Порожній ProductId",
        () => testOrder.AddLine(
            "",
            "Полиця настінна Line 120",
            1590.00m,
            1));


    testOrder.AddLine(
        "F-005",
        "Полиця настінна Line 120",
        1590.00m,
        1);

    testOrder.Confirm();


    TryDo(
        "Додавання рядка після підтвердження",
        () => testOrder.AddLine(
            "F-006",
            "Стіл",
            5000.00m,
            1));


    TryDo(
        "Повторне підтвердження",
        () => testOrder.Confirm());


    Console.WriteLine();

    Console.WriteLine(
        $"Стан після всіх відмов: {testOrder}");

    Console.WriteLine(
        "Після помилкових операцій об'єкт залишився у коректному стані.");

Console.WriteLine();
Console.WriteLine(
    "=== Додаткове 1: ImportResult з ЛР3 ===");

ImportResult<ProductDto> week3Import =
    new ImportResult<ProductDto>(
        [
            new ProductDto(
                "F-100",
                "Коректний товар",
                1000m),

            new ProductDto(
                "",
                "Товар без Id",
                500m),

            new ProductDto(
                "F-102",
                "Товар з від'ємною ціною",
                -10m)
        ],
        []);

ImportResult<OrderLine> domainImport =
    Week3ImportAdapter.ToOrderLines(
        week3Import);

Console.WriteLine(
    $"Створено сутностей: {domainImport.Items.Count}");

Console.WriteLine(
    $"Не пройшли інваріанти: {domainImport.Errors.Count}");

foreach (string error in domainImport.Errors)
{
    Console.WriteLine(
        $"! {error}");
}

Console.WriteLine();
Console.WriteLine(
    "=== Додаткове 2: правило між Order та Product ===");

Order furnitureOrder =
    Order.Create(
        "O-FURNITURE",
        "C-500");

Product availableProduct =
    Product.Create(
        "F-001",
        "Тумба приліжкова Nord",
        1890m,
        true);

Product unavailableProduct =
    Product.Create(
        "F-003",
        "Журнальний столик Loft Oak",
        2790m,
        false);

OrderPolicy.AddProductToOrder(
    furnitureOrder,
    availableProduct,
    1);

Console.WriteLine(
    $"Доступний товар додано: {availableProduct.Name}");

TryDo(
    "Спроба додати недоступний товар",
    () => OrderPolicy.AddProductToOrder(
        furnitureOrder,
        unavailableProduct,
        1));

Console.WriteLine(
    $"Кількість рядків у замовленні: {furnitureOrder.Lines.Count}");

Console.WriteLine();
Console.WriteLine(
    "=== Додаткове 3: OrderStatus ===");

Order statusOrder =
    Order.Create(
        "O-STATUS",
        "C-STATUS");

Console.WriteLine(
    $"Початковий статус: {statusOrder.Status}");

statusOrder.AddLine(
    "F-200",
    "Тестовий товар",
    500m,
    1);

statusOrder.Confirm();

Console.WriteLine(
    $"Після Confirm: {statusOrder.Status}");

statusOrder.Cancel();

Console.WriteLine(
    $"Після Cancel: {statusOrder.Status}");

TryDo(
    "Спроба Confirm після Cancel",
    () => statusOrder.Confirm());

    return 0;
}

bool runLab5 =
    args.Length == 0 ||
    args.Contains("--file") ||
    args.Contains("--memory") ||
    args.Contains("--cache");

if (runLab5)
{
    Console.WriteLine(
        "=== Лабораторна робота №5 ===");
    Console.WriteLine();

    string dataPath =
        Path.Combine(
            AppContext.BaseDirectory,
            "data",
            "orders.json");

   IOrderStore store = args.Contains("--file")
    ? new FileOrderStore(dataPath)
    : new InMemoryOrderStore(SampleData.Orders());

if (args.Contains("--cache"))
    store = new CachingOrderStore(store);

    OrderService service =
        new OrderService(store);

    Console.WriteLine(
        $"Сховище: {store.GetType().Name}");

    if (args.Contains("--file"))
        Console.WriteLine(
            $"Файл: {dataPath}");

    Console.WriteLine();

    string orderId =
        "O-" +
        Guid.NewGuid()
            .ToString("N")[..6]
            .ToUpperInvariant();

    Order created =
        service.CreateOrder(
            orderId,
            "C-500");

    service.AddLine(
        orderId,
        "F-001",
        "Тумба приліжкова Nord",
        1890m,
        1);

    service.AddLine(
        orderId,
        "F-002",
        "Комод Lora 4 шухляди",
        4690m,
        1);

    service.Confirm(orderId);

    Console.WriteLine(
        $"Створено: {created}");

    Console.WriteLine();
    Console.WriteLine(
        "=== Список замовлень ===");

    foreach (Order order
             in service.All().Take(8))
    {
        Console.WriteLine(order);
    }

    Console.WriteLine();

    Console.WriteLine(
        "=== Пошук за id ===");

    Console.WriteLine(
        service.Find(orderId));

    Console.WriteLine();

    Console.WriteLine(
        "=== Додаткове: пошук через Func ===");

    foreach (Order order in
             service
                 .FindAll(x => x.Total >= 3000)
                 .Take(5))
    {
        Console.WriteLine(order);
    }

    Console.WriteLine();

    Console.WriteLine(
        "=== Сценарій відмови ===");

    try
    {
        service.Confirm(
            "O-NOT-FOUND");
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            $"{ex.GetType().Name} — {ex.Message}");
    }

    return 0;
}

if (args.Length > 0 && args[0] == "--mixed")
{
    string mixedPath = args.Length > 1
        ? args[1]
        : Path.Combine("data", "mixed-sample.txt");

    if (!File.Exists(mixedPath))
    {
        Console.WriteLine(
            $"Файл не знайдено: {Path.GetFullPath(mixedPath)}");
        return 1;
    }

    string[] lines = File.ReadAllLines(
        mixedPath,
        Encoding.UTF8);

    foreach (string line in lines)
    {
        MixedRecord? record = MixedLineParser.Parse(line);

        switch (record)
        {
            case ProductMixed product:
                Console.WriteLine(
                    $"Товар: {product.Value.Id} | " +
                    $"{product.Value.Name} | " +
                    $"{product.Value.Price:F2}");
                break;

            case WarehouseMixed warehouse:
                Console.WriteLine(
                    $"Склад: {warehouse.Value.Id} | " +
                    $"{warehouse.Value.Name}");
                break;

            default:
                Console.WriteLine(
                    $"Не вдалося розпізнати: {line}");
                break;
        }
    }

    return 0;
}


string path = args.Length > 0
    ? args[0]
    : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine(
        $"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

string extension =
    Path.GetExtension(path).ToLowerInvariant();

ImportResult<ProductDto> result = extension switch
{
    ".csv" => ProductCsvImporter.Load(path),

    ".json" => ProductJsonImporter.Load(path),

    _ => new ImportResult<ProductDto>(
        [],
        [$"Непідтримуваний формат файлу: {extension}"])
};


Console.WriteLine(
    $"Завантажено записів: {result.Items.Count}");

foreach (ProductDto p in result.Items.Take(5))
{
    Console.WriteLine(
        $"{p.Id,-8} {p.Name,-26} {p.Price,10:F2}");
}

if (result.Errors.Count > 0)
{
    Console.WriteLine(
        $"Пропущено рядків: {result.Errors.Count}");

    foreach (string e in result.Errors)
    {
        Console.WriteLine($"! {e}");
    }
}


int total =
    result.Items.Count + result.Errors.Count;

double errorPercent = total > 0
    ? result.Errors.Count * 100.0 / total
    : 0;

Console.WriteLine(
    $"Усього: {total} | " +
    $"Прийнято: {result.Items.Count} | " +
    $"Пропущено: {result.Errors.Count} | " +
    $"Помилок: {errorPercent:F2}%");

return 0;

static class StoreFactory
{
    public static IOrderStore Create(
        string[] args,
        string dataPath)
    {
        if (args.Contains("--file"))
            return new FileOrderStore(dataPath);

        return new InMemoryOrderStore(
            SampleData.Orders());
    }
}