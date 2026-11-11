
using System.Runtime.CompilerServices;
using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Import;

public sealed class JsonOrderLineImporter : IOrderLineImporter
{
    public async Task<IReadOnlyList<OrderLineDto>> LoadAsync(
        string path, CancellationToken ct = default)
    {
        var result = new List<OrderLineDto>();

        await foreach (var line in StreamAsync(path, ct))
            result.Add(line);

        return result;
    }

    public async IAsyncEnumerable<OrderLineDto> StreamAsync(
        string path,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ct.ThrowIfCancellationRequested();

        await using var stream = new FileStream(
            path, FileMode.Open, FileAccess.Read,
            FileShare.Read, 4096, useAsync: true);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        int number = 0;

        await foreach (var line in
            JsonSerializer.DeserializeAsyncEnumerable<OrderLineDto?>(
                stream, options, cancellationToken: ct))
        {
            ct.ThrowIfCancellationRequested();
            number++;

            if (line is null)
                throw new DomainException(
                    $"Запис {number}: порожній рядок замовлення");

            try
            {
                OrderLine.Create(
                    line.ProductId, line.Name,
                    line.Price, line.Quantity);
            }
            catch (ArgumentException ex)
            {
                throw new DomainException(
                    $"Запис {number}: {ex.Message}", ex);
            }

            yield return line;
        }
    }
}
