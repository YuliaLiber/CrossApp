
using System.Text.Json;
using Core.Domain;
using Core.Storage;
using Xunit;

namespace Core.Tests;

public class Lab9StorageTests
{
    private static string NewPath()
    {
        return Path.Combine(
            Path.GetTempPath(),
            "crossapp-tests",
            Guid.NewGuid().ToString("N"),
            "orders.json");
    }

    [Fact]
    public void SaveAndReload_PreservesOrder()
    {
        string path = NewPath();

        var store = new FileOrderStore(path);

        var order = Order.Create("O-TEST9", "C-TEST9");
        order.AddLine("P-1", "Стіл", 1500m, 2);

        store.Add(order);

        var reloaded = new FileOrderStore(path);
        var saved = reloaded.GetById("O-TEST9");

        Assert.NotNull(saved);
        Assert.Single(saved.Lines);
        Assert.Equal(3000m, saved.Total);
    }

    [Fact]
    public void Export_PreservesUkrainianText()
    {
        string path = NewPath();

        var store = new FileOrderStore(path);
        var order = Order.Create("O-UKR", "C-UKR");

        order.AddLine(
            "P-2", "Крісло м'яке", 2500m, 1);

        store.Add(order);

        string json = File.ReadAllText(path);

        Assert.Contains("Крісло м'яке", json);
        Assert.Contains("\"customerId\"", json);
        Assert.Contains("\"lines\"", json);
    }

    [Fact]
    public void AtomicSave_DoesNotLeaveTempFile()
    {
        string path = NewPath();

        var store = new FileOrderStore(path);
        var order = Order.Create("O-TMP", "C-TMP");

        store.Add(order);

        Assert.True(File.Exists(path));
        Assert.False(File.Exists(path + ".tmp"));
    }

    [Fact]
    public void InvalidJson_ThrowsJsonException()
    {
        string path = NewPath();

        Directory.CreateDirectory(
            Path.GetDirectoryName(path)!);

        File.WriteAllText(path, "{\"broken\":");

        var store = new FileOrderStore(path);

        Assert.Throws<JsonException>(() => store.List());
    }
}
