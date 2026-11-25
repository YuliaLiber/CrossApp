using Api.Contracts;
using Api.Validation;
using Core.Abstractions;
using Core.Domain;

namespace Api.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrders(this WebApplication app)
    {
        var group = app.MapGroup("/api/orders")
            .WithTags("Orders");

        group.MapGet("", GetOrdersAsync)
            .WithName("GetOrders")
            .Produces<PagedOrdersDto>(200)
            .ProducesValidationProblem();

        group.MapGet("/{id}", GetOrderAsync)
            .WithName("GetOrderById")
            .Produces<OrderDto>(200)
            .Produces(404);

        group.MapGet("/{id}/total", GetTotalAsync)
            .WithName("GetOrderTotal")
            .Produces<OrderTotalDto>(200)
            .Produces(404);

        group.MapPost("", CreateOrderAsync)
            .WithName("CreateOrder")
            .Produces<OrderDto>(201)
            .ProducesValidationProblem()
            .AddEndpointFilter<SaveOrderValidationFilter>();

        group.MapPut("/{id}", UpdateOrderAsync)
            .WithName("UpdateOrder")
            .Produces<OrderDto>(200)
            .Produces(404)
            .Produces(409)
            .ProducesValidationProblem()
            .AddEndpointFilter<SaveOrderValidationFilter>();

        group.MapPost("/{id}/confirm", ConfirmOrderAsync)
            .WithName("ConfirmOrder")
            .Produces<OrderDto>(200)
            .Produces(404)
            .ProducesProblem(409);

        group.MapDelete("/{id}", DeleteOrderAsync)
            .WithName("DeleteOrder")
            .Produces(204)
            .Produces(404);
    }

    private static async Task<IResult> GetOrdersAsync(
        IAsyncOrderStore store,
        int? page,
        int? pageSize,
        string? search,
        CancellationToken ct)
    {
        int p = page ?? 1;
        int size = pageSize ?? 20;

        if (p < 1 || size < 1 || size > 100)
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["page"] = ["Номер сторінки має бути додатним."],
                    ["pageSize"] = ["Розмір сторінки: від 1 до 100."]
                });

        var orders = await store.ListAsync(ct);

        var query = orders.OrderBy(x => x.Id).AsEnumerable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x =>
                x.Id.Contains(search, StringComparison.OrdinalIgnoreCase)
                || x.CustomerId.Contains(search, StringComparison.OrdinalIgnoreCase));

        int total = query.Count();

        var items = query
            .Skip((p - 1) * size)
            .Take(size)
            .Select(ToDto)
            .ToList();

        return Results.Ok(new PagedOrdersDto(items, total));
    }

    private static async Task<IResult> GetOrderAsync(
        string id, IAsyncOrderStore store, CancellationToken ct)
    {
        var order = await store.GetByIdAsync(id, ct);

        return order is null
            ? Results.NotFound()
            : Results.Ok(ToDto(order));
    }

    private static async Task<IResult> GetTotalAsync(
        string id, IAsyncOrderStore store, CancellationToken ct)
    {
        var order = await store.GetByIdAsync(id, ct);

        return order is null
            ? Results.NotFound()
            : Results.Ok(new OrderTotalDto(order.Id, order.Total));
    }

    private static async Task<IResult> CreateOrderAsync(
        SaveOrderRequest? request,
        IAsyncOrderStore store,
        CancellationToken ct)
    {
        if (!TryValidate(request, out var errors))
            return Results.ValidationProblem(errors);

        string id = "O-" + Guid.NewGuid()
            .ToString("N")[..12].ToUpperInvariant();

        var order = BuildOrder(id, request!);

        await store.AddAsync(order, ct);

        return Results.Created($"/api/orders/{id}", ToDto(order));
    }

    private static async Task<IResult> UpdateOrderAsync(
        string id,
        SaveOrderRequest? request,
        IAsyncOrderStore store,
        CancellationToken ct)
    {
        if (!TryValidate(request, out var errors))
            return Results.ValidationProblem(errors);

        var existing = await store.GetByIdAsync(id, ct);

        if (existing is null)
            return Results.NotFound();

        if (existing.Status != OrderStatus.Draft)
            return Results.Problem(
                title: "Замовлення не можна змінити",
                detail: "Редагування дозволено лише у стані Draft.",
                statusCode: 409);

        var updated = BuildOrder(id, request!, existing.CreatedAt);

        await store.UpdateAsync(updated, ct);

        return Results.Ok(ToDto(updated));
    }

    private static async Task<IResult> ConfirmOrderAsync(
        string id, IAsyncOrderStore store, CancellationToken ct)
    {
        var order = await store.GetByIdAsync(id, ct);

        if (order is null)
            return Results.NotFound();

        order.Confirm();

        await store.UpdateAsync(order, ct);

        return Results.Ok(ToDto(order));
    }

    private static async Task<IResult> DeleteOrderAsync(
        string id, IAsyncOrderStore store, CancellationToken ct)
    {
        bool removed = await store.RemoveAsync(id, ct);

        return removed ? Results.NoContent() : Results.NotFound();
    }

    private static Order BuildOrder(
        string id,
        SaveOrderRequest request,
        DateTimeOffset? createdAt = null)
    {
        var order = Order.Create(id, request.Customer!, createdAt);

        foreach (var line in request.Lines!)
            order.AddLine(
                line!.ProductId!,
                line.Name!,
                line.Price,
                line.Quantity);

        return order;
    }

    private static OrderDto ToDto(Order order)
    {
        return new OrderDto(
            order.Id,
            order.CustomerId,
            order.Status.ToString(),
            order.Total,
            order.Lines.Select(x => new OrderLineDto(
                x.ProductId,
                x.Name,
                x.Price,
                x.Quantity)).ToList(),
            order.CreatedAt);
    }

    private static bool TryValidate(
        SaveOrderRequest? request,
        out Dictionary<string, string[]> errors)
    {
        errors = new Dictionary<string, string[]>();

        if (request is null)
        {
            errors["body"] = ["Тіло запиту обов'язкове."];
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.Customer)
            || request.Customer.Length > 100)
            errors["customer"] =
                ["Клієнт обов'язковий, максимум 100 символів."];

        if (request.Lines is null || request.Lines.Count == 0)
        {
            errors["lines"] =
                ["Замовлення повинно містити хоча б один товар."];
        }
        else
        {
            for (int i = 0; i < request.Lines.Count; i++)
            {
                var line = request.Lines[i];

                if (line is null)
                {
                    errors[$"lines[{i}]"] =
                        ["Позиція не може бути null."];
                    continue;
                }

                if (string.IsNullOrWhiteSpace(line.ProductId)
                    || line.ProductId.Length > 100)
                    errors[$"lines[{i}].productId"] =
                        ["Артикул обов'язковий, максимум 100 символів."];

                if (string.IsNullOrWhiteSpace(line.Name)
                    || line.Name.Length > 200)
                    errors[$"lines[{i}].name"] =
                        ["Назва обов'язкова, максимум 200 символів."];

                if (line.Price < 0)
                    errors[$"lines[{i}].price"] =
                        ["Ціна не може бути від'ємною."];

                if (line.Quantity <= 0)
                    errors[$"lines[{i}].quantity"] =
                        ["Кількість має бути більшою за нуль."];
            }
        }

        return errors.Count == 0;
    }
}
