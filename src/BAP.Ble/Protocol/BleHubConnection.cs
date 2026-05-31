using BAP.Core.Enums;
using BAP.Core.Interfaces;
using BAP.Core.Models;
using Microsoft.Extensions.Logging;

namespace BAP.Ble.Protocol;

/// <summary>
/// Base class for BLE hub connections. Provides shared wiring for
/// IBleAdapter/IBleDevice usage and event dispatch. Subclasses implement
/// protocol-specific message building and parsing.
/// </summary>
public abstract class BleHubConnection : IHubConnection
{
    protected readonly IBleAdapter BleAdapter;
    protected readonly ILogger Logger;
    protected IBleDevice? Device;

    public HubModel Hub { get; }
    public bool IsConnected => Device?.IsConnected ?? false;

    public event EventHandler<HubColorEventArgs>? ColorTriggered;
    public event EventHandler<HubDistanceEventArgs>? DistanceTriggered;
    public event EventHandler<HubRemoteButtonEventArgs>? RemoteButtonTriggered;
    public event EventHandler<HubBatteryEventArgs>? BatteryUpdated;
    public event EventHandler<HubPortEventArgs>? PortUpdated;
    public event EventHandler? Disconnected;

    protected BleHubConnection(HubModel hub, IBleAdapter bleAdapter, ILogger logger)
    {
        Hub = hub;
        BleAdapter = bleAdapter;
        Logger = logger;
    }

    public abstract Task ConnectAsync(CancellationToken cancellationToken = default);
    public abstract Task SetMotorSpeedAsync(string port, int speed);
    public abstract Task SetLightBrightnessAsync(string port, int brightness);
    public abstract Task SetLedColorAsync(SensorColor color);
    public abstract Task StopMotorAsync(string port, bool brake = false);

    public virtual async Task DisconnectAsync()
    {
        if (Device != null)
        {
            await Device.DisconnectAsync();
            Device.Dispose();
            Device = null;
        }
        Hub.IsConnected = false;
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
        GC.SuppressFinalize(this);
    }

    // ── Event helpers for subclasses ──

    protected void OnColorTriggered(string portId, SensorColor color)
    {
        var port = Hub.RegisteredPorts.FirstOrDefault(p => p.Id == portId);
        if (port != null)
        {
            port.LatestColor = color;
            port.LastColorTick = Environment.TickCount64;
        }
        ColorTriggered?.Invoke(this, new HubColorEventArgs { PortId = portId, Color = color });
    }

    protected void OnDistanceTriggered(string portId, int distance)
    {
        var port = Hub.RegisteredPorts.FirstOrDefault(p => p.Id == portId);
        if (port != null)
        {
            port.LatestDistance = distance;
            port.LastDistanceTick = Environment.TickCount64;
        }
        DistanceTriggered?.Invoke(this, new HubDistanceEventArgs { PortId = portId, Distance = distance });
    }

    protected void OnRemoteButtonTriggered(string? portId, RemoteButton button)
    {
        RemoteButtonTriggered?.Invoke(this, new HubRemoteButtonEventArgs { PortId = portId, Button = button });
    }

    protected void OnBatteryUpdated(double level, double voltage = 0)
    {
        Hub.BatteryLevel = level;
        Hub.BatteryVoltage = voltage;
        BatteryUpdated?.Invoke(this, new HubBatteryEventArgs { BatteryLevel = level, BatteryVoltage = voltage });
    }

    protected void OnPortUpdated(string portId, bool connected, PortDevice device)
    {
        var port = Hub.RegisteredPorts.FirstOrDefault(p => p.Id == portId);
        if (port != null)
        {
            port.Connected = connected;
            port.Device = device;
        }
        PortUpdated?.Invoke(this, new HubPortEventArgs { PortId = portId, Connected = connected, Device = device });
    }

    protected void OnDisconnected()
    {
        Hub.IsConnected = false;
        Disconnected?.Invoke(this, EventArgs.Empty);
    }

    protected PortModel? GetPort(string portId) =>
        Hub.RegisteredPorts.FirstOrDefault(p => p.Id == portId);

    protected PortModel? GetPortByValue(int value) =>
        Hub.RegisteredPorts.FirstOrDefault(p => p.Value == value);

    /// <summary>
    /// Write to a characteristic with error handling and disconnect detection.
    /// </summary>
    protected async Task SafeWriteAsync(IBleCharacteristic? characteristic, byte[] data, bool withResponse = false)
    {
        if (characteristic == null) return;
        try
        {
            await characteristic.WriteAsync(data, withResponse);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "{HubName} lost Bluetooth connection", Hub.Name);
            OnDisconnected();
        }
    }
}
