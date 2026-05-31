using BAP.Core.Interfaces;

namespace BAP.Ble;

/// <summary>
/// No-op BLE adapter used when no platform-specific adapter is available.
/// Scanning and connection attempts are silently ignored.
/// </summary>
internal sealed class NullBleAdapter : IBleAdapter
{
    public bool IsScanning => false;

#pragma warning disable CS0067
    public event EventHandler<BleDeviceDiscoveredEventArgs>? DeviceDiscovered;
#pragma warning restore CS0067

    public Task StartScanningAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task StopScanningAsync()
        => Task.CompletedTask;

    public Task<IBleDevice?> ConnectAsync(ulong bluetoothAddress, CancellationToken cancellationToken = default)
        => Task.FromResult<IBleDevice?>(null);
}
