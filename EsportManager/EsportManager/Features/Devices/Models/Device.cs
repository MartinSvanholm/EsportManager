using CliWrap;
using CliWrap.EventStream;
using MudBlazor;
using System.Diagnostics;
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
    private string logPath => $@"{Environment.CurrentDirectory}\Logs\{Name}_Fortnite_{DateTime.Now.Date.ToShortDateString()}.txt";

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

    public async Task UpdateFortnite(CancellationToken cancellationToken, ISnackbar snackbar, Action processChangedCallback)
    {
        try
        {
            ShowDetails = true;

            string sharedFolderPath = GetSharedFolderPath();

            Directory.CreateDirectory($@"{Environment.CurrentDirectory}\Logs");

            var cmd = Cli.Wrap($"robocopy")
                .WithWorkingDirectory(Environment.CurrentDirectory)
                .WithArguments($@"\\HVKSERVER\Fortnite {sharedFolderPath} /MIR /XF *.mancpn *.manifest");

            await foreach (var cmdEvent in cmd.ListenAsync(cancellationToken))
            {
                switch (cmdEvent)
                {
                    case StartedCommandEvent started:
                        Process = new DeviceProcess(started.ProcessId);
                        Process.ProcessChanged += processChangedCallback;
                        Process?.SetMessage($"Process started; ID: {started.ProcessId}", logPath);
                        Debug.WriteLine($"Process started; ID: {started.ProcessId}");
                        break;
                    case StandardOutputCommandEvent stdOut:
                        Process?.SetMessage(stdOut.Text, logPath);
                        Debug.WriteLine($"Out> {stdOut.Text}");
                        if (stdOut.Text.Contains("ERROR 1326") || stdOut.Text.Contains("ERROR 1909"))
                        {
                            Process?.CancelProcess(logPath);
                            Process?.ProcessChanged -= processChangedCallback;
                        }
                        break;
                    case StandardErrorCommandEvent stdErr:
                        Process?.SetErrorMessage(stdErr.Text, logPath);
                        break;
                    case ExitedCommandEvent exited:
                        Debug.WriteLine($"Process exited; Code: {exited.ExitCode}");
                        break;
                }
            }

            snackbar.Add("updating fortnite", Severity.Info);
        }
        catch (OperationCanceledException)
        {
            Process?.CancelProcess(logPath);
            Process?.ProcessChanged -= processChangedCallback;
        }
        catch (Exception e)
        {
            snackbar.Add(e.Message, Severity.Error);
        }
    }

    public string GetSharedFolderPath()
    {
        return $@"\\{IPAddress.ToString()}\Fortnite";
    }
}