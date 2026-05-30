using BAP.Core.Enums;
using BAP.Core.Interfaces;
using BAP.Core.Models;
using Microsoft.Extensions.Logging;

namespace BAP.Ble.Protocol;

/// <summary>
/// WeDo 2.0 Smart Hub protocol implementation. Uses multiple GATT characteristics
/// for port detection, sensor data, motor control, and battery monitoring.
/// </summary>
public class WedoHubConnection : BleHubConnection
{
    private IBleCharacteristic? _motorCharacteristic;
    private IBleCharacteristic? _portTypeCharacteristic;
    private IBleCharacteristic? _sensorCharacteristic;
    private IBleCharacteristic? _buttonCharacteristic;
    private IBleCharacteristic? _batteryCharacteristic;
    private IBleCharacteristic? _portTypeWriteCharacteristic;
    private IBleCharacteristic? _disconnectCharacteristic;

    public WedoHubConnection(HubModel hub, IBleAdapter bleAdapter, ILogger<WedoHubConnection> logger)
        : base(hub, bleAdapter, logger) { }

    public override async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        Device = await BleAdapter.ConnectAsync(Hub.BluetoothAddress, cancellationToken);
        if (Device == null)
            throw new InvalidOperationException($"Failed to connect to WeDo 2.0 {Hub.Name}");

        // Main service characteristics
        _portTypeCharacteristic = await Device.GetCharacteristicAsync(GattConstants.WedoService, GattConstants.WedoPortType);
        _buttonCharacteristic = await Device.GetCharacteristicAsync(GattConstants.WedoService, GattConstants.WedoButton);
        _disconnectCharacteristic = await Device.GetCharacteristicAsync(GattConstants.WedoService, GattConstants.WedoDisconnect);

        // Sensor service characteristics
        _sensorCharacteristic = await Device.GetCharacteristicAsync(GattConstants.WedoSensorService, GattConstants.WedoSensorValue);
        _portTypeWriteCharacteristic = await Device.GetCharacteristicAsync(GattConstants.WedoSensorService, GattConstants.WedoPortTypeWrite);
        _motorCharacteristic = await Device.GetCharacteristicAsync(GattConstants.WedoSensorService, GattConstants.WedoMotorWrite);

        // Battery service
        _batteryCharacteristic = await Device.GetCharacteristicAsync(GattConstants.WedoBatteryService, GattConstants.WedoBattery);

        if (_motorCharacteristic == null)
            throw new InvalidOperationException($"WeDo 2.0 motor characteristic not found on {Hub.Name}");

        // Subscribe to notifications
        if (_portTypeCharacteristic != null)
        {
            _portTypeCharacteristic.ValueChanged += OnPortTypeChanged;
            await _portTypeCharacteristic.StartNotificationsAsync();
        }
        if (_sensorCharacteristic != null)
        {
            _sensorCharacteristic.ValueChanged += OnSensorValueChanged;
            await _sensorCharacteristic.StartNotificationsAsync();
        }
        if (_buttonCharacteristic != null)
        {
            _buttonCharacteristic.ValueChanged += OnButtonChanged;
            await _buttonCharacteristic.StartNotificationsAsync();
        }
        if (_batteryCharacteristic != null)
        {
            _batteryCharacteristic.ValueChanged += OnBatteryChanged;
            await _batteryCharacteristic.StartNotificationsAsync();
        }

        Hub.IsConnected = true;
        Device.Disconnected += (_, _) => OnDisconnected();
        Logger.LogInformation("WeDo 2.0 {HubName} connected", Hub.Name);
    }

    public override async Task SetMotorSpeedAsync(string port, int speed)
    {
        var portObj = GetPort(port);
        if (portObj == null) return;

        portObj.Speed = speed;
        Hub.IsBusy = speed != 0;
        await SafeWriteAsync(_motorCharacteristic, [(byte)portObj.Value, 0x01, 0x02, (byte)speed]);
    }

    public override async Task SetLightBrightnessAsync(string port, int brightness)
    {
        var portObj = GetPort(port);
        if (portObj == null) return;

        portObj.Speed = brightness;
        await SafeWriteAsync(_motorCharacteristic, [(byte)portObj.Value, 0x01, 0x02, (byte)brightness]);
    }

    public override async Task SetLedColorAsync(SensorColor color)
    {
        // Activate LED port, then set color
        if (_portTypeWriteCharacteristic != null)
            await SafeWriteAsync(_portTypeWriteCharacteristic, [0x06, 0x17, 0x01, 0x01]);
        await SafeWriteAsync(_motorCharacteristic, [0x06, 0x04, 0x01, (byte)color]);
    }

    public override async Task StopMotorAsync(string port, bool brake = false)
    {
        await SetMotorSpeedAsync(port, 0);
    }

    public override async Task DisconnectAsync()
    {
        if (_disconnectCharacteristic != null)
            await SafeWriteAsync(_disconnectCharacteristic, []);
        await base.DisconnectAsync();
    }

    private void OnPortTypeChanged(object? sender, byte[] data)
    {
        if (data.Length < 2) return;

        var port = GetPortByValue(data[0]);
        if (port == null) return;

        bool connected = data[1] == 1;
        var device = data.Length > 3 ? (PortDevice)data[3] : PortDevice.Unknown;
        OnPortUpdated(port.Id, connected, device);

        if (connected && (device == PortDevice.BoostDistance || device == PortDevice.WeDo2Distance))
        {
            port.Function = port.Function == PortFunction.NotUsed ? PortFunction.Sensor : port.Function;
            _ = ActivatePortDeviceAsync((byte)port.Value, 0x00);
        }
    }

    private void OnSensorValueChanged(object? sender, byte[] data)
    {
        try
        {
            if (data.Length < 2) return;

            if (data[0] == 0x01)
            {
                OnRemoteButtonTriggered(null, RemoteButton.Power);
                return;
            }

            var port = GetPortByValue(data[1]);
            if (port == null || !port.Connected) return;

            switch (port.Device)
            {
                case PortDevice.WeDo2Distance:
                {
                    int distance = data[2];
                    if (data.Length > 3 && data[3] == 1) distance += 255;

                    if (ShouldTriggerDistance(port))
                        OnDistanceTriggered(port.Id, distance);
                    break;
                }
                case PortDevice.BoostDistance:
                {
                    int distance = data[2];
                    if (ShouldTriggerDistance(port))
                        OnDistanceTriggered(port.Id, distance);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error parsing WeDo sensor data from {HubName}", Hub.Name);
        }
    }

    private void OnButtonChanged(object? sender, byte[] data)
    {
        // WeDo 2.0 button press notification
    }

    private void OnBatteryChanged(object? sender, byte[] data)
    {
        if (data.Length > 0)
            OnBatteryUpdated(data[0]);
    }

    private async Task ActivatePortDeviceAsync(byte portValue, byte type)
    {
        if (_portTypeWriteCharacteristic != null)
            await SafeWriteAsync(_portTypeWriteCharacteristic,
                [0x01, 0x02, portValue, type, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x01]);
    }

    private static bool ShouldTriggerDistance(PortModel port) =>
        Environment.TickCount64 - port.LastDistanceTick > port.DistanceColorCooldownMs;
}
