using Core;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;

Console.OutputEncoding = Encoding.UTF8;

EnvironmentReport report = EnvironmentInfo.Collect();

bool jsonMode = args.Contains("--json");

if (jsonMode)
{
    var jsonOptions = new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

     Console.WriteLine(JsonSerializer.Serialize(report, jsonOptions));
}
else
{
    Console.WriteLine("CrossApp – інформація про середовище");
    Console.WriteLine(new string('-', 52));

    Console.WriteLine($"ОС             : {report.OsDescription}");
    Console.WriteLine($"Runtime        : {report.FrameworkDescription}");
    Console.WriteLine($"Архітектура    : {report.ProcessArchitecture}");
    Console.WriteLine($"RID (визначено): {report.DetectedRid}");
    Console.WriteLine($"RID (від .NET) : {report.ReportedRid}");
    Console.WriteLine($"Каталог        : {report.BaseDirectory}");
    Console.WriteLine($"Збірка         : {report.BuildNote}");
}