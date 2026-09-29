using CliWrap.EventStream;
using EsportManager.Features.Process.Models;
using System.Diagnostics;

namespace EsportManager.Features.Process.PingProcess.Models;

public class PingProcess : DeviceProcess
{
    public PingProcess(string name, CliWrap.Command command, int totalAttempts, int intervalSeconds) : base(name, command)
    {
        TotalAttempts = totalAttempts;
        IntervalSeconds = intervalSeconds;
    }

    public PingProcess(string name, CliWrap.Command command, int totalAttempts, int intervalSeconds, Serilog.ILogger logger) : base(name, command, logger)
    {
        TotalAttempts = totalAttempts;
        IntervalSeconds = intervalSeconds;
    }

    public bool? IsOnline { get; private set; }
    public int TotalAttempts { get; }
    public int IntervalSeconds { get; }
    public int AttemptNumber { get; private set; }
    public DateTime? LastAttemptAt { get; private set; }

    public override void HandleStarted(StartedCommandEvent started)
    {
        base.HandleStarted(started);
        LastAttemptAt = DateTime.UtcNow;
    }

    public override void HandleStandardOutput(StandardOutputCommandEvent stdOut)
    {
        base.HandleStandardOutput(stdOut);

        // A genuine echo reply always includes "bytes=" (e.g. "Reply from 1.2.3.4: bytes=32 time<1ms TTL=64").
        // A router or the local stack telling us it CAN'T reach the destination - ARP failure, no route, a
        // routing loop, a firewall block, etc. - is also formatted as "Reply from <router>: <reason>." but
        // never includes "bytes=" (e.g. "Destination host/net/port/protocol unreachable.", "TTL expired in
        // transit.", "Communication with destination host is administratively prohibited."). Requiring
        // "bytes=" - rather than trying to enumerate every failure phrasing - is what keeps a device that
        // isn't actually reachable from ever being reported online.
        bool isReply = stdOut.Text.Contains("Reply from", StringComparison.OrdinalIgnoreCase)
            && stdOut.Text.Contains("bytes=", StringComparison.OrdinalIgnoreCase);

        bool isFailedAttempt = !isReply &&
            (stdOut.Text.Contains("Reply from", StringComparison.OrdinalIgnoreCase)
                || stdOut.Text.Contains("Request timed out", StringComparison.OrdinalIgnoreCase)
                || stdOut.Text.Contains("could not find host", StringComparison.OrdinalIgnoreCase)
                || stdOut.Text.Contains("transmit failed", StringComparison.OrdinalIgnoreCase));

        if (isReply)
        {
            IsOnline = true;
        }
        else if (isFailedAttempt)
        {
            IsOnline = false;
        }

        if (isReply || isFailedAttempt)
        {
            AttemptNumber++;
            LastAttemptAt = DateTime.UtcNow;
        }

        if (isReply)
        {
            Cancel(processStatus: PingStatus.Online);
        }
    }

    public override void HandleExited(ExitedCommandEvent exited)
    {
        // Don't trust the exit code as an "online" signal: ping.exe can exit 0 even when every response was
        // an unreachable/TTL-expired error rather than a genuine reply (they still count as "Received", not
        // "Lost"). Default to offline unless a genuine "Reply from ...: bytes=..." line was actually seen.
        IsOnline ??= false;
        _status = PingStatus.FromReachability(IsOnline.Value);

        Debug.WriteLine($"Ping exited with code {exited.ExitCode} ({_status})");
    }
}
