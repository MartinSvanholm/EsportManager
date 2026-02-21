namespace EsportManager.Features.Devices.Models;

public class DeviceStatus
{
    public DeviceStatus()
    {
        State = StateEnum.Busy;
    }

    private StateEnum _state { get; set; }
    public StateEnum State
    {
        get => _state;
        private set
        {
            _state = value;
            OnStatusChanged();
        }
    }

    public event Action? StatusChanged;

    private void OnStatusChanged()
    {
        StatusChanged?.Invoke();
    }

    public enum StateEnum
    {
        Offline,
        Online,
        Busy
    }
}