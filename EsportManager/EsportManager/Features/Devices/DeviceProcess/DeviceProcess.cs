using CliWrap.EventStream;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace EsportManager.Features.Devices.Models;

public class DeviceProcess : IDisposable
{
    public DeviceProcess(CliWrap.Command command, Action processChangedCallback)
    {
        Command = command;
        ProcessChanged += processChangedCallback;
        LogPath = string.Empty;

        Status = ProcessStatusEnum.Initialized;
        StatusMessage = "Process initialized";
    }

    public DeviceProcess(CliWrap.Command command, Action processChangedCallback, string logPath)
    {
        Command = command;
        ProcessChanged += processChangedCallback;
        LogPath = logPath;

        Status = ProcessStatusEnum.Initialized;
        StatusMessage = "Process initialized";
    }

    public int Id { get; private set; }
    public CliWrap.Command Command { get; private set; }
    public ProcessStatusEnum Status { get; private set; }
    public bool IsRunning => Status == ProcessStatusEnum.Running;
    public string StatusMessage { get; private set; }
    public int Progress { get; private set; }

    public event Action? ProcessChanged;
    private void OnProcessChanged()
    {
        ProcessChanged?.Invoke();
    }

    private string LogPath { get; set; }

    public async Task StartProcess(CancellationToken cancellationToken = default)
    {
        try
        {
            await foreach (var cmdEvent in Command.ListenAsync(cancellationToken))
            {
                switch (cmdEvent)
                {
                    case StartedCommandEvent started:
                        HandleProcessStarted(started);
                        break;
                    case StandardOutputCommandEvent stdOut:
                        HandleStandardOutputCommandEvent(stdOut);
                        break;
                    case StandardErrorCommandEvent stdErr:
                        HandleStandardErrorCommandEvent(stdErr);
                        break;
                    case ExitedCommandEvent exited:
                        HandleProcessExited(exited);
                        break;
                }
            }
        }
        catch (OperationCanceledException oce)
        {
            SetMessage($"Process cancelled by user: {oce.Message}");
            Status = ProcessStatusEnum.CancelledByUser;

            Debug.WriteLine($"Process cancelled by user: {oce.Message}");
            OnProcessChanged();
        }
        catch (Exception e)
        {
            SetMessage($"An error happened: {e.Message}");
            Debug.WriteLine($"An error happened: {e.Message}");
            OnProcessChanged();
        }
    }

    private void HandleProcessStarted(StartedCommandEvent started)
    {
        Id = started.ProcessId;
        Status = ProcessStatusEnum.Running;

        SetMessage($"Process started; ID: {started.ProcessId}");
        Debug.WriteLine($"Process started; ID: {started.ProcessId}");

        OnProcessChanged();
    }

    private void HandleStandardOutputCommandEvent(StandardOutputCommandEvent stdOut)
    {
        SetMessage($"{stdOut.Text}");
        Debug.WriteLine($"Out> {stdOut.Text}");

        OnProcessChanged();
    }

    private void HandleStandardErrorCommandEvent(StandardErrorCommandEvent stdErr)
    {
        SetMessage($"{stdErr.Text}");
        Debug.WriteLine($"Err> {stdErr.Text}");

        OnProcessChanged();
    }

    private void HandleProcessExited(ExitedCommandEvent exited)
    {
        Status = exited.ExitCode == 0 ? ProcessStatusEnum.Finished : ProcessStatusEnum.FinishedWithError;
        SetMessage($"Process exited with code {exited.ExitCode}");
        Debug.WriteLine($"Process exited with code {exited.ExitCode}");

        OnProcessChanged();
        ProcessChanged.GetInvocationList().ToList().ForEach(d => ProcessChanged -= (Action)d);
    }

    private void SetMessage(string message)
    {
        message = message.Trim();

        if (IsInvalidMessage(message)) return;

        if (!string.IsNullOrWhiteSpace(LogPath))
        {
            File.AppendAllLines(LogPath, [message]);
        }

        Regex regex = new(@"^\d+(\.\d+)?%$");
        if (regex.IsMatch(message))
        {
            double number = double.Parse(message.Split('%')[0], CultureInfo.InvariantCulture);
            Progress = Convert.ToInt32(Math.Round(number));

            StatusMessage = $"{StatusMessage.Split(':')[0]}: {message}";
            return;
        }
        Progress = 0;

        StatusMessage = message;
    }

    private bool IsInvalidMessage(string message)
    {
        switch (message)
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

    public void Dispose()
    {
        ProcessChanged?.GetInvocationList().ToList().ForEach(d => ProcessChanged -= (Action)d);
    }

    public enum ProcessStatusEnum
    {
        Initialized,
        Running,
        Finished,
        FinishedWithError,
        CancelledByUser
    }
}