using BAP.Core.Enums;
using BAP.Core.Interfaces;
using BAP.Core.Models;
using Microsoft.Extensions.Logging;

namespace BAP.Ble.Protocol;

/// <summary>
/// BuWizz hub protocol implementation. Simple 4-channel motor controller
/// with a single command characteristic. All motor speeds are sent as a
/// 6-byte packet: [0x10, chA, chB, chC, chD, 0x00].
/// </summary>
public class BuWizzHubConnection : BleHubConnection
{
    private IBleCharacteristic? _characteristic;

    public BuWizzHubConnection(HubModel hub, IBleAdapter bleAdapter, ILogger<BuWizzHubConnection> logger)
        : base(hub, bleAdapter, logger) { }

    public override async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        Device = await BleAdapter.ConnectAsync(Hub.BluetoothAddress, cancellationToken);
        if (Device == null)
            throw new InvalidOperationException($"Failed to connect to BuWizz {Hub.Name}");

        _characteristic = await Device.GetCharacteristicAsync(
            GattConstants.BuWizzService, GattConstants.BuWizzCharacteristic);

        if (_characteristic == null)
            throw new InvalidOperationException($"BuWizz characteristic not found on {Hub.Name}");

        // Set power level to max (4)
        await SafeWriteAsync(_characteristic, [0x11, 4], withResponse: true);

        Hub.IsConnected = true;
        Device.Disconnected += (_, _) => OnDisconnected();
        Logger.LogInformation("BuWizz {HubName} connected", Hub.Name);
    }

    public override async Task SetMotorSpeedAsync(string port, int speed)
    {
        var portObj = GetPort(port);
        if (portObj == null) return;

        portObj.Speed = speed;
        Hub.IsBusy = speed != 0;

        await SendAllMotorSpeedsAsync();
    }

    public override async Task SetLightBrightnessAsync(string port, int brightness)
    {
        var portObj = GetPort(port);
        if (portObj == null) return;

        portObj.Speed = brightness;
        await SendAllMotorSpeedsAsync();
    }

    public override Task SetLedColorAsync(SensorColor color)
    {
        // BuWizz has no addressable LED
        return Task.CompletedTask;
    }

    public override async Task StopMotorAsync(string port, bool brake = false)
    {
        await SetMotorSpeedAsync(port, 0);
    }

    /// <summary>
    /// Builds the BuWizz motor packet: [0x10, portA, portB, portC, portD, 0x00].
    /// </summary>
    internal byte[] BuildMotorPacket()
    {
        var data = new byte[6];
        data[0] = 0x10;
        for (int i = 0; i < Hub.RegisteredPorts.Count && i < 4; i++)
            data[i + 1] = (byte)Hub.RegisteredPorts[i].Speed;
        data[5] = 0;
        return data;
    }

    private async Task SendAllMotorSpeedsAsync()
    {
        await SafeWriteAsync(_characteristic, BuildMotorPacket(), withResponse: true);
    }
}
