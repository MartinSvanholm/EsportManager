namespace EsportManager;

public class LaunchSettings
{
    private const string DefaultServer = "HVKSERVER";

    public string Server { get; }

    public LaunchSettings()
    {
        Server = ParseServer(Environment.GetCommandLineArgs());
    }

    private static string ParseServer(string[] args)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i].Equals("--server", StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }

        return DefaultServer;
    }
}
