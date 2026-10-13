using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class InMemoryOrderStore : IOrderStore
{
    private readonly Dictionary<string, Order> _items;

    public InMemoryOrderStore(IEnumerable<Order>? seed = null)
    {
        _items = (seed ?? [])
            .ToDictionary(
                x => x.Id,
                StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<Order> List()
    {
        return _items.Values.ToList();
    }

    public Order? GetById(string id)
    {
        return _items.GetValueOrDefault(id);
    }

    public void Add(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        if (_items.ContainsKey(order.Id))
            throw new InvalidOperationException(
                $"Замовлення з id={order.Id} вже існує.");

        _items.Add(order.Id, order);
    }

    public void Update(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        if (!_items.ContainsKey(order.Id))
            throw new InvalidOperationException(
                $"Замовлення з id={order.Id} не знайдено.");

        _items[order.Id] = order;
    }

    public bool Remove(string id)
    {
        return _items.Remove(id);
    }
}