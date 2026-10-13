using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class CachingOrderStore : IOrderStore
{
    private readonly IOrderStore _inner;

    private readonly Dictionary<string, Order> _cache =
        new(StringComparer.OrdinalIgnoreCase);

    public CachingOrderStore(IOrderStore inner)
    {
        _inner = inner
            ?? throw new ArgumentNullException(nameof(inner));
    }

    public IReadOnlyList<Order> List()
    {
        IReadOnlyList<Order> orders =
            _inner.List();

        foreach (Order order in orders)
            _cache[order.Id] = order;

        return orders;
    }

    public Order? GetById(string id)
    {
        if (_cache.TryGetValue(id, out Order? order))
            return order;

        order = _inner.GetById(id);

        if (order is not null)
            _cache[id] = order;

        return order;
    }

    public void Add(Order order)
    {
        _inner.Add(order);
        _cache[order.Id] = order;
    }

    public void Update(Order order)
    {
        _inner.Update(order);
        _cache[order.Id] = order;
    }

    public bool Remove(string id)
    {
        bool removed =
            _inner.Remove(id);

        if (removed)
            _cache.Remove(id);

        return removed;
    }
}