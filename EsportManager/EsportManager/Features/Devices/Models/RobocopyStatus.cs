using MudBlazor;

namespace EsportManager.Features.Devices.Models;

public class RobocopyStatus : ProcessStatus
{
    public static readonly RobocopyStatus NoFilesCopied = new("No files were copied. No failure was met. No files were mismatched.", 0, Severity.Warning);
    public static readonly RobocopyStatus FilesCopied = new("One or more files were copied successfully.", 1, Severity.Success);
    public static readonly RobocopyStatus ExtraFilesDetected = new("Extra files or directories were detected.", 2, Severity.Warning);
    public static readonly RobocopyStatus FilesCopiedAndExtraDetected = new("One or more files were copied successfully. Extra files or directories were detected.", 3, Severity.Warning);
    public static readonly RobocopyStatus MismatchedDetected = new("Mismatched files or directories were detected.", 4, Severity.Warning);
    public static readonly RobocopyStatus FilesCopiedAndMismatchDetected = new("One or more files were copied successfully. Mismatched files or directories were detected.", 5, Severity.Warning);
    public static readonly RobocopyStatus ExtraAndMismatchDetected = new("Extra files or directories were detected. Mismatched files or directories were detected.", 6, Severity.Warning);
    public static readonly RobocopyStatus FilesCopiedExtraAndMismatchDetected = new("One or more files were copied successfully. Extra files or directories were detected. Mismatched files or directories were detected.", 7, Severity.Warning);
    public static readonly RobocopyStatus CopyErrors = new("Some files or directories could not be copied (copy errors occurred and the retry limit was exceeded).", 8, Severity.Error);
    public static readonly RobocopyStatus FatalError = new("Serious error. Robocopy did not copy any files.", 16, Severity.Error);

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
