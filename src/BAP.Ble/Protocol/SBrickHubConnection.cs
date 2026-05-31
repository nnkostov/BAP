using BAP.Core.Enums;
using BAP.Core.Interfaces;
using BAP.Core.Models;
using Microsoft.Extensions.Logging;

namespace BAP.Ble.Protocol;

/// <summary>
/// SBrick protocol implementation. SBrick uses its own BLE protocol with
/// a dedicated commands characteristic and periodic ADC sensor polling.
/// </summary>
public class SBrickHubConnection : BleHubConnection
{
    private IBleCharacteristic? _characteristic;
    private IBleCharacteristic? _commandCharacteristic;
    private Timer? _pingTimer;

    public SBrickHubConnection(HubModel hub, IBleAdapter bleAdapter, ILogger<SBrickHubConnection> logger)
        : base(hub, bleAdapter, logger) { }

    public override async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        Device = await BleAdapter.ConnectAsync(Hub.BluetoothAddress, cancellationToken);
        if (Device == null)
            throw new InvalidOperationException($"Failed to connect to SBrick {Hub.Name}");

        _characteristic = await Device.GetCharacteristicAsync(
            GattConstants.SBrickService, GattConstants.SBrickCharacteristic);
        _commandCharacteristic = await Device.GetCharacteristicAsync(
            GattConstants.SBrickService, GattConstants.SBrickCommands);

        if (_characteristic == null || _commandCharacteristic == null)
            throw new InvalidOperationException($"SBrick characteristics not found on {Hub.Name}");

        _characteristic.ValueChanged += OnValueChanged;
        await _characteristic.StartNotificationsAsync();

        // Enable ADC channels for sensor + voltage monitoring
        await SafeWriteAsync(_commandCharacteristic, [0x2C, 0x01, 0x03, 0x05, 0x07, 0x08]);
        await SafeWriteAsync(_commandCharacteristic, [0x2E, 0x01, 0x03, 0x05, 0x07, 0x08]);

        // Stop all ports initially
        foreach (var port in Hub.RegisteredPorts)
            await StopMotorAsync(port.Id);

        // Start keepalive ping timer (150ms interval)
        _pingTimer = new Timer(_ => _ = PingAsync(), null, 150, 150);

        Hub.IsConnected = true;
        Device.Disconnected += (_, _) => OnDisconnected();
        Logger.LogInformation("SBrick {HubName} connected", Hub.Name);
    }

    public override async Task SetMotorSpeedAsync(string port, int speed)
    {
        var portObj = GetPort(port);
        if (portObj == null) return;

        portObj.Speed = speed;
        Hub.IsBusy = speed != 0;

        byte direction = (byte)(speed > 0 ? 0 : 1);
        byte power = (byte)Math.Abs(speed * 2.5);
        await SafeWriteAsync(_commandCharacteristic, [0x01, (byte)portObj.Value, direction, power]);
    }

    public override async Task SetLightBrightnessAsync(string port, int brightness)
    {
        var portObj = GetPort(port);
        if (portObj == null) return;

        portObj.Speed = brightness;
        await SafeWriteAsync(_commandCharacteristic, [0x01, (byte)portObj.Value, 1, (byte)(brightness * 2.5)]);
    }

    public override Task SetLedColorAsync(SensorColor color)
    {
        // SBrick has no built-in LED to control
        return Task.CompletedTask;
    }

    public override async Task StopMotorAsync(string port, bool brake = false)
    {
        await SetMotorSpeedAsync(port, 0);
    }

    public override async Task DisconnectAsync()
    {
        _pingTimer?.Dispose();
        _pingTimer = null;
        await base.DisconnectAsync();
    }

    /// <summary>
    /// Builds the SBrick motor command: [0x01, channel, direction, power].
    /// </summary>
    internal static byte[] BuildMotorCommand(byte channel, int speed) =>
        [0x01, channel, (byte)(speed > 0 ? 0 : 1), (byte)Math.Abs(speed * 2.5)];

    private async Task PingAsync()
    {
        await SafeWriteAsync(_commandCharacteristic, [0x02]);
    }

    private void OnValueChanged(object? sender, byte[] raw)
    {
        try
        {
            ParseSBrickStream(raw);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error parsing SBrick message from {HubName}", Hub.Name);
        }
    }

    private void ParseSBrickStream(byte[] raw)
    {
        int offset = 0;
        while (offset < raw.Length)
        {
            if (offset >= raw.Length) break;
            byte len = raw[offset];
            if (len < 1 || offset + 1 + len > raw.Length) break;

            var message = new byte[len];
            Array.Copy(raw, offset + 1, message, 0, len);
            offset += 1 + len;

            if (message.Length < 2) continue;

            switch (message[0])
            {
                case 0x04: ParseStatus(message); break;
                case 0x06: ParseAdcData(message); break;
            }
        }
    }

    private void ParseStatus(byte[] message)
    {
        switch (message[1])
        {
            case 0x00: break; // ACK
            case 0x08: Logger.LogWarning("SBrick {HubName}: thermal protection active", Hub.Name); break;
            default: Logger.LogDebug("SBrick status: {Code}", message[1]); break;
        }
    }

    private void ParseAdcData(byte[] message)
    {
        int portId = 0;
        for (int i = 0; i < message.Length - 1; i += 2)
        {
            if (i + 2 >= message.Length) break;
            var val = (short)(((message[i + 1] & 0xF0) >> 4) | (message[i + 2] << 4));
            int channel = message[i + 1] & 0x0F;

            if (channel == 8)
            {
                double battery = val / 4092f * 100f * 2.6f;
                OnBatteryUpdated((int)battery);
            }
            else
            {
                int mappedPortId = channel switch
                {
                    1 => 0, 3 => 2, 5 => 1, _ => 3
                };

                if (mappedPortId < Hub.RegisteredPorts.Count)
                {
                    var port = Hub.RegisteredPorts[mappedPortId];
                    if (port.Function == PortFunction.Sensor && ShouldTriggerDistance(port))
                    {
                        int maxCurrent = (int)(val / 2.85f);
                        port.MaxDistance = maxCurrent;
                        if (port.MinDistance == 0) port.MinDistance = maxCurrent - 50;

                        float distRatio = port.MaxDistance != port.MinDistance
                            ? (float)(val - port.MinDistance) / (port.MaxDistance - port.MinDistance)
                            : 0;
                        int distance = (int)(distRatio * 10f);
                        OnDistanceTriggered(port.Id, distance);
                    }
                }
                portId++;
            }
        }
    }

    private static bool ShouldTriggerDistance(PortModel port) =>
        Environment.TickCount64 - port.LastDistanceTick > port.DistanceColorCooldownMs;
}
