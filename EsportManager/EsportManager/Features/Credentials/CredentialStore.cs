using System.Net;
using EsportManager.Features.Credentials.Models;
using Device = EsportManager.Features.Devices.Models.Device;
using static EsportManager.Features.Credentials.WindowsCredentialManager;

namespace EsportManager.Features.Credentials;

/// <summary>
/// Keeps the copy credentials in Windows Credential Manager. The default and per-device credentials are
/// stored as generic entries owned by the app, and the credential robocopy actually uses is written as a
/// domain password entry targeting the device IP, so Windows picks it up when connecting to \\ip\share.
/// Every entry is tagged with <see cref="EntryComment"/> so the app can find its own entries again.
/// </summary>
public class CredentialStore
{
    public const string DefaultUserName = "esmcopy";

    private const string EntryComment = "EsportManager";
    private const string DefaultTarget = "EsportManager:Default";
    private const string DeviceTargetPrefix = "EsportManager:Device:";

    public CopyCredential? GetDefault() => ReadCredential(DefaultTarget);

    public CopyCredential? GetDeviceCredential(IPAddress ip) => ReadCredential(DeviceTarget(ip));

    public bool HasNetworkCredential(IPAddress ip) => Read(ip.ToString(), CredentialType.DomainPassword) != null;

    public int SaveDefault(CopyCredential credential, IEnumerable<Device> devices)
    {
        Write(DefaultTarget, CredentialType.Generic, credential.UserName, credential.Password, EntryComment);

        return ApplyDefault(devices);
    }

    public int ApplyDefault(IEnumerable<Device> devices)
    {
        CopyCredential? defaultCredential = GetDefault();
        if (defaultCredential == null)
        {
            return 0;
        }

        int appliedCount = 0;
        foreach (Device device in devices.Where(HasAddress))
        {
            if (GetDeviceCredential(device.IPAddress) != null)
            {
                continue;
            }

            WriteNetworkCredential(device.IPAddress, defaultCredential);
            appliedCount++;
        }

        return appliedCount;
    }

    public void SaveDeviceCredential(Device device, CopyCredential credential)
    {
        Write(DeviceTarget(device.IPAddress), CredentialType.Generic, credential.UserName, credential.Password, EntryComment);
        WriteNetworkCredential(device.IPAddress, credential);
    }

    public void ResetDeviceToDefault(Device device)
    {
        Delete(DeviceTarget(device.IPAddress), CredentialType.Generic);

        CopyCredential? defaultCredential = GetDefault();
        if (defaultCredential != null)
        {
            WriteNetworkCredential(device.IPAddress, defaultCredential);
        }
        else
        {
            Delete(device.IPAddress.ToString(), CredentialType.DomainPassword);
        }
    }

    public void RemoveDevice(Device device)
    {
        Delete(DeviceTarget(device.IPAddress), CredentialType.Generic);
        Delete(device.IPAddress.ToString(), CredentialType.DomainPassword);
    }

    public int RemoveAllDevices()
    {
        List<StoredCredential> deviceEntries = Enumerate()
            .Where(c => c.Comment == EntryComment && c.TargetName != DefaultTarget)
            .ToList();

        foreach (StoredCredential entry in deviceEntries)
        {
            Delete(entry.TargetName, entry.Type);
        }

        return deviceEntries.Count(c => c.Type == CredentialType.DomainPassword);
    }

    private static bool HasAddress(Device device) => !device.IPAddress.Equals(IPAddress.None);

    private static string DeviceTarget(IPAddress ip) => $"{DeviceTargetPrefix}{ip}";

    private static CopyCredential? ReadCredential(string target)
    {
        var credential = Read(target, CredentialType.Generic);

        return credential == null ? null : new CopyCredential(credential.Value.UserName, credential.Value.Password);
    }

    private static void WriteNetworkCredential(IPAddress ip, CopyCredential credential)
    {
        string userName = credential.UserName.Contains('\\') ? credential.UserName : $@"{ip}\{credential.UserName}";

        Write(ip.ToString(), CredentialType.DomainPassword, userName, credential.Password, EntryComment);
    }
}
