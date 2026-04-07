using EsportManager.Resources.Strings;
using MudBlazor;

namespace EsportManager.Features.Devices.Models;

public class RobocopyStatus : ProcessStatus
{
    public static readonly RobocopyStatus NoFilesCopied = new(AppStrings.RobocopyNoFilesCopied, 0, Severity.Warning);
    public static readonly RobocopyStatus FilesCopied = new(AppStrings.RobocopyFilesCopied, 1, Severity.Success);
    public static readonly RobocopyStatus ExtraFilesDetected = new(AppStrings.RobocopyExtraFilesDetected, 2, Severity.Warning);
    public static readonly RobocopyStatus FilesCopiedAndExtraDetected = new(AppStrings.RobocopyFilesCopiedAndExtra, 3, Severity.Warning);
    public static readonly RobocopyStatus MismatchedDetected = new(AppStrings.RobocopyMismatchDetected, 4, Severity.Warning);
    public static readonly RobocopyStatus FilesCopiedAndMismatchDetected = new(AppStrings.RobocopyFilesCopiedAndMismatch, 5, Severity.Warning);
    public static readonly RobocopyStatus ExtraAndMismatchDetected = new(AppStrings.RobocopyExtraAndMismatch, 6, Severity.Warning);
    public static readonly RobocopyStatus FilesCopiedExtraAndMismatchDetected = new(AppStrings.RobocopyFilesCopiedExtraAndMismatch, 7, Severity.Warning);
    public static readonly RobocopyStatus CopyErrors = new(AppStrings.RobocopyCopyErrors, 8, Severity.Error);
    public static readonly RobocopyStatus FatalError = new(AppStrings.RobocopyFatalError, 16, Severity.Error);

    public int ExitCode { get; }

    private RobocopyStatus(string name, int exitCode, Severity severity = Severity.Normal)
        : base(name, severity)
    {
        ExitCode = exitCode;
    }

    public static RobocopyStatus FromExitCode(int exitCode) => exitCode switch
    {
        0 => NoFilesCopied,
        1 => FilesCopied,
        2 => ExtraFilesDetected,
        3 => FilesCopiedAndExtraDetected,
        4 => MismatchedDetected,
        5 => FilesCopiedAndMismatchDetected,
        6 => ExtraAndMismatchDetected,
        7 => FilesCopiedExtraAndMismatchDetected,
        >= 8 and < 16 => CopyErrors,
        _ => FatalError,
    };
}
