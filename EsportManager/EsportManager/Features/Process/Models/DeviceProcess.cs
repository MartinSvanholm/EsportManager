using CliWrap.EventStream;
using EsportManager.Resources.Strings;
using Serilog;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace EsportManager.Features.Process.Models;

public class DeviceProcess
{
    public DeviceProcess(string name, CliWrap.Command command)
    {
        _name = name;
        _command = command;
        _status = ProcessStatus.Initialized;
        _output = string.Empty;
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public DeviceProcess(string name, CliWrap.Command command, Serilog.ILogger logger)
    {
        _name = name;
        _command = command;
        _logger = logger;
        _status = ProcessStatus.Initialized;
        _output = string.Empty;
        _cancellationTokenSource = new CancellationTokenSource();
    }

    private int _id { get; set; }
    public int Id
    {
        get => _id;
    }

    private string _name { get; set; }
    public string Name
    {
        get => _name;
    }

    private CliWrap.Command _command { get; set; }
    public CliWrap.Command Command
    {
        get => _command;
    }

    protected ProcessStatus? _customStatus { get; set; }
    protected ProcessStatus _status { get; set; }
    public ProcessStatus Status
    {
        get => _customStatus ?? _status;
    }

    public bool IsRunning => Status == ProcessStatus.Running;

    public bool HasError => Status.IsError;

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

    private Serilog.ILogger? _logger { get; set; }
    private bool CancelRequestedByUser { get; set; }

    public async Task Run()
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
                        break;
                }
            }
        }
        catch (OperationCanceledException oce)
        {
            if (CancelRequestedByUser)
            {
                _status = ProcessStatus.CancelledByUser;
                Debug.WriteLine($"Process cancelled by user: {oce.Message}");
            }

            _status = ProcessStatus.FinishedWithError;
            Debug.WriteLine($"Process cancelled due to an error: {oce.Message}");
        }
        catch (Exception e)
        {
            _status = ProcessStatus.FinishedWithError;
            Debug.WriteLine($"An error happened: {e.Message}");
        }
    }

    public void Cancel(bool requestedByUser = false, ProcessStatus? processStatus = null)
    {
        if (processStatus != null)
        {
            _customStatus = processStatus;
        }

        CancelRequestedByUser = requestedByUser;
        CancellationTokenSource.Cancel();
    }

    public virtual void HandleStarted(StartedCommandEvent started)
    {
        _id = started.ProcessId;
        _status = ProcessStatus.Running;

        SetOutput(string.Format(AppStrings.ProcessStarted, started.ProcessId));
        Debug.WriteLine($"Process started; ID: {started.ProcessId}");
    }
    
    public virtual void HandleStandardOutput(StandardOutputCommandEvent stdOut)
    {
        SetOutput($"{stdOut.Text}");
        Debug.WriteLine($"Out> {stdOut.Text}");
    }

    public virtual void HandleStandardError(StandardErrorCommandEvent stdErr)
    {
        SetOutput($"{stdErr.Text}");
        Debug.WriteLine($"Err> {stdErr.Text}");
    }

    public virtual void HandleExited(ExitedCommandEvent exited)
    {
        _status = ProcessStatus.Finished;

        string output = string.Format(AppStrings.ProcessExitedWithCode, exited.ExitCode);
        SetOutput(output);
        Debug.WriteLine(output);
    }

    private void SetOutput(string output)
    {
        output = output.Trim();

        if (IsInvalidOutput(output)) return;

        _logger?.Information(output);

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
}