using System.Text;
using Core.Domain;
using Core.Dto;
using Core.Import;

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