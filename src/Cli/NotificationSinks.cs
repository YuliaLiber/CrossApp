using Core.Abstractions;

public class ConsoleSink : INotificationSink
{
    public void Notify(string message)
    {
        Console.WriteLine(message);
    }
}

public class FileSink : INotificationSink
{
    private readonly string _path;

    public FileSink(string path)
    {
        _path = path;
    }

    public void Notify(string message)
    {
        Directory.CreateDirectory(
            Path.GetDirectoryName(_path)!);

        File.AppendAllText(
            _path, message + Environment.NewLine);
    }
}