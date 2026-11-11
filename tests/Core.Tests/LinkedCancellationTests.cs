using Xunit;

namespace Core.Tests;

public class LinkedCancellationTests
{
    [Fact]
    public async Task TimeoutCancelsLinkedToken()
    {
        using var userCts = new CancellationTokenSource();
        using var timeoutCts = new CancellationTokenSource();

        using var linkedCts =
            CancellationTokenSource.CreateLinkedTokenSource(
                userCts.Token,
                timeoutCts.Token);

        timeoutCts.CancelAfter(20);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () =>
                await Task.Delay(
                    Timeout.Infinite,
                    linkedCts.Token));

        Assert.True(timeoutCts.IsCancellationRequested);
        Assert.True(linkedCts.IsCancellationRequested);
        Assert.False(userCts.IsCancellationRequested);
    }
}
