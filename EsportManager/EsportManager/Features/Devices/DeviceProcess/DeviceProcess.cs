using CliWrap.EventStream;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace EsportManager.Features.Devices.Models;

public class DeviceProcess : IDisposable
{
    public DeviceProcess(CliWrap.Command command, Func<Task> processChangedCallback)
    {
        Command = command;
        ProcessChanged += processChangedCallback;
        LogPath = string.Empty;

        Status = ProcessStatusEnum.Initialized;
        StatusMessage = "Process initialized";

        UpdateTimer = new System.Timers.Timer(1000);
        UpdateTimer.Elapsed += async (sender, args) => OnProcessChanged();
    }

    public DeviceProcess(CliWrap.Command command, Func<Task> processChangedCallback, string logPath)
    {
        Command = command;
        ProcessChanged += processChangedCallback;
        LogPath = logPath;

        Status = ProcessStatusEnum.Initialized;
        StatusMessage = "Process initialized";

        UpdateTimer = new System.Timers.Timer(2000);
        UpdateTimer.Elapsed += async (sender, args) => OnProcessChanged();
    }

    public int Id { get; private set; }
    public CliWrap.Command Command { get; private set; }
    public ProcessStatusEnum Status { get; private set; }
    public bool IsRunning => Status == ProcessStatusEnum.Running;
    public string StatusMessage { get; private set; }
    public int Progress { get; private set; }
    public Func<Task> ProcessChanged { get; set; }
    private void OnProcessChanged()
    {
        ProcessChanged();
    }

    private string LogPath { get; set; }
    private System.Timers.Timer UpdateTimer { get; set; }

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

            UpdateTimer.Stop();
            UpdateTimer.Dispose();

            OnProcessChanged();
        }
        catch (Exception e)
        {
            SetMessage($"An error happened: {e.Message}");
            Debug.WriteLine($"An error happened: {e.Message}");

            UpdateTimer.Stop();
            UpdateTimer.Dispose();

            OnProcessChanged();
        }
    }

    private void HandleProcessStarted(StartedCommandEvent started)
    {
        Id = started.ProcessId;
        Status = ProcessStatusEnum.Running;

        UpdateTimer.Start();

        SetMessage($"Process started; ID: {started.ProcessId}");
        Debug.WriteLine($"Process started; ID: {started.ProcessId}");

        OnProcessChanged();
    }

    private void HandleStandardOutputCommandEvent(StandardOutputCommandEvent stdOut)
    {
        SetMessage($"{stdOut.Text}");
        Debug.WriteLine($"Out> {stdOut.Text}");
    }

    private void HandleStandardErrorCommandEvent(StandardErrorCommandEvent stdErr)
    {
        SetMessage($"{stdErr.Text}");
        Debug.WriteLine($"Err> {stdErr.Text}");

        OnProcessChanged();
    }

    private void HandleProcessExited(ExitedCommandEvent exited)
    {
        Status = ProcessStatusEnum.Finished;
        string message = $"Process exited with code {exited.ExitCode}";

        switch (exited.ExitCode)
        {
            case 0:
                message += ": No files were copied. No failure was met. No files were mismatched. The files already exist in the destination directory; so the copy operation was skipped.";
                break;
            case 1:
                message += ": All files were copied successfully.";
                break;
            case 2:
                message += ": There are some additional files in the destination directory that aren't present in the source directory. No files were copied.";
                break;
            case 3:
                message += ": Some files were copied. Additional files were present. No failure was met.";
                break;
            case 5:
                message += ": Some files were copied. Some files were mismatched. No failure was met.";
                break;
            case 6:
                message += ": Additional files and mismatched files exist. No files were copied and no failures were met. Which means that the files already exist in the destination directory.";
                break;
            case 7:
                message += ": Files were copied, a file mismatch was present, and additional files were present.";
                break;
            case 8:
                message += ": Several files didn't copy.";
                Status = ProcessStatusEnum.FinishedWithError;
                break;
        }

        SetMessage(message);
        Debug.WriteLine(message);

        UpdateTimer.Stop();
        UpdateTimer.Dispose();

        OnProcessChanged();
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
        UpdateTimer.Stop();
        UpdateTimer.Dispose();
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