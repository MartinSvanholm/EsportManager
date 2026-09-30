using EsportManager.Features.Process.Models;

namespace EsportManager.Features.Devices.Models;

public class DevicePlan
{
    private readonly List<DeviceProcess> _steps = [];

    public IReadOnlyList<DeviceProcess> Steps => _steps;

    public DeviceProcess? CurrentStep => _steps.FirstOrDefault(s => s.IsRunning);

    public PlanState State
    {
        get
        {
            if (_steps.Count == 0) return PlanState.Empty;
            if (_steps.Any(s => s.IsRunning)) return PlanState.Executing;
            if (_steps.All(s => s.Status == ProcessStatus.Initialized)) return PlanState.Draft;
            return PlanState.Completed;
        }
    }

    public void Enqueue(DeviceProcess step)
    {
        ClearIfCompleted();
        _steps.Add(step);
    }

    public void Enqueue(IEnumerable<DeviceProcess> steps)
    {
        ClearIfCompleted();
        _steps.AddRange(steps);
    }

    private void ClearIfCompleted()
    {
        if (State == PlanState.Completed)
        {
            _steps.Clear();
        }
    }

    public void Clear()
    {
        _steps.Clear();
    }

    public async Task ExecuteAsync()
    {
        foreach (var step in _steps)
        {
            if (!step.IsRunning)
            {
                await step.Run();

                if (step.HasError)
                {
                    break;
                }
            }
        }
    }

    public enum PlanState
    {
        Empty,
        Draft,
        Executing,
        Completed
    }
}
