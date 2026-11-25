using Core.Domain;

namespace Core.Abstractions;

public interface IAsyncOrderStore
{
    Task<IReadOnlyList<Order>> ListAsync(
        CancellationToken ct = default);

    Task<Order?> GetByIdAsync(
        string id, CancellationToken ct = default);

    Task AddAsync(
        Order order, CancellationToken ct = default);

    Task UpdateAsync(
        Order order, CancellationToken ct = default);

    Task<bool> RemoveAsync(
        string id, CancellationToken ct = default);
}
