using CliWrap.EventStream;
using EsportManager.Features.Process.Models;
using EsportManager.Resources.Strings;
using MudBlazor;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace EsportManager.Features.Process.RobocopyProcess.Models;

public class RobocopyProcess : DeviceProcess
{
    public RobocopyProcess(string name, CliWrap.Command command) : base(name, command)
    {
    }

    public RobocopyProcess(string name, CliWrap.Command command, Serilog.ILogger logger) : base(name, command, logger)
    {
    }

    public override void HandleExited(ExitedCommandEvent exited)
    {
        _status = RobocopyStatus.FromExitCode(exited.ExitCode);

        Debug.WriteLine($"Robocopy exited with code {exited.ExitCode} ({_status})");
    }

    public override void HandleStandardError(StandardErrorCommandEvent stdErr)
    {
        var (customError, shouldBreak) = GetCustomError(stdErr.Text);
        base.HandleStandardError(stdErr);

        if (shouldBreak)
        {
            Cancel(processStatus: new ProcessStatus(AppStrings.StatusFinishedWithError, customError, severity: Severity.Error));
        }
    }

    public override void HandleStandardOutput(StandardOutputCommandEvent stdOut)
    {
        var (customError, shouldBreak) = GetCustomError(stdOut.Text);
        base.HandleStandardOutput(stdOut);

        if (shouldBreak)
        {
            Cancel(processStatus: new ProcessStatus(AppStrings.StatusFinishedWithError, customError, severity: Severity.Error));
        }
    }

    public override void HandleStarted(StartedCommandEvent started)
    {
        base.HandleStarted(started);
    }

    private static readonly Regex ErrorCodeRegex = new(@"ERROR (\d+)", RegexOptions.Compiled);

    private static (string CustomError, bool ShouldBreak) GetCustomError(string text)
    {
        var match = ErrorCodeRegex.Match(text);
        if (match.Success && int.TryParse(match.Groups[1].Value, out var code) && RobocopyErrors.Errors.TryGetValue(code, out var error))
        {
            return (error.Message, error.ShouldBreak);
        }

        return (text, false);
    }
}
