using System.Net;
using CliWrap;
using EsportManager.Features.Credentials;
using Device = EsportManager.Features.Devices.Models.Device;

namespace EsportManager.Features.Devices;

public class DeviceStore(CredentialStore credentialStore)
{
    private const string DevicesFileName = "devices";

    private List<Device> _devices = [];
    private bool _isLoaded;

    public IReadOnlyList<Device> Devices => _devices;

    private static string DevicesFilePath => Path.Combine(Environment.CurrentDirectory, DevicesFileName);

    public async Task EnsureLoadedAsync(CancellationToken token = default)
    {
        if (_isLoaded)
        {
            return;
        }

        await ReloadAsync(token);
    }

    public Device? Find(IPAddress ip) => _devices.FirstOrDefault(d => d.IPAddress.Equals(ip));

    public async Task ScanAsync()
    {
        try
        {
            File.Delete(DevicesFilePath);

            await Cli.Wrap("WakeMeOnLan.exe")
                .WithWorkingDirectory(Environment.CurrentDirectory)
                .WithArguments("/scan")
                .ExecuteAsync();

            await Cli.Wrap("WakeMeOnLan.exe")
                .WithWorkingDirectory(Environment.CurrentDirectory)
                .WithArguments($"/scomma {DevicesFileName}")
                .ExecuteAsync();
        }
        finally
        {
            await ReloadAsync();
        }

        credentialStore.ApplyDefault(_devices);
    }

    private async Task ReloadAsync(CancellationToken token = default)
    {
        List<Device> devices = [];

        if (File.Exists(DevicesFilePath))
        {
            string[] lines = await File.ReadAllLinesAsync(DevicesFilePath, token);
            foreach (string line in lines.Skip(1))
            {
                Device scanned = new(line);
                Device? existing = scanned.IPAddress.Equals(IPAddress.None) ? null : Find(scanned.IPAddress);

                if (existing != null)
                {
                    existing.Name = scanned.Name;
                    existing.MACAdress = scanned.MACAdress;
                    devices.Add(existing);
                }
                else
                {
                    devices.Add(scanned);
                }
            }
        }

        _devices = devices;
        _isLoaded = true;
    }
}
