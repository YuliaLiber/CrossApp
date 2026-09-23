using System.Text;
using System.Text.Json;
using Core.Dto;

namespace Core.Import;

public static class ProductJsonImporter
{
    public static ImportResult<ProductDto> Load(string path)
    {
        try
        {
            string json = File.ReadAllText(path, Encoding.UTF8);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var items =
                JsonSerializer.Deserialize<List<ProductDto>>(json, options) ?? [];

            return new ImportResult<ProductDto>(
                items,
                Array.Empty<string>());
        }
        catch (JsonException ex)
        {
            return new ImportResult<ProductDto>(
                Array.Empty<ProductDto>(),
                new[] { $"Помилка JSON: {ex.Message}" });
        }
    }
}