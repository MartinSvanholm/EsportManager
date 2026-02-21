using MudBlazor;

namespace EsportManager.Features.Devices.Models;

public class DeviceProcess
{
    public DeviceProcess(int id)
    {
        Id = id;
        _statusMassge = string.Empty;
        IsRunning = true;
    }

    public int? Id { get; internal set; }

    private string _statusMassge { get; set; }
    public string StatusMessage
    {
        get => _statusMassge;
        private set
        {
            _statusMassge = value;
            OnProcessChanged();
        }
    }

    public Severity Severity { get; set; }
    public bool IsRunning { get; private set; }

    public event Action? ProcessChanged;

    private void OnProcessChanged()
    {
        ProcessChanged?.Invoke();
    }

    public void CancelProcess(string logPath, string errorMessage = "")
    {
        IsRunning = false;

        File.AppendAllLines(logPath, ["Process cancelled"]);
        StatusMessage = "Process cancelled";
        StatusMessage += !string.IsNullOrWhiteSpace(errorMessage) ? $": {errorMessage}" : string.Empty;
    }

    public void SetMessage(string message, string logPath)
    {
        if (IsInvalidMessage(message)) return;

        File.AppendAllLines(logPath, [message.Trim()]);

        Severity = Severity.Normal;
        StatusMessage = message.Trim();
    }

    public void SetErrorMessage(string message, string logPath)
    {
        if (IsInvalidMessage(message)) return;

        File.AppendAllLines(logPath, [message.Trim()]);

        StatusMessage = message.Trim();
        Severity = Severity.Error;
    }

    private bool IsInvalidMessage(string message)
    {
        switch (message.Trim())
        {
            case null:
            case "":
            case " ":
                return true;
            case string s when s.StartsWith('-'):
                return true;
            default: 
                return false;
        }
    }
}