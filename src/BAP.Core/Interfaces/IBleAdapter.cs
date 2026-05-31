namespace BAP.Core.Interfaces;

/// <summary>
/// Platform-agnostic Bluetooth Low Energy adapter for scanning and connecting.
/// </summary>
public interface IBleAdapter
{
    /// <summary>
    /// Start scanning for BLE devices.
    /// </summary>
    Task StartScanningAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stop scanning for BLE devices.
    /// </summary>
    Task StopScanningAsync();

    /// <summary>
    /// Whether scanning is currently active.
    /// </summary>
    bool IsScanning { get; }

    /// <summary>
    /// Raised when a new BLE device is discovered during scanning.
    /// </summary>
    event EventHandler<BleDeviceDiscoveredEventArgs> DeviceDiscovered;

    /// <summary>
    /// Connect to a specific BLE device by address.
    /// </summary>
    Task<IBleDevice?> ConnectAsync(ulong bluetoothAddress, CancellationToken cancellationToken = default);
}

public class BleDeviceDiscoveredEventArgs : EventArgs
{
    public required ulong BluetoothAddress { get; init; }
    public required string Name { get; init; }
    public byte[]? ManufacturerData { get; init; }
}
