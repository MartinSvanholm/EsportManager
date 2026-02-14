using CliWrap;
using MudBlazor;
using System.Net;

namespace EsportManager.Features.Devices.Models;

public class Device
{
    public Device(IPAddress iPAddress, string macAdress)
    {
        IPAddress = iPAddress ?? throw new ArgumentNullException(nameof(iPAddress));
        MACAdress = macAdress ?? throw new ArgumentNullException(nameof(macAdress));
        State = StateEnum.Loading;

        Name = string.Empty;
    }

    public Device(string csvLine)
    {
        string[] values = csvLine.Split(',');

        if (!string.IsNullOrEmpty(values[0]))
        {
            IPAddress = IPAddress.Parse(values[0]);
        }

        Name = values[1];
        MACAdress = values[2];
    }

    public string Name { get; set; }
    public IPAddress IPAddress { get; set; }
    public string MACAdress { get; set; }
    public StateEnum State { get; set; }
    public bool ShowSaveBtn =>  IsSaved == false && State == StateEnum.Online;
    public bool ShowPowerOnBtn => State == StateEnum.Offline;
    public bool ShowUpdateFortniteBtn => State == StateEnum.Online;
    public bool IsSaved { get; internal set; }

    public event Action? StateChanged;

    protected virtual void OnStateChanged()
    {
        StateChanged?.Invoke();
    }

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

    public enum StateEnum
    {
        Offline,
        Online,
        Loading
    }
}