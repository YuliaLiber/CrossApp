
using Core.Abstractions;
using Core.Domain;
using Core.Events;

namespace Core.Services;

public sealed class OrderService
{
    private readonly IOrderStore _store;
    private readonly INotificationSink? _sink;

    public event EventHandler<OrderChangedEventArgs>? Changed;

    public OrderService(IOrderStore store, INotificationSink? sink = null)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _sink = sink;
    }

    private void OnChanged(Order order, OrderChangeKind kind)
    {
        Changed?.Invoke(this,
            new OrderChangedEventArgs(
                order.Id, kind, order.Lines.Count, order.Total));

        _sink?.Notify($"{kind}: {order.Id}, сума: {order.Total}");
    }

    public Order CreateOrder(string id, string customerId)
    {
        var order = Order.Create(id, customerId);
        _store.Add(order);
        OnChanged(order, OrderChangeKind.OrderCreated);
        return order;
    }

    public void AddLine(string orderId, string productId,
        string name, decimal price, int quantity)
    {
        var order = GetRequired(orderId);
        order.AddLine(productId, name, price, quantity);
        _store.Update(order);
        OnChanged(order, OrderChangeKind.LineAdded);
    }

    public void Confirm(string orderId)
    {
        var order = GetRequired(orderId);
        order.Confirm();
        _store.Update(order);
        OnChanged(order, OrderChangeKind.OrderConfirmed);
    }

    public void Cancel(string orderId)
    {
        var order = GetRequired(orderId);
        order.Cancel();
        _store.Update(order);
    }

    public IReadOnlyList<Order> All() => _store.List();

    public Order? Find(string id) => _store.GetById(id);

    public bool Remove(string id) => _store.Remove(id);

    public List<Order> FindAll(Func<Order, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _store.List().Where(predicate).ToList();
    }

    // ЛР8: асинхронний імпорт з підтримкою скасування і прогресу
    public async Task<int> ImportAsync(
        IOrderLineImporter importer,
        string orderId,
        string path,
        CancellationToken ct = default,
        IProgress<int>? progress = null)
    {
        ArgumentNullException.ThrowIfNull(importer);
        ArgumentException.ThrowIfNullOrWhiteSpace(orderId);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        ct.ThrowIfCancellationRequested();

        var order = GetRequired(orderId);
        var lines = await importer.LoadAsync(path, ct);

        progress?.Report(0);

        for (int i = 0; i < lines.Count; i++)
        {
            ct.ThrowIfCancellationRequested();

            var line = lines[i];

            AddLine(order.Id, line.ProductId,
                line.Name, line.Price, line.Quantity);

            progress?.Report((i + 1) * 100 / lines.Count);
        }

        if (lines.Count == 0)
            progress?.Report(100);

        return lines.Count;
    }

    private Order GetRequired(string id) =>
        _store.GetById(id) ??
        throw new InvalidOperationException(
            $"Замовлення з id={id} не знайдено.");
}
