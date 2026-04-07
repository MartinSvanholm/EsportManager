using MudBlazor;

namespace EsportManager.Features.Devices.Models;

public class ProcessStatus
{
    public static readonly ProcessStatus Initialized = new("Initialized");
    public static readonly ProcessStatus Running = new("Running");
    public static readonly ProcessStatus Finished = new("Finished", severity: Severity.Success);
    public static readonly ProcessStatus FinishedWithError = new("Finished with error", severity: Severity.Error);
    public static readonly ProcessStatus CancelledByUser = new("Cancelled by user", severity: Severity.Error);
    public static readonly ProcessStatus CancelledWithError = new("Cancelled with error", severity: Severity.Error);

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
