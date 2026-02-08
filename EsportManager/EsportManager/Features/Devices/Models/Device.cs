using MudBlazor;
using SQLite;
using System.Net;

namespace EsportManager.Features.Devices.Models;

public class Device
{
    public Device(IPAddress iPAddress, string macAdress)
    {
        IPAddress = iPAddress ?? throw new ArgumentNullException(nameof(iPAddress));
        MacAdress = macAdress ?? throw new ArgumentNullException(nameof(macAdress));
        State = StateEnum.Loading;

        Name = string.Empty;
    }

    public Device(DeviceDb db)
    {
        IPAddress = IPAddress.Parse(db.IPAddress ?? throw new ArgumentNullException(nameof(db.IPAddress)));
        MacAdress = db.MacAdress ?? throw new ArgumentNullException(nameof(db.MacAdress));
        State = StateEnum.Loading;

        Name = db.Name;
        IsSaved = true;
    }

    public string Name { get; set; }
    public IPAddress IPAddress { get; set; }
    public string MacAdress { get; set; }
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

    public async Task GetHostName(SQLiteConnection connection)
    {
        try
        {
            DeviceDb? deviceDb = connection.Table<DeviceDb>().Where(d => d.MacAdress == MacAdress).FirstOrDefault();

            if (deviceDb != null)
            {
                Name = deviceDb.Name;
                IsSaved = true;
            }

            var hostEntry = await Dns.GetHostEntryAsync(IPAddress);

            Name = hostEntry.HostName;
            State = StateEnum.Online;
        }
        catch (Exception)
        {
            State = StateEnum.Offline;
        }

        OnStateChanged();
    }

    public async Task Save(SQLiteConnection connection, ISnackbar snackbar)
    {
        try
        {
            DeviceDb deviceDb = MapToDb();

            connection.Insert(deviceDb);

            snackbar.Add("Device saved", Severity.Success);
        }
        catch (Exception e)
        {
            snackbar.Add(e.Message, Severity.Error);
        }
    }

    public DeviceDb MapToDb()
    {
        return new DeviceDb
        {
            Name = Name,
            IPAddress = IPAddress.ToString(),
            MacAdress = MacAdress,
        };
    }

    public enum StateEnum
    {
        Offline,
        Online,
        Loading
    }
}