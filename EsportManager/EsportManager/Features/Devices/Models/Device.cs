using CliWrap;
using MudBlazor;
using System.Net;

namespace EsportManager.Features.Devices.Models;

public class Device
{
    public Device(IPAddress iPAddress, string macAdress)
    {
        Name = string.Empty;
        IPAddress = iPAddress ?? throw new ArgumentNullException(nameof(iPAddress));
        MACAdress = macAdress ?? throw new ArgumentNullException(nameof(macAdress));

        Status = new DeviceStatus();
        Process = null;
    }

    public Device(string csvLine)
    {
        string[] values = csvLine.Split(',');

        if (!string.IsNullOrEmpty(values[0]))
        {
            IPAddress = IPAddress.Parse(values[0]);
        } 
        else
        {
            IPAddress = IPAddress.None;
        }

        Name = values[1];
        MACAdress = values[2];

        Status = new DeviceStatus();
        Process = null;
    }

    public string Name { get; set; }
    public IPAddress IPAddress { get; set; }
    public string MACAdress { get; set; }
    public DeviceStatus Status { get; set; }
    public DeviceProcess? Process { get; set; }
    public bool ShowDetails { get; set; }

    public async Task TurnOn(ISnackbar snackbar)
    {
        try
        {
            var result = await Cli.Wrap($"WakeMeOnLan.exe")
                .WithWorkingDirectory(Environment.CurrentDirectory)
                .WithArguments($"/wakeup {MACAdress}")
                .ExecuteAsync();

            snackbar.Add("Pc turning on", Severity.Info);
        }
        catch (Exception e)
        {
            snackbar.Add(e.Message, Severity.Error);
        }
    }

    public void UpdateFortnite(CancellationToken cancellationToken, Func<Task> processChangedCallback)
    {
        ShowDetails = true;

        string sharedFolderPath = GetSharedFolderPath();

        Directory.CreateDirectory($@"{Environment.CurrentDirectory}\Logs");
        string logPath = $@"{Environment.CurrentDirectory}\Logs\{Name}_Fortnite_{DateTime.Now.Date.ToShortDateString()}.txt";

        var cmd = Cli.Wrap($"robocopy")
            .WithWorkingDirectory(Environment.CurrentDirectory)
            .WithArguments($@"\\HVKSERVER\Fortnite {sharedFolderPath} /MIR /Z /FFT /XF *.mancpn *.manifest")
            .WithValidation(CommandResultValidation.None);

        Process = new(cmd, processChangedCallback, logPath);
        Process.StartProcess(cancellationToken);
    }

    public string GetSharedFolderPath()
    {
        return $@"\\{IPAddress.ToString()}\Fortnite";
    }
}