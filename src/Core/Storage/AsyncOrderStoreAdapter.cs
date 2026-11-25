using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class AsyncOrderStoreAdapter : IAsyncOrderStore
{
    private readonly IOrderStore _store;

    public AsyncOrderStoreAdapter(IOrderStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<Order>> ListAsync(
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        lock (_store)
            return Task.FromResult(_store.List());
    }

    public Task<Order?> GetByIdAsync(
        string id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        lock (_store)
            return Task.FromResult(_store.GetById(id));
    }

    public Task AddAsync(
        Order order, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        lock (_store)
            _store.Add(order);

        return Task.CompletedTask;
    }

    public Task UpdateAsync(
        Order order, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        lock (_store)
            _store.Update(order);

        return Task.CompletedTask;
    }

    public Task<bool> RemoveAsync(
        string id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        lock (_store)
            return Task.FromResult(_store.Remove(id));
    }
}
