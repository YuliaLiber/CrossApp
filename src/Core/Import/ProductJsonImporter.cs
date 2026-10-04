using System.Text;
using System.Text.Json;
using Core.Dto;

namespace Core.Import;

public static class ProductJsonImporter
{
    public static ImportResult<ProductDto> Load(string path)
    {
        var items = new List<ProductDto>();
        var errors = new List<string>();

        try
        {
            string json = File.ReadAllText(path, Encoding.UTF8);

            using JsonDocument document = JsonDocument.Parse(json);

            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                errors.Add("JSON повинен містити масив товарів");
                return new ImportResult<ProductDto>(items, errors);
            }

            int number = 0;

            foreach (JsonElement element in document.RootElement.EnumerateArray())
            {
                number++;

                if (element.ValueKind != JsonValueKind.Object)
                {
                    errors.Add($"запис {number}: очікується об'єкт товару");
                    continue;
                }

                if (!TryGetProperty(element, "Id", out JsonElement idElement) ||
                    idElement.ValueKind != JsonValueKind.String ||
                    string.IsNullOrWhiteSpace(idElement.GetString()))
                {
                    errors.Add($"запис {number}: Id порожній або відсутній");
                    continue;
                }

                if (!TryGetProperty(element, "Name", out JsonElement nameElement) ||
                    nameElement.ValueKind != JsonValueKind.String ||
                    string.IsNullOrWhiteSpace(nameElement.GetString()))
                {
                    errors.Add($"запис {number}: назва порожня або відсутня");
                    continue;
                }

                if (!TryGetProperty(element, "Price", out JsonElement priceElement) ||
                    !priceElement.TryGetDecimal(out decimal price) ||
                    price < 0)
                {
                    errors.Add(
                        $"запис {number}: ціна не є коректним невід'ємним числом");
                    continue;
                }

                string id = idElement.GetString()!;
                string name = nameElement.GetString()!;

                items.Add(new ProductDto(id, name, price));
            }
        }
        catch (JsonException ex)
        {
            errors.Add($"Помилка JSON: {ex.Message}");
        }

        return new ImportResult<ProductDto>(items, errors);
    }

    private static bool TryGetProperty(
        JsonElement element,
        string propertyName,
        out JsonElement value)
    {
        foreach (JsonProperty property in element.EnumerateObject())
        {
            if (string.Equals(
                property.Name,
                propertyName,
                StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }
}