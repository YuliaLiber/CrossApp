using System.Text;
using Core.Dto;
using Core.Import;


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