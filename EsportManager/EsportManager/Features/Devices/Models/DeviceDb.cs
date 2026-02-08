using SQLite;
using System.Net;

namespace EsportManager.Features.Devices.Models;

public class DeviceDb
{
    public string Name { get; set; }
    public string IPAddress { get; set; }

    [PrimaryKey]
    public string MacAdress { get; set; }
}