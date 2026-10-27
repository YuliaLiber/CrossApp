using Core.Abstractions;
using Core.Domain;
using Core.Events;

namespace Core.Services;

public sealed class OrderService
{
    private readonly IOrderStore _store;

public event EventHandler<OrderChangedEventArgs>? Changed;

private void OnChanged(Order order, OrderChangeKind kind)
{
    Changed?.Invoke(
        this,
        new OrderChangedEventArgs(
            order.Id,
            kind,
            order.Lines.Count,
            order.Total
        )
    );
}

    public OrderService(IOrderStore store)
    {
        _store = store
            ?? throw new ArgumentNullException(nameof(store));
    }

   public Order CreateOrder(string id, string customerId)
{
    Order order = Order.Create(id, customerId);

    _store.Add(order);

    OnChanged(order, OrderChangeKind.OrderCreated);

    return order;
}

   public void AddLine(
    string orderId,
    string productId,
    string name,
    decimal price,
    int quantity)
{
    Order order = GetRequired(orderId);

    order.AddLine(productId, name, price, quantity);

    _store.Update(order);

    OnChanged(order, OrderChangeKind.LineAdded);
}

   public void Confirm(string orderId)
{
    Order order = GetRequired(orderId);

    order.Confirm();

    _store.Update(order);

    OnChanged(order, OrderChangeKind.OrderConfirmed);
}

    public void Cancel(string orderId)
    {
        Order order = GetRequired(orderId);

        order.Cancel();

        _store.Update(order);
    }

    public IReadOnlyList<Order> All()
    {
        return _store.List();
    }

    public Order? Find(string id)
    {
        return _store.GetById(id);
    }

    public bool Remove(string id)
    {
        return _store.Remove(id);
    }

    public IReadOnlyList<Order> Search(
        Func<Order, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return _store
            .List()
            .Where(predicate)
            .ToList();
    }

    private Order GetRequired(string id)
    {
        return _store.GetById(id)
            ?? throw new InvalidOperationException(
                $"Замовлення з id={id} не знайдено.");
    }
}