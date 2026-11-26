using Core.Abstractions;
using Core.Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class EfOrderStore(AppDbContext db) : IAsyncOrderStore
{
    public async Task<IReadOnlyList<Order>> ListAsync(
        CancellationToken ct = default)
    {
        var rows = await db.Orders
            .AsNoTracking()
            .Include(x => x.Lines)
            .OrderBy(x => x.Id)
            .ToListAsync(ct);

        return rows.Select(ToDomain).ToList();
    }

    public async Task<Order?> GetByIdAsync(
        string id, CancellationToken ct = default)
    {
        var row = await db.Orders
            .AsNoTracking()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        return row is null ? null : ToDomain(row);
    }

    public async Task AddAsync(
        Order order, CancellationToken ct = default)
    {
        db.Orders.Add(ToRow(order));

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is SqliteException
            { SqliteExtendedErrorCode: 1555 or 2067 })
        {
            throw new DomainException(
                "Замовлення з таким ID вже існує", ex);
        }
    }

    public async Task UpdateAsync(
        Order order, CancellationToken ct = default)
    {
        var row = await db.Orders
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == order.Id, ct);

        if (row is null)
            throw new KeyNotFoundException("Замовлення не знайдене.");

        row.CustomerId = order.CustomerId;
        row.Status = order.Status.ToString();
        row.CreatedAt = order.CreatedAt;

        db.OrderLines.RemoveRange(row.Lines.ToList());
        row.Lines.Clear();

        foreach (var line in order.Lines)
        {
            row.Lines.Add(new OrderLineRow
            {
                OrderId = order.Id,
                ProductId = line.ProductId,
                Name = line.Name,
                Price = line.Price,
                Quantity = line.Quantity
            });
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> RemoveAsync(
        string id, CancellationToken ct = default)
    {
        var row = await db.Orders
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (row is null)
            return false;

        db.Orders.Remove(row);
        await db.SaveChangesAsync(ct);

        return true;
    }

    private static OrderRow ToRow(Order order) => new()
    {
        Id = order.Id,
        CustomerId = order.CustomerId,
        Status = order.Status.ToString(),
        CreatedAt = order.CreatedAt,
        Lines = order.Lines.Select(x => new OrderLineRow
        {
            OrderId = order.Id,
            ProductId = x.ProductId,
            Name = x.Name,
            Price = x.Price,
            Quantity = x.Quantity
        }).ToList()
    };

    private static Order ToDomain(OrderRow row)
    {
        var dto = new Core.Dto.OrderDto(
            row.Id,
            row.CustomerId,
            row.Status,
            row.Lines.OrderBy(x => x.Id)
                .Select(x => new Core.Dto.OrderLineDto(
                    x.ProductId,
                    x.Name,
                    x.Price,
                    x.Quantity))
                .ToList(),
            row.CreatedAt);

        return Order.FromDto(dto);
    }
}
