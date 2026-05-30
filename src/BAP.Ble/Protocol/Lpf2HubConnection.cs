using BAP.Core.Enums;
using BAP.Core.Interfaces;
using BAP.Core.Models;
using Microsoft.Extensions.Logging;

namespace BAP.Ble.Protocol;

/// <summary>
/// LPF2 protocol implementation for Powered Up Hub and Boost Move Hub.
/// Handles the shared LEGO Wireless Protocol 3.0 used by these hubs.
/// </summary>
public class Lpf2HubConnection : BleHubConnection
{
    private IBleCharacteristic? _characteristic;

    public Lpf2HubConnection(HubModel hub, IBleAdapter bleAdapter, ILogger<Lpf2HubConnection> logger)
        : base(hub, bleAdapter, logger) { }

    public override async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        Device = await BleAdapter.ConnectAsync(Hub.BluetoothAddress, cancellationToken);
        if (Device == null)
            throw new InvalidOperationException($"Failed to connect to {Hub.Name}");

        _characteristic = await Device.GetCharacteristicAsync(
            GattConstants.Lpf2Service, GattConstants.Lpf2Characteristic);

        if (_characteristic == null)
            throw new InvalidOperationException($"LPF2 characteristic not found on {Hub.Name}");

        _characteristic.ValueChanged += OnValueChanged;
        await _characteristic.StartNotificationsAsync();

        // Request firmware version, activate button + voltage reports
        await WriteAsync([0x01, 0x03, 0x05]);
        await WriteAsync([0x01, 0x02, 0x02]);
        await WriteAsync([0x01, 0x06, 0x02]);
        await WriteAsync([0x41, 0x3c, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01]);

        Hub.IsConnected = true;
        Device.Disconnected += (_, _) => OnDisconnected();
        Logger.LogInformation("{HubName} ({HubType}) connected", Hub.Name, Hub.Type);
    }

    public override async Task SetMotorSpeedAsync(string port, int speed)
    {
        var portObj = GetPort(port);
        if (portObj == null) return;

        byte[] message;
        if (Hub.Type == HubType.BoostMoveHub && port == "AB")
            message = [0x81, (byte)portObj.Value, 0x11, 0x02, (byte)speed, (byte)speed, 0x64, 0x7f, 0x03];
        else if (Hub.Type == HubType.BoostMoveHub)
            message = [0x81, (byte)portObj.Value, 0x11, 0x01, (byte)speed, 0x64, 0x7f, 0x03];
        else if (port == "AB")
            message = [0x81, 57, 0x11, 0x02, (byte)speed, (byte)speed];
        else
            message = [0x81, (byte)portObj.Value, 0x11, 0x51, 0x00, (byte)speed];

        portObj.Speed = speed;
        Hub.IsBusy = speed != 0 && speed != 127;
        await WriteAsync(message);
    }

    public override async Task SetLightBrightnessAsync(string port, int brightness)
    {
        var portObj = GetPort(port);
        if (portObj == null) return;

        await WriteAsync([0x81, (byte)portObj.Value, 0x11, 0x51, 0x00, (byte)brightness]);
    }

    public override async Task SetLedColorAsync(SensorColor color)
    {
        byte ledPort = Hub.Type == HubType.PoweredUpRemote ? (byte)0x34 : (byte)0x32;
        await WriteAsync([0x41, ledPort, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00]);
        await WriteAsync([0x81, ledPort, 0x11, 0x51, 0x00, (byte)color]);
    }

    public override async Task StopMotorAsync(string port, bool brake = false)
    {
        if (brake && (Hub.Type == HubType.PoweredUpHub || Hub.Type == HubType.BoostMoveHub))
            await SetMotorSpeedAsync(port, 127);
        else
            await SetMotorSpeedAsync(port, 0);
    }

    public override async Task DisconnectAsync()
    {
        if (Hub.Type == HubType.PoweredUpHub || Hub.Type == HubType.BoostMoveHub)
            await WriteAsync([0x02, 0x01]);
        await base.DisconnectAsync();
    }

    // ── Protocol message builders ──

    internal static byte[] BuildMotorMessage(HubType hubType, byte portValue, int speed, bool isAbPort)
    {
        if (hubType == HubType.BoostMoveHub && isAbPort)
            return [0x81, portValue, 0x11, 0x02, (byte)speed, (byte)speed, 0x64, 0x7f, 0x03];
        if (hubType == HubType.BoostMoveHub)
            return [0x81, portValue, 0x11, 0x01, (byte)speed, 0x64, 0x7f, 0x03];
        if (isAbPort)
            return [0x81, 57, 0x11, 0x02, (byte)speed, (byte)speed];
        return [0x81, portValue, 0x11, 0x51, 0x00, (byte)speed];
    }

    internal static byte[] BuildLedMessage(byte ledPort, SensorColor color) =>
        [0x81, ledPort, 0x11, 0x51, 0x00, (byte)color];

    // ── Message parsing ──

    private void OnValueChanged(object? sender, byte[] data)
    {
        try
        {
            ParseLpf2Stream(data);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error parsing LPF2 message from {HubName}", Hub.Name);
        }
    }

    private void ParseLpf2Stream(byte[] raw)
    {
        int offset = 0;
        while (offset < raw.Length)
        {
            int len = raw[offset];
            if (len < 1 || offset + len > raw.Length) break;

            var message = new byte[len];
            Array.Copy(raw, offset, message, 0, len);
            offset += len;

            if (message.Length < 3) continue;

            switch (message[2])
            {
                case 0x01: ParseDeviceInfo(message); break;
                case 0x04: ParsePortMessage(message); break;
                case 0x45: ParseSensorMessage(message); break;
                case 0x82: break; // port action feedback
            }
        }
    }

    private void ParseDeviceInfo(byte[] data)
    {
        if (data.Length < 6) return;

        if (data[3] == 0x02 && data[5] == 1)
        {
            OnRemoteButtonTriggered(null, RemoteButton.Power);
        }
        else if (data[3] == 0x06)
        {
            OnBatteryUpdated(data[5]);
        }
    }

    private void ParsePortMessage(byte[] data)
    {
        if (data.Length < 5) return;

        var port = GetPortByValue(data[3]);
        if (port == null) return;

        bool connected = data[4] == 1 || data[4] == 2;
        var device = data.Length >= 6 ? (PortDevice)data[5] : PortDevice.Unknown;
        OnPortUpdated(port.Id, connected, device);

        if (connected)
            RegisterDeviceAttachment(port, device);
    }

    private void ParseSensorMessage(byte[] data)
    {
        if (data.Length < 5) return;

        // Voltage reports
        if (data[3] == 0x3b && Hub.Type == HubType.PoweredUpRemote)
        {
            if (data.Length >= 6)
                OnBatteryUpdated(Hub.BatteryLevel, BitConverter.ToUInt16(data, 4) / 500.0);
            return;
        }
        if (data[3] == 0x3c)
        {
            if (data.Length >= 6)
                OnBatteryUpdated(Hub.BatteryLevel, BitConverter.ToUInt16(data, 4) / 400.0);
            return;
        }

        var port = GetPortByValue(data[3]);
        if (port == null || !port.Connected) return;

        switch (port.Device)
        {
            case PortDevice.WeDo2Distance:
            {
                int distance = data[4];
                if (data.Length > 5 && data[5] == 1) distance += 255;

                if (ShouldTriggerDistance(port))
                    OnDistanceTriggered(port.Id, distance);
                break;
            }
            case PortDevice.BoostDistance:
            {
                if (data.Length < 8) break;

                double distance = data[5] * 1.5;
                double partial = data[7];
                if (partial > 0) distance += 1.0 / partial;

                if (ShouldTriggerDistance(port))
                    OnDistanceTriggered(port.Id, (int)distance);

                if (partial > 5)
                {
                    var color = (SensorColor)data[4];
                    if (ShouldTriggerColor(port))
                        OnColorTriggered(port.Id, color);
                }
                break;
            }
            case PortDevice.PoweredUpRemoteButton:
            {
                var button = data[4] switch
                {
                    0x01 => RemoteButton.Plus,
                    0xff => RemoteButton.Minus,
                    0x7f => RemoteButton.Stop,
                    0x00 => RemoteButton.Released,
                    _ => (RemoteButton?)null
                };
                if (button.HasValue)
                    OnRemoteButtonTriggered(port.Id, button.Value);
                break;
            }
        }
    }

    private void RegisterDeviceAttachment(PortModel port, PortDevice device)
    {
        if (device == PortDevice.TrainMotor && port.Function == PortFunction.NotUsed)
            port.Function = PortFunction.TrainMotor;
        else if (IsMotorDevice(device) && port.Function == PortFunction.NotUsed)
            port.Function = PortFunction.Motor;
        else if ((device == PortDevice.BoostDistance || device == PortDevice.WeDo2Distance) && port.Function == PortFunction.NotUsed)
            port.Function = PortFunction.Sensor;
        else if (device == PortDevice.PoweredUpRemoteButton)
            port.Function = PortFunction.Button;
        else if (device == PortDevice.LedLights && port.Function == PortFunction.NotUsed)
            port.Function = PortFunction.Light;

        // Activate sensor reports for distance/color sensors
        if (device == PortDevice.BoostDistance || device == PortDevice.WeDo2Distance || device == PortDevice.PoweredUpRemoteButton)
        {
            byte sensorType = device == PortDevice.BoostDistance ? (byte)0x08 : (byte)0x00;
            _ = ActivatePortDeviceAsync((byte)port.Value, sensorType);
        }
    }

    private async Task ActivatePortDeviceAsync(byte portValue, byte type)
    {
        await WriteAsync([0x01, 0x00], addLength: false);
        await WriteAsync([0x41, portValue, type, 0x01, 0x00, 0x00, 0x00, 0x01]);
    }

    private static bool IsMotorDevice(PortDevice device) => device is
        PortDevice.BasicMotor or PortDevice.TrainMotor or PortDevice.BoostMotor or
        PortDevice.BoostExtMotor or PortDevice.EV3Motor or
        PortDevice.ControlPlusLMotor or PortDevice.ControlPlusXlMotor;

    private static bool ShouldTriggerDistance(PortModel port) =>
        Environment.TickCount64 - port.LastDistanceTick > port.DistanceColorCooldownMs;

    private static bool ShouldTriggerColor(PortModel port) =>
        Environment.TickCount64 - port.LastColorTick > port.DistanceColorCooldownMs;

    private async Task WriteAsync(byte[] message, bool addLength = true)
    {
        if (_characteristic == null) return;

        byte[] payload;
        if (addLength)
        {
            payload = new byte[message.Length + 2];
            payload[0] = (byte)(message.Length + 2);
            payload[1] = 0x00;
            Array.Copy(message, 0, payload, 2, message.Length);
        }
        else
        {
            payload = message;
        }

        await SafeWriteAsync(_characteristic, payload);
    }
}
