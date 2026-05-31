namespace BAP.Core.Interfaces;

/// <summary>
/// Represents a BLE GATT characteristic.
/// </summary>
public interface IBleCharacteristic
{
    Guid Uuid { get; }

    /// <summary>
    /// Write data to the characteristic.
    /// </summary>
    Task WriteAsync(byte[] data, bool withResponse = false);

    /// <summary>
    /// Read data from the characteristic.
    /// </summary>
    Task<byte[]> ReadAsync();

    /// <summary>
    /// Subscribe to value change notifications.
    /// </summary>
    Task StartNotificationsAsync();

    /// <summary>
    /// Unsubscribe from value change notifications.
    /// </summary>
    Task StopNotificationsAsync();

    /// <summary>
    /// Raised when the characteristic value changes (notification received).
    /// </summary>
    event EventHandler<byte[]> ValueChanged;
}
