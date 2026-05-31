using BAP.Core.Enums;
using BAP.Core.Interfaces;
using BAP.Core.Models;
using Microsoft.Extensions.Logging;

namespace BAP.Ble.Protocol;

/// <summary>
/// PFx Brick protocol implementation. Uses a framed serial-over-BLE protocol
/// with [5B 5B 5B ... payload ... 5D 5D 5D] message framing and a 16-byte
/// action structure for motors, lights, and sound.
/// </summary>
public class PFxHubConnection : BleHubConnection
{
    private IBleCharacteristic? _rxCharacteristic;
    private IBleCharacteristic? _txCharacteristic;
    private volatile TaskCompletionSource<byte[]>? _rxTcs = new();

    // PFx command constants
    private const byte PfxCmdTestAction = 0x13;
    private const byte EvtMotorSpeedHiresMask = 0x3F;
    private const byte EvtMotorSpeedHires = 0x80;
    private const byte EvtMotorSpeedHiresRev = 0x40;
    private const byte EvtMotorOutputMask = 0x0F;
    private const byte EvtMotorSetSpd = 0x70;
    private const byte EvtLightfxOnOffToggle = 0x01;
    private const byte EvtTransitionOn = 0x01;
    private const byte EvtTransitionOff = 0x02;
    private const byte EvtSoundPlayOnce = 0x4;

    public PFxHubConnection(HubModel hub, IBleAdapter bleAdapter, ILogger<PFxHubConnection> logger)
        : base(hub, bleAdapter, logger) { }

    public override async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        Device = await BleAdapter.ConnectAsync(Hub.BluetoothAddress, cancellationToken);
        if (Device == null)
            throw new InvalidOperationException($"Failed to connect to PFx {Hub.Name}");

        _rxCharacteristic = await Device.GetCharacteristicAsync(
            GattConstants.PFxService, GattConstants.PFxRxCharacteristic);
        _txCharacteristic = await Device.GetCharacteristicAsync(
            GattConstants.PFxService, GattConstants.PFxTxCharacteristic);

        if (_rxCharacteristic == null || _txCharacteristic == null)
            throw new InvalidOperationException($"PFx characteristics not found on {Hub.Name}");

        _rxCharacteristic.ValueChanged += OnRxValueChanged;
        await _rxCharacteristic.StartNotificationsAsync();

        Hub.IsConnected = true;
        Device.Disconnected += (_, _) => OnDisconnected();
        Logger.LogInformation("PFx {HubName} connected", Hub.Name);
    }

    public override async Task SetMotorSpeedAsync(string port, int speed)
    {
        var portObj = GetPort(port);
        if (portObj == null) return;

        double sf = Math.Clamp(speed, -100.0, 100.0) / 100.0 * 63.0;
        portObj.Speed = speed;

        int si = (int)Math.Abs(sf) & EvtMotorSpeedHiresMask;
        si |= EvtMotorSpeedHires;
        if (sf < 0) si |= EvtMotorSpeedHiresRev;

        int m = PortToMask(port) & EvtMotorOutputMask;
        m |= EvtMotorSetSpd;

        var action = new byte[16];
        action[1] = (byte)m;         // motorActionId
        action[2] = (byte)si;        // motorParam1
        await SendActionAsync(action);
    }

    public override async Task SetLightBrightnessAsync(string port, int brightness)
    {
        await SetMotorSpeedAsync(port, brightness);
    }

    public override Task SetLedColorAsync(SensorColor color)
    {
        // PFx has no addressable LED
        return Task.CompletedTask;
    }

    public override async Task StopMotorAsync(string port, bool brake = false)
    {
        await SetMotorSpeedAsync(port, 0);
    }

    /// <summary>
    /// Toggle lights on/off for specified channels.
    /// </summary>
    public async Task SetLightFxToggleAsync(string channels, bool on)
    {
        var action = new byte[16];
        action[5] = PortToMask(channels);          // lightOutputMask
        action[4] = EvtLightfxOnOffToggle;          // lightFxId
        action[11] = on ? EvtTransitionOn : EvtTransitionOff; // lightParam4
        await SendActionAsync(action);
    }

    /// <summary>
    /// Play an audio file by ID.
    /// </summary>
    public async Task PlayAudioFileAsync(byte fileId)
    {
        var action = new byte[16];
        action[12] = EvtSoundPlayOnce;  // soundFxId
        action[13] = fileId;            // soundFileId
        await SendActionAsync(action);
    }

    // ── Protocol internals ──

    private async Task SendActionAsync(byte[] action)
    {
        var msg = new byte[17];
        msg[0] = PfxCmdTestAction;
        Array.Copy(action, 0, msg, 1, 16);
        await SendFramedDataAsync(msg);
    }

    private async Task SendFramedDataAsync(byte[] data)
    {
        // PFx framing: [5B 5B 5B] + data + [5D 5D 5D]
        var framed = new byte[data.Length + 6];
        framed[0] = framed[1] = framed[2] = 0x5B;
        Array.Copy(data, 0, framed, 3, data.Length);
        framed[^3] = framed[^2] = framed[^1] = 0x5D;

        // Send in 20-byte BLE chunks
        int offset = 0;
        while (offset < framed.Length)
        {
            int chunkSize = Math.Min(framed.Length - offset, 20);
            var chunk = new byte[chunkSize];
            Array.Copy(framed, offset, chunk, 0, chunkSize);
            await SafeWriteAsync(_txCharacteristic, chunk);
            offset += 20;
        }
    }

    private void OnRxValueChanged(object? sender, byte[] data)
    {
        _rxTcs?.TrySetResult(data);
    }

    /// <summary>
    /// Convert channel string ("A", "1,2,3") to PFx bitmask.
    /// </summary>
    internal static byte PortToMask(string channel)
    {
        int mask = 0;
        foreach (char c in channel)
        {
            mask |= c switch
            {
                '1' or 'A' or 'a' => 0x01,
                '2' or 'B' or 'b' => 0x02,
                '3' or 'C' or 'c' => 0x04,
                '4' or 'D' or 'd' => 0x08,
                '5' => 0x10,
                '6' => 0x20,
                '7' => 0x40,
                '8' => 0x80,
                _ => 0
            };
        }
        return (byte)mask;
    }
}
