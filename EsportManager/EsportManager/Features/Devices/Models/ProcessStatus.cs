using EsportManager.Resources.Strings;
using MudBlazor;

namespace EsportManager.Features.Devices.Models;

public class ProcessStatus
{
    public static readonly ProcessStatus Initialized = new(AppStrings.StatusInitialized);
    public static readonly ProcessStatus Running = new(AppStrings.StatusRunning);
    public static readonly ProcessStatus Finished = new(AppStrings.StatusFinished, severity: Severity.Success);
    public static readonly ProcessStatus FinishedWithError = new(AppStrings.StatusFinishedWithError, severity: Severity.Error);
    public static readonly ProcessStatus CancelledByUser = new(AppStrings.StatusCancelledByUser, severity: Severity.Error);
    public static readonly ProcessStatus CancelledWithError = new(AppStrings.StatusCancelledWithError, severity: Severity.Error);

    public string Name { get; }
    public bool IsError => Severity == Severity.Error;
    public bool IsWarning => Severity == Severity.Warning;
    public Severity Severity { get;}

    protected ProcessStatus(string name, Severity severity = Severity.Normal)
    {
        Name = name;
        Severity = severity;
    }

    public override string ToString() => Name;
}
