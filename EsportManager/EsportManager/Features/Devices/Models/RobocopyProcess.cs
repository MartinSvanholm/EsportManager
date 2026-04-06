using CliWrap.EventStream;
using System.Diagnostics;
using System.Text.RegularExpressions;

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
        base.HandleExited(exited);
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
            Process process = Process.GetProcessById(Id);
            process.Kill(true);
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
