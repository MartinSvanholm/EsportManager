using EsportManager.Features.Process.Models;
using EsportManager.Resources.Strings;
using MudBlazor;

namespace EsportManager.Features.Process.PingProcess.Models;

public class PingStatus : ProcessStatus
{
    public static readonly PingStatus Online = new(AppStrings.PingStatusOnline, Severity.Success);
    public static readonly PingStatus Offline = new(AppStrings.PingStatusOffline, Severity.Warning);

    private PingStatus(string name, Severity severity) : base(name, severity)
    {
    }

    public static PingStatus FromReachability(bool isOnline) => isOnline ? Online : Offline;
}
