using CliWrap;
using MudBlazor;
using System.ComponentModel;
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
        ProcessQueue = [];
    }

    public string Name { get; set; }
    public IPAddress IPAddress { get; set; }
    public string MACAdress { get; set; }
    public StatusEnum Status { get; private set; }
    public List<DeviceProcess> ProcessQueue { get; private set; }
    public DeviceProcess? CurrentProcess => ProcessQueue?.FirstOrDefault(p => p.IsRunning);
    public bool ShowDetails { get; set; }

    public void EnqueueProcess(DeviceProcess process)
    {
        ProcessQueue.Add(process);
    }

    public void EnqueueProcesses(List<DeviceProcess> processes)
    {
        ProcessQueue.AddRange(processes);
    }

    public void ClearQueue()
    {
        ProcessQueue.Clear();
    }

    public async Task StartQueue()
    {
        foreach (var process in ProcessQueue)
        {
            if (!process.IsRunning)
            {
                await process.Run();

                if (process.HasError)
                {
                    break;
                }
            }
        }
    }

    public async Task TurnOn(Action statusCallback)
    {
        ShowDetails = true;

        var wakeMeOnLanCmd = Cli.Wrap($"WakeMeOnLan.exe")
            .WithWorkingDirectory(Environment.CurrentDirectory)
            .WithArguments($"/wakeup {MACAdress}");

        //ProcessQueue.Add(new DeviceProcess(wakeMeOnLanCmd));
        //await Process.Run();

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

    public enum StatusEnum
    {
        Offline,
        Online,
        Busy,
        [Description("Waiting for response")]
        WaitingForResponse
    }
}