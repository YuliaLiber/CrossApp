using Core.Domain;
using Core.Import;
using Core.Reports;
using Core.Services;
using Core.Storage;
using Xunit;

namespace Core.Tests;

public class UnitTest1
{
    [Fact]
    public void Confirm_AlreadyConfirmed_ThrowsDomainException()
    {
        var order = Order.Create("O-1", "C-1");
        order.AddLine("F-1", "Стіл", 1000m, 1);
        order.Confirm();

        Assert.Throws<DomainException>(() => order.Confirm());
        Assert.Equal(OrderStatus.Confirmed, order.Status);
        Assert.Equal(1000m, order.Total);
    }

    [Fact]
    public void AddLine_InvalidQuantity_KeepsState()
    {
        var order = Order.Create("O-2", "C-2");

        Assert.ThrowsAny<ArgumentException>(() =>
            order.AddLine("F-1", "Стіл", 1000m, -1));

        Assert.Empty(order.Lines);
        Assert.Equal(0m, order.Total);
    }

    [Fact]
    public void Changed_RaisesOnlyAfterSuccess()
    {
        var service = new OrderService(new InMemoryOrderStore());
        var order = service.CreateOrder("O-3", "C-3");

        int events = 0;
        service.Changed += (_, _) => events++;

        service.AddLine(order.Id, "F-1", "Стіл", 1000m, 1);
        Assert.Equal(1, events);

        service.Confirm(order.Id);
        Assert.Equal(2, events);

        Assert.Throws<DomainException>(() =>
            service.AddLine(order.Id, "F-2", "Комод", 2000m, 1));

        Assert.Equal(2, events);
        Assert.Single(order.Lines);
    }

    [Fact]
    public void AddLine_CalculatesCorrectTotal()
    {
        var order = Order.Create("O-4", "C-4");
        order.AddLine("F-1", "Тумба", 1890m, 2);
        order.AddLine("F-2", "Комод", 4690m, 1);

        Assert.Equal(2, order.Lines.Count);
        Assert.Equal(8470m, order.Total);
    }

    [Theory]
    [InlineData(1000, 3)]
    [InlineData(5000, 2)]
    [InlineData(6000, 1)]
    [InlineData(9000, 0)]
    public void ExpensiveOrders_FiltersCorrectly(
        int threshold, int expected)
    {
        var result = ReportService.ExpensiveOrders(
            TestData.Items(), threshold);

        Assert.Equal(expected, result.Count);
    }

    [Fact]
    public void TopCustomers_HasStableOrder()
    {
        var orders = new List<Order>
        {
            TestData.MakeOrder("O-2", "C-2", 5000m),
            TestData.MakeOrder("O-1", "C-1", 5000m),
            TestData.MakeOrder("O-3", "C-3", 1000m)
        };

        var first = ReportService.TopCustomers(orders, 2)
            .Select(x => x.CustomerId).ToArray();

        var second = ReportService.TopCustomers(orders, 2)
            .Select(x => x.CustomerId).ToArray();

        Assert.Equal(new[] { "C-1", "C-2" }, first);
        Assert.Equal(first, second);
    }

    [Fact]
    public async Task ImportAsync_AlreadyCancelled_Throws()
    {
        var service = new OrderService(new InMemoryOrderStore());
        var order = service.CreateOrder("O-7", "C-7");

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportAsync(
                new JsonOrderLineImporter(),
                order.Id, "unused.json", cts.Token));

        Assert.Empty(order.Lines);
    }

    [Fact]
    public async Task LoadAsync_InvalidLine_ThrowsDomainException()
    {
        string path = Path.Combine(
            Path.GetTempPath(),
            $"lab8-{Guid.NewGuid():N}.json");

        try
        {
            await File.WriteAllTextAsync(
                path,
                """[{"ProductId":"F-1","Name":"Стіл","Price":1000,"Quantity":0}]""",
                CancellationToken.None);

            var importer = new JsonOrderLineImporter();

            var ex = await Assert.ThrowsAsync<DomainException>(
                () => importer.LoadAsync(path, CancellationToken.None));

            Assert.Contains("Запис 1", ex.Message);
            Assert.IsAssignableFrom<ArgumentException>(
                ex.InnerException);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}

internal static class TestData
{
    public static Order MakeOrder(
        string id, string customerId, decimal total)
    {
        var order = Order.Create(id, customerId);
        order.AddLine("F-1", "Тестовий товар", total, 1);
        return order;
    }

    public static IReadOnlyList<Order> Items() => new List<Order>
    {
        MakeOrder("O-101", "C-101", 3000m),
        MakeOrder("O-102", "C-102", 6000m),
        MakeOrder("O-103", "C-103", 9000m)
    };
}
