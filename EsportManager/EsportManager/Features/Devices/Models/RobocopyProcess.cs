using CliWrap.EventStream;
using System.Diagnostics;

namespace EsportManager.Features.Devices.Models;

public class RobocopyProcess : DeviceProcess
{
    public RobocopyProcess(string name, CliWrap.Command command) : base(name, command)
    {
    }

    public RobocopyProcess(string name, CliWrap.Command command, string logPath) : base(name, command, logPath)
    {
    }

    public override void HandleExited(ExitedCommandEvent exited)
    {
        _status = RobocopyStatus.FromExitCode(exited.ExitCode);

        Debug.WriteLine($"Robocopy exited with code {exited.ExitCode} ({_status})");
    }

    public override void HandleStandardError(StandardErrorCommandEvent stdErr)
    {
        base.HandleStandardError(stdErr);
    }

    public override void HandleStandardOutput(StandardOutputCommandEvent stdOut)
    {
        base.HandleStandardOutput(stdOut);

        if (ShouldBreak(stdOut))
        {
            Cancel();
        }
    }

    public override void HandleStarted(StartedCommandEvent started)
    {
        base.HandleStarted(started);
    }

    private bool ShouldBreak(StandardOutputCommandEvent stdOut)
    {
        return stdOut.Text switch
        {
            string a when a.Contains("ERROR 53") => true,
            _ => false,
        };
    }
}
