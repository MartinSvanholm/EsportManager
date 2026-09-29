using CliWrap.EventStream;
using EsportManager.Features.Process.Models;
using System.Diagnostics;

namespace EsportManager.Features.Process.WakeOnLanProcess.Models;

public class WakeOnLanProcess : DeviceProcess
{
    public WakeOnLanProcess(string name, CliWrap.Command command) : base(name, command)
    {
    }

    public WakeOnLanProcess(string name, CliWrap.Command command, Serilog.ILogger logger) : base(name, command, logger)
    {
    }

    public override void HandleExited(ExitedCommandEvent exited)
    {
        _status = exited.ExitCode == 0 ? ProcessStatus.Finished : ProcessStatus.FinishedWithError;

        Debug.WriteLine($"WakeMeOnLan exited with code {exited.ExitCode} ({_status})");
    }
}
