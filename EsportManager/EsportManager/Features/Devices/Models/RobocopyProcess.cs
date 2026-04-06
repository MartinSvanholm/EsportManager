using CliWrap.EventStream;
using System;
using System.Collections.Generic;
using System.Text;
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

        Regex regex = new("ERROR \\d{1,2}");

        if (regex.IsMatch(stdOut.Text))
        {
            Cancel();
        }
    }

    public override void HandleStarted(StartedCommandEvent started)
    {
        base.HandleStarted(started);
    }
}
