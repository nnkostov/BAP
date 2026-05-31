namespace BAP.Core.Interfaces;

/// <summary>
/// Represents a connected BLE device.
/// </summary>
public interface IBleDevice : IDisposable
{
    string Name { get; }
    string DeviceId { get; }
    ulong BluetoothAddress { get; }
    bool IsConnected { get; }

    /// <summary>
    /// Get a GATT characteristic by service and characteristic UUIDs.
    /// </summary>
    Task<IBleCharacteristic?> GetCharacteristicAsync(Guid serviceUuid, Guid characteristicUuid);

    /// <summary>
    /// Disconnect from the device.
    /// </summary>
    Task DisconnectAsync();

    /// <summary>
    /// Raised when the device disconnects unexpectedly.
    /// </summary>
    event EventHandler Disconnected;
}
