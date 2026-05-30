using BAP.Core.Enums;
using BAP.Core.Models;

namespace BAP.Core.Interfaces;

/// <summary>
/// Represents an active connection to a LEGO hub.
/// Provides high-level motor/sensor/LED operations abstracted over the specific protocol.
/// </summary>
public interface IHubConnection : IAsyncDisposable
{
    HubModel Hub { get; }
    bool IsConnected { get; }

    Task ConnectAsync(CancellationToken cancellationToken = default);
    Task DisconnectAsync();

    Task SetMotorSpeedAsync(string port, int speed);
    Task SetLightBrightnessAsync(string port, int brightness);
    Task SetLedColorAsync(SensorColor color);
    Task StopMotorAsync(string port, bool brake = false);

    event EventHandler<HubColorEventArgs> ColorTriggered;
    event EventHandler<HubDistanceEventArgs> DistanceTriggered;
    event EventHandler<HubRemoteButtonEventArgs> RemoteButtonTriggered;
    event EventHandler<HubBatteryEventArgs> BatteryUpdated;
    event EventHandler<HubPortEventArgs> PortUpdated;
    event EventHandler Disconnected;
}

public class HubColorEventArgs : EventArgs
{
    public required string PortId { get; init; }
    public required SensorColor Color { get; init; }
}

public class HubDistanceEventArgs : EventArgs
{
    public required string PortId { get; init; }
    public required int Distance { get; init; }
}

public class HubRemoteButtonEventArgs : EventArgs
{
    public string? PortId { get; init; }
    public required RemoteButton Button { get; init; }
}

public class HubBatteryEventArgs : EventArgs
{
    public double BatteryLevel { get; init; }
    public double BatteryVoltage { get; init; }
}

public class HubPortEventArgs : EventArgs
{
    public required string PortId { get; init; }
    public required bool Connected { get; init; }
    public required PortDevice Device { get; init; }
}
