using System.Net;
using System.Net.NetworkInformation;
using EsportManager.Features.Devices.UpdateFortnite;
using EsportManager.Resources.Strings;

namespace EsportManager.Features.Devices.DeviceSettings;

public static class ShareConnectionTester
{
    private const int PingTimeoutMilliseconds = 1000;

    private const int ErrorPathNotFound = 3;
    private const int ErrorAccessDenied = 5;
    private const int ErrorBadNetPath = 53;
    private const int ErrorBadNetName = 67;
    private const int ErrorLogonFailure = 1326;

    public record Result(bool Success, string Message);

    public static async Task<Result> TestAsync(IPAddress ip)
    {
        if (!await IsReachable(ip))
        {
            return new Result(false, AppStrings.ConnectionDeviceOffline);
        }

        foreach (string share in (string[])[FortniteShares.GameFiles, FortniteShares.Manifests])
        {
            string? error = await Task.Run(() => TestShare(ip, share));
            if (error != null)
            {
                return new Result(false, error);
            }
        }

        return new Result(true, AppStrings.ConnectionOk);
    }

    private static async Task<bool> IsReachable(IPAddress ip)
    {
        try
        {
            using Ping ping = new();
            PingReply reply = await ping.SendPingAsync(ip, PingTimeoutMilliseconds);

            return reply.Status == IPStatus.Success;
        }
        catch (PingException)
        {
            return false;
        }
    }

    private static string? TestShare(IPAddress ip, string share)
    {
        string sharePath = $@"\\{ip}\{share}";
        string testFile = Path.Combine(sharePath, $".esportmanager-test-{Guid.NewGuid():N}");

        try
        {
            File.WriteAllText(testFile, string.Empty);
            File.Delete(testFile);

            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return string.Format(AppStrings.ConnectionAccessDenied, sharePath);
        }
        catch (IOException e)
        {
            return (e.HResult & 0xFFFF) switch
            {
                ErrorLogonFailure => AppStrings.RobocopyErrorIncorrectCredentials,
                ErrorAccessDenied => string.Format(AppStrings.ConnectionAccessDenied, sharePath),
                ErrorPathNotFound or ErrorBadNetPath or ErrorBadNetName => string.Format(AppStrings.ConnectionShareNotFound, sharePath),
                _ => string.Format(AppStrings.ConnectionFailed, sharePath, e.Message)
            };
        }
    }
}
