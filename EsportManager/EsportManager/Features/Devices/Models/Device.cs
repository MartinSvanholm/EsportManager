using CliWrap;
using EsportManager.Features.Process.Models;
using MudBlazor;
using System.ComponentModel;
using System.Net;

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
        Plan = new DevicePlan();
    }

    public string Name { get; set; }
    public IPAddress IPAddress { get; set; }
    public string MACAdress { get; set; }
    public StatusEnum Status { get; private set; }
    public DevicePlan Plan { get; }
    public DeviceProcess? CurrentProcess => Plan.CurrentStep;
    public bool ShowDetails { get; set; }

    public void EnqueueProcess(DeviceProcess process)
    {
        Plan.Enqueue(process);
    }

    public void EnqueueProcesses(List<DeviceProcess> processes)
    {
        Plan.Enqueue(processes);
    }

    public void ClearQueue()
    {
        Plan.Clear();
    }

    public void SetStatus(StatusEnum status)
    {
        Status = status;
    }

    public async Task ExecutePlan()
    {
        StatusEnum previousStatus = Status;
        SetStatus(StatusEnum.Busy);

        await Plan.ExecuteAsync();

        SetStatus(previousStatus);
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

    public enum StatusEnum
    {
        Offline,
        Online,
        Busy,
        [Description("Waiting for response")]
        WaitingForResponse
    }
}
