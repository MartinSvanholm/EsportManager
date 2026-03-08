using CliWrap;
using MudBlazor;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;

namespace EsportManager.Features.Devices.Models;

public class Device
{
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
        Status = StatusEnum.Offline;
        Process = null;
    }

    public string Name { get; set; }
    public IPAddress IPAddress { get; set; }
    public string MACAdress { get; set; }
    public StatusEnum Status { get; private set; }
    public DeviceProcess? Process { get; set; }
    public bool ShowDetails { get; set; }

    public async Task TurnOn(Action statusCallback)
    {
        ShowDetails = true;

        var wakeMeOnLanCmd = Cli.Wrap($"WakeMeOnLan.exe")
            .WithWorkingDirectory(Environment.CurrentDirectory)
            .WithArguments($"/wakeup {MACAdress}");

        Process = new DeviceProcess(wakeMeOnLanCmd);
        await Process.Run();

        Status = StatusEnum.WaitingForResponse;
        statusCallback();

        Ping pingSender = new();
        pingSender.PingCompleted += (sender, e) =>
        {
            if (e?.Reply?.Status == IPStatus.Success)
            {
                Status = StatusEnum.Online;
            }
            else
            {
                Status = StatusEnum.Offline;
            }
            statusCallback();
        };

        pingSender.SendAsync(IPAddress, 30000, null);
    }

    public async Task TurnOff(ISnackbar snackbar)
    {
        StatusEnum previousStatus = Status;

        try
        {
            var cmd = Cli.Wrap($"shutdown")
                .WithWorkingDirectory(Environment.CurrentDirectory)
                .WithArguments($@"/s /m \\{IPAddress.ToString()} /t 0 /f")
                .WithValidation(CommandResultValidation.None);
        }
        catch (Exception e)
        {
            snackbar.Add(e.Message, Severity.Error);
        }

        Status = previousStatus;
    }

    public void CheckStatus(Action statusCallback)
    {
        Status = StatusEnum.WaitingForResponse;
        statusCallback();

        Ping pingSender = new();
        pingSender.PingCompleted += (sender, e) =>
        {
            if (e?.Reply?.Status == IPStatus.Success)
            {
                Status = StatusEnum.Online;
            }
            else
            {
                Status = StatusEnum.Offline;
            }
            statusCallback();
        };

        pingSender.SendAsync(IPAddress, 5000,  null);
    }

    public async Task UpdateFortnite(ISnackbar snackbar, Action commandFinshedCallback)
    {
        StatusEnum previousStatus = Status;

        try
        {
            Status = StatusEnum.Busy;

            await CopyFortniteGameFiles();
            await CopyFortniteManiFestFiles();
        }
        catch (Exception e)
        {
            snackbar.Add(e.Message, Severity.Error);
        }

        Status = previousStatus;
    }

    private async Task CopyFortniteGameFiles()
    {
        ShowDetails = true;

        string sharedFolderPath = GetSharedFolderPath();

        Directory.CreateDirectory($@"{Environment.CurrentDirectory}\Logs");
        string logPath = $@"{Environment.CurrentDirectory}\Logs\{Name}_Fortnite_{DateTime.Now.Date.ToShortDateString()}.txt";

        var cmd = Cli.Wrap($"robocopy")
            .WithWorkingDirectory(Environment.CurrentDirectory)
            .WithArguments($@"\\HVKSERVER\Fortnite {sharedFolderPath} /MIR /FFT /COPY:DATSO /DCOPY:DAT")
            .WithValidation(CommandResultValidation.None);

        Process = new DeviceProcess(cmd, logPath);
        await Process.Run();
    }

    private async Task CopyFortniteManiFestFiles()
    {
        ShowDetails = true;

        string sharedFolderPath = GetSharedFolderPath();

        Directory.CreateDirectory($@"{Environment.CurrentDirectory}\Logs");
        string logPath = $@"{Environment.CurrentDirectory}\Logs\{Name}_Fortnite_{DateTime.Now.Date.ToShortDateString()}.txt";

        var cmd = Cli.Wrap($"robocopy")
            .WithWorkingDirectory(Environment.CurrentDirectory)
            .WithArguments($@"\\HVKSERVER\FortniteManifests \\{IPAddress.ToString()}\FortniteManifests /E /COPY:DATSO")
            .WithValidation(CommandResultValidation.None);

        Process = new DeviceProcess(cmd, logPath);
        await Process.Run();
    }

    private string GetSharedFolderPath()
    {
        return $@"\\{IPAddress.ToString()}\Fortnite";
    }

    public enum StatusEnum
    {
        Offline,
        Online,
        Busy,
        [Description("Waiting for response")]
        WaitingForResponse
    }
}