using System.Text.Json;
using Core.Domain;
using Core.Import;
using Core.Services;
using Core.Storage;

public static class Lab8Runner
{
    private class ConsoleProgress : IProgress<int>
    {
        public void Report(int value) =>
            Console.WriteLine($"Прогрес: {value}%");
    }

    public static async Task<int> RunAsync(string[] args)
    {
        Console.WriteLine("=== Лабораторна робота №8 ===");

        string path = args.FirstOrDefault(x => x.EndsWith(".json"))
            ?? "data/lab8-order-lines.json";

        int? timeout = null;
        string? option = args.FirstOrDefault(x =>
            x.StartsWith("--timeout-ms="));

        if (option != null)
        {
            if (!int.TryParse(
                    option["--timeout-ms=".Length..], out int ms)
                || ms < 0)
            {
                Console.WriteLine("Некоректний тайм-аут.");
                return 1;
            }

            timeout = ms;
        }

        using var userCts = new CancellationTokenSource();
        using var timeoutCts = new CancellationTokenSource();
        using var linkedCts =
            CancellationTokenSource.CreateLinkedTokenSource(
                userCts.Token, timeoutCts.Token);

        ConsoleCancelEventHandler handler = (_, e) =>
        {
            e.Cancel = true;
            userCts.Cancel();
        };

        Console.CancelKeyPress += handler;

        try
        {
            if (timeout == 0)
                timeoutCts.Cancel();
            else if (timeout.HasValue)
                timeoutCts.CancelAfter(timeout.Value);

            if (args.Contains("--cancel"))
                userCts.Cancel();

            var service = new OrderService(new InMemoryOrderStore());
            var order = service.CreateOrder("O-LAB8", "C-LAB8");

            int events = 0;
            service.Changed += (_, _) => events++;

            var importer = new JsonOrderLineImporter();
            int added = 0;

            if (args.Contains("--stream"))
            {
                Console.WriteLine("Режим: потоковий імпорт");

                await foreach (var line in
                    importer.StreamAsync(path, linkedCts.Token))
                {
                    linkedCts.Token.ThrowIfCancellationRequested();

                    service.AddLine(
                        order.Id, line.ProductId,
                        line.Name, line.Price, line.Quantity);

                    added++;
                }
            }
            else
            {
                IProgress<int>? progress = args.Contains("--progress")
                    ? new ConsoleProgress()
                    : null;

                added = await service.ImportAsync(
                    importer, order.Id, path,
                    linkedCts.Token, progress);
            }

            Console.WriteLine($"Імпортовано записів: {added}");
            Console.WriteLine($"Рядків у замовленні: {order.Lines.Count}");
            Console.WriteLine($"Сума замовлення: {order.Total:F2} грн");
            Console.WriteLine($"Подій Changed: {events}");

            return 0;
        }
        catch (OperationCanceledException)
        {
            string message = timeoutCts.IsCancellationRequested &&
                             !userCts.IsCancellationRequested
                ? "Імпорт скасовано через тайм-аут."
                : "Імпорт скасовано.";

            Console.WriteLine(message);
            return 0;
        }
        catch (DomainException ex)
        {
            Console.WriteLine($"Дані відхилено: {ex.Message}");
            return 1;
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"Файл не знайдено: {ex.FileName}");
            return 1;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Помилка JSON: {ex.Message}");
            return 1;
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Помилка файлу: {ex.Message}");
            return 1;
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Некоректний аргумент: {ex.Message}");
            return 1;
        }
        finally
        {
            Console.CancelKeyPress -= handler;
        }
    }
}
