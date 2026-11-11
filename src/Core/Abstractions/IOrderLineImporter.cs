using Core.Dto;

namespace Core.Abstractions;

public interface IOrderLineImporter
{
    Task<IReadOnlyList<OrderLineDto>> LoadAsync(
        string path,
        CancellationToken ct = default);
}