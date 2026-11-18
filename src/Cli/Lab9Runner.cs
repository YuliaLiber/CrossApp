
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Core.Configuration;
using Core.Domain;
using Core.Storage;
using Microsoft.Extensions.Configuration;

public static class Lab9Runner
{
    public static int Run(string[] args)
    {
        try
        {
            string environment =
                Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                ?? "Development";

            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile(
                    $"appsettings.{environment}.json",
                    optional: true);

            if (environment == "Development")
            {
                builder.AddUserSecrets(
                    Assembly.GetExecutingAssembly(),
                    optional: true);
            }

            string[] configArgs = args
                .Where(a =>
                    a.StartsWith("--Storage:", StringComparison.OrdinalIgnoreCase) ||
                    a.StartsWith("--Orders:", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            var configuration = builder
                .AddEnvironmentVariables()
                .AddEnvironmentVariables(prefix: "CROSSAPP_")
                .AddCommandLine(configArgs)
                .Build();

            var options =
                configuration.Get<AppOptions>() ?? new AppOptions();

            string? overridePath =
                Environment.GetEnvironmentVariable("CROSSAPP_DATA_PATH");

            bool hasCliPath = configArgs.Any(a =>
                a.StartsWith(
                    "--Storage:DataPath=",
                    StringComparison.OrdinalIgnoreCase));

            var storageOptions = new StorageOptions
            {
                DataPath = hasCliPath
                    ? options.Storage.DataPath
                    : overridePath ?? options.Storage.DataPath,

                FileName = options.Storage.FileName
            };

            string filePath = storageOptions.ResolveFullPath(
                AppContext.BaseDirectory);

            string? apiKey = configuration["Integration:ApiKey"];

            Console.WriteLine("=== Лабораторна робота №9 ===");
            Console.WriteLine($"Середовище: {environment}");
            Console.WriteLine(
                $"Робочий каталог: {Environment.CurrentDirectory}");
            Console.WriteLine(
                $"Каталог застосунку: {AppContext.BaseDirectory}");
            Console.WriteLine($"Файл даних: {filePath}");
            Console.WriteLine($"Валюта: {options.Orders.Currency}");

            Console.WriteLine(
                apiKey is null
                    ? "ApiKey: не заданий"
                    : $"ApiKey: присутній, довжина {apiKey.Length}");

            var store = new FileOrderStore(filePath);

            if (args.Contains("--import"))
            {
                var orders = store.List();

                Console.WriteLine(
                    $"Імпортовано замовлень: {orders.Count}");

                foreach (var order in orders)
                {
                    Console.WriteLine(order);
                }

                return 0;
            }

            if (args.Contains("--export"))
            {
                if (store.GetById("O-LAB9") is null)
                {
                    var order1 = Order.Create("O-LAB9", "C-LAB9");

                    order1.AddLine(
                        "P-901", "Стіл дерев'яний", 3500m, 2);

                    order1.AddLine(
                        "P-902", "Крісло м'яке", 2500m, 1);

                    store.Add(order1);
                }

                if (store.GetById("O-LAB9-2") is null)
                {
                    var order2 = Order.Create("O-LAB9-2", "C-LAB9-2");

                    order2.AddLine(
                        "P-903", "Шафа для одягу", 4200m, 1);

                    order2.AddLine(
                        "P-904", "Полиця настінна", 800m, 3);

                    store.Add(order2);
                }

                if (store.GetById("O-LAB9-3") is null)
                {
                    var order3 = Order.Create("O-LAB9-3", "C-LAB9-3");

                    order3.AddLine(
                        "P-905", "Диван розкладний", 12500m, 1);

                    order3.AddLine(
                        "P-906", "Журнальний столик", 1800m, 1);

                    store.Add(order3);
                }

                Console.WriteLine(
                    $"Експортовано замовлень: {store.List().Count}");

                var reloaded = new FileOrderStore(filePath);

                Console.WriteLine(
                    $"Повторно прочитано: {reloaded.List().Count}");

                Console.WriteLine(
                    $"Тимчасовий файл залишився: " +
                    $"{File.Exists(filePath + ".tmp")}");

                return 0;
            }

            Console.WriteLine("Використай --export або --import.");
            return 0;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Помилка JSON: {ex.Message}");
            return 1;
        }
        catch (Exception ex) when (
            ex is IOException or
            UnauthorizedAccessException or
            ArgumentException or
            InvalidOperationException)
        {
            Console.WriteLine($"Помилка: {ex.Message}");
            return 1;
        }
    }
}
