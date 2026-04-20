using Serilog;

namespace EsportManager.Features.Process;

public class ProcessLoggerFactory
{
    private readonly string _logDirectory = Path.Combine(Environment.CurrentDirectory, "Logs");

    public Serilog.ILogger CreateUpdateFortniteLogger(string deviceName)
    {
        return CreateLogger(deviceName, "UpdateFortnite");
    }

    public Serilog.ILogger CreateLogger(string deviceName, string processName)
    {
        Directory.CreateDirectory(_logDirectory);

        string sanitizedDeviceName = SanitizeFileName(deviceName);
        string sanitizedProcessName = SanitizeFileName(processName);
        string date = DateTime.Now.ToString("yyyy-MM-dd");

        string logPath = Path.Combine(_logDirectory, $"{sanitizedDeviceName}_{sanitizedProcessName}_{date}.txt");

        return new LoggerConfiguration()
            .WriteTo.File(logPath)
            .CreateLogger();
    }

    private static string SanitizeFileName(string name)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", name.Split(invalidChars));
    }
}