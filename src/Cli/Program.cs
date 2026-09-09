using System.Runtime.InteropServices;
using System.Text.Json;

bool jsonMode = args.Contains("--json");

var info = new
{
    Title = "CrossApp – практикум з крос-платформного програмування",
    Student = "Юлія Лібер",
    Group = "ФЕІ-36",
    OSDescription = RuntimeInformation.OSDescription,
    OSVersion = Environment.OSVersion.ToString(),
    ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
    DotNetVersion = Environment.Version.ToString(),
    Runtime = RuntimeInformation.FrameworkDescription,
    AppDirectory = AppContext.BaseDirectory,
    CurrentDirectory = Environment.CurrentDirectory,
    Domain = "Замовлення",
    Entities = new[]
    {
        "Customer",
        "Product",
        "Order",
        "OrderLine"
    }
};

if (jsonMode)
{
    Console.WriteLine(JsonSerializer.Serialize(info));
}
else
{
    Console.WriteLine(info.Title);
    Console.WriteLine($"Студент: {info.Student}, група {info.Group}");
    Console.WriteLine(new string('-', 52));

    Console.WriteLine($"ОС (OSDescription)   : {info.OSDescription}");
    Console.WriteLine($"ОС (Environment)     : {info.OSVersion}");
    Console.WriteLine($"Архітектура процесу  : {info.ProcessArchitecture}");
    Console.WriteLine($"Версія .NET (CLR)    : {info.DotNetVersion}");
    Console.WriteLine($"Runtime              : {info.Runtime}");
    Console.WriteLine($"Каталог застосунку   : {info.AppDirectory}");
    Console.WriteLine($"Поточний каталог     : {info.CurrentDirectory}");

    Console.WriteLine(new string('-', 52));
    Console.WriteLine("Предметна область: Замовлення (клієнти, товари, замовлення, рядки замовлення)");
}