namespace Core.Events;

public sealed class OrderChangedEventArgs : EventArgs
{
    public string OrderId { get; }
    public OrderChangeKind Kind { get; }
    public int LinesCount { get; }
    public decimal Total { get; }
    public DateTimeOffset At { get; }

    public OrderChangedEventArgs(
        string orderId,
        OrderChangeKind kind,
        int linesCount,
        decimal total)
    {
        OrderId = orderId;
        Kind = kind;
        LinesCount = linesCount;
        Total = total;
        At = DateTimeOffset.Now;
    }
}