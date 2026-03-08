using CliWrap.EventStream;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace EsportManager.Features.Devices.Models;

public class DeviceProcess
{
    public DeviceProcess(CliWrap.Command command)
    {
        _command = command;
        LogPath = string.Empty;
        _status = ProcessStatusEnum.Initialized;
        _output = string.Empty;
        _cancellationTokenSource = new CancellationTokenSource();
    }
     
    public DeviceProcess(CliWrap.Command command, string logPath)
    {
        _command = command;
        LogPath = logPath;
        _status = ProcessStatusEnum.Initialized;
        _output = string.Empty;
        _cancellationTokenSource = new CancellationTokenSource();
    }

    private int _id { get; set; }
    public int Id
    {
        get => _id;
    }

    private CliWrap.Command _command { get; set; }
    public CliWrap.Command Command
    {
        get => _command;
    }

    private ProcessStatusEnum _status { get; set; }
    public ProcessStatusEnum Status
    {
        get => _status;
    }

    public bool IsRunning => Status == ProcessStatusEnum.Running;

    private string _output { get; set; }
    public string Output
    {
        get => _output;
    }

    private int _progress { get; set; }
    public int Progress
    {
        get => _progress;
    }

    private CancellationTokenSource _cancellationTokenSource { get; set; }
    public CancellationTokenSource CancellationTokenSource
    {
        get => _cancellationTokenSource;
    }

    private string DoneMessage { get; set; }

    private string LogPath { get; set; }

    public delegate Task ProcessChangedHandler(CommandEvent commandEvent, DeviceProcess process);
    public event ProcessChangedHandler? ProcessChanged;
    private void OnProcessChanged(CommandEvent commandEvent)
    {
        ProcessChanged?.Invoke(commandEvent, this);
    }

    public async Task<CommandEvent?> Run()
    {
        try
        {
            await foreach (var cmdEvent in Command.ListenAsync(CancellationTokenSource.Token))
            {
                switch (cmdEvent)
                {
                    case StartedCommandEvent started:
                        HandleStarted(started);
                        break;
                    case StandardOutputCommandEvent stdOut:
                        HandleStandardOutput(stdOut);
                        break;
                    case StandardErrorCommandEvent stdErr:
                        HandleStandardError(stdErr);
                        break;
                    case ExitedCommandEvent exited:
                        HandleExited(exited);
                        return exited;
                }
            }
        }
        catch (OperationCanceledException oce)
        {
            _status = ProcessStatusEnum.CancelledByUser;
            Debug.WriteLine($"Process cancelled by user: {oce.Message}");

            OnProcessChanged(new ExitedCommandEvent(0));
        }
        catch (Exception e)
        {
            _status = ProcessStatusEnum.FinishedWithError;
            Debug.WriteLine($"An error happened: {e.Message}");

            OnProcessChanged(new ExitedCommandEvent(0));
        }

        return null;
    }

    public void Cancel()
    {
        CancellationTokenSource.Cancel();
    }

    private void HandleStarted(StartedCommandEvent started)
    {
        _id = started.ProcessId;
        _status = ProcessStatusEnum.Running;

        SetOutput($"Process started; ID: {started.ProcessId}");
        Debug.WriteLine($"Process started; ID: {started.ProcessId}");

        OnProcessChanged(started);
    }
    
    private void HandleStandardOutput(StandardOutputCommandEvent stdOut)
    {
        SetOutput($"{stdOut.Text}");
        Debug.WriteLine($"Out> {stdOut.Text}");

        OnProcessChanged(stdOut);
    }

    private void HandleStandardError(StandardErrorCommandEvent stdErr)
    {
        SetOutput($"{stdErr.Text}");
        Debug.WriteLine($"Err> {stdErr.Text}");

        OnProcessChanged(stdErr);
    }

    private void HandleExited(ExitedCommandEvent exited)
    {
        _status = ProcessStatusEnum.Finished;

        string output = $"Process exited with code {exited.ExitCode}";
        SetOutput(output);
        Debug.WriteLine(output);

        OnProcessChanged(exited);
    }

    private void SetOutput(string output)
    {
        output = output.Trim();

        if (IsInvalidOutput(output)) return;

        if (!string.IsNullOrWhiteSpace(LogPath))
        {
            File.AppendAllLines(LogPath, [output]);
        }

        Regex regex = new(@"^\d+(\.\d+)?%$");
        if (regex.IsMatch(output))
        {
            double number = double.Parse(output.Split('%')[0], CultureInfo.InvariantCulture);
            _progress = Convert.ToInt32(Math.Round(number));

            _output = $"{Output.Split(':')[0]}: {output}";
            return;
        }

        _progress = 0;
        _output = output;
    }

    private bool IsInvalidOutput(string output)
    {
        switch (output)
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

    public enum ProcessStatusEnum
    {
        Initialized,
        Running,
        Finished,
        [Description("Finished with error")]
        FinishedWithError,
        [Description("Cancelled by user")]
        CancelledByUser
    }
}