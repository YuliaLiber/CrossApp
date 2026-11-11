
using Core.Abstractions;
using Core.Domain;
using Core.Dto;
using Core.Services;
using Core.Storage;
using Xunit;

namespace Core.Tests;

public class ImportCancellationTests
{
    [Fact]
    public async Task ImportAsync_CancelDuringImport_StopsProcessing()
    {
        var service = new OrderService(new InMemoryOrderStore());
        var order = service.CreateOrder("O-100", "C-100");

        using var cts = new CancellationTokenSource();

        int events = 0;

        service.Changed += (_, _) =>
        {
            events++;
            cts.Cancel();
        };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportAsync(
                new FakeImporter(),
                order.Id,
                "unused.json",
                cts.Token));

        Assert.Single(order.Lines);
        Assert.Equal(1000m, order.Total);
        Assert.Equal(1, events);
    }

    private class FakeImporter : IOrderLineImporter
    {
        public Task<IReadOnlyList<OrderLineDto>> LoadAsync(
            string path,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            IReadOnlyList<OrderLineDto> lines =
            [
                new("F-1", "Стіл", 1000m, 1),
                new("F-2", "Комод", 2000m, 1),
                new("F-3", "Шафа", 3000m, 1)
            ];

            return Task.FromResult(lines);
        }
    }
}
