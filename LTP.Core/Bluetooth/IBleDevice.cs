using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegoTrainProject.Bluetooth
{
    /// <summary>
    /// Abstraction for a connected Bluetooth LE device.
    /// </summary>
    public interface IBleDevice : IDisposable
    {
        /// <summary>
        /// Gets the Bluetooth address of the device.
        /// </summary>
        ulong BluetoothAddress { get; }

        /// <summary>
        /// Gets the device name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets whether the device is connected.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Raised when the connection state changes.
        /// </summary>
        event EventHandler<BleConnectionChangedEventArgs> ConnectionChanged;

        /// <summary>
        /// Discovers all GATT services on the device.
        /// </summary>
        Task<IReadOnlyList<IBleService>> GetServicesAsync();

        /// <summary>
        /// Gets a specific GATT service by UUID.
        /// </summary>
        Task<IBleService> GetServiceAsync(Guid serviceUuid);

        /// <summary>
        /// Disconnects from the device.
        /// </summary>
        Task DisconnectAsync();
    }

    /// <summary>
    /// Event args for connection state changes.
    /// </summary>
    public class BleConnectionChangedEventArgs : EventArgs
    {
        public bool IsConnected { get; }
        public string Reason { get; }

        public BleConnectionChangedEventArgs(bool isConnected, string reason = null)
        {
            IsConnected = isConnected;
            Reason = reason;
        }
    }

    /// <summary>
    /// Abstraction for a GATT service.
    /// </summary>
    public interface IBleService : IDisposable
    {
        /// <summary>
        /// Gets the service UUID.
        /// </summary>
        Guid Uuid { get; }

        /// <summary>
        /// Discovers all characteristics of this service.
        /// </summary>
        Task<IReadOnlyList<IBleCharacteristic>> GetCharacteristicsAsync();

        /// <summary>
        /// Gets a specific characteristic by UUID.
        /// </summary>
        Task<IBleCharacteristic> GetCharacteristicAsync(Guid characteristicUuid);
    }

    /// <summary>
    /// Abstraction for a GATT characteristic.
    /// </summary>
    public interface IBleCharacteristic : IDisposable
    {
        /// <summary>
        /// Gets the characteristic UUID.
        /// </summary>
        Guid Uuid { get; }

        /// <summary>
        /// Gets the characteristic properties.
        /// </summary>
        BleCharacteristicProperties Properties { get; }

        /// <summary>
        /// Raised when the characteristic value changes (notifications/indications).
        /// </summary>
        event EventHandler<BleCharacteristicValueChangedEventArgs> ValueChanged;

        /// <summary>
        /// Reads the current value of the characteristic.
        /// </summary>
        Task<byte[]> ReadAsync();

        /// <summary>
        /// Writes a value to the characteristic.
        /// </summary>
        Task<bool> WriteAsync(byte[] data, bool withResponse = true);

        /// <summary>
        /// Enables notifications for this characteristic.
        /// </summary>
        Task<bool> EnableNotificationsAsync();

        /// <summary>
        /// Disables notifications for this characteristic.
        /// </summary>
        Task<bool> DisableNotificationsAsync();
    }

    /// <summary>
    /// Event args for characteristic value changes.
    /// </summary>
    public class BleCharacteristicValueChangedEventArgs : EventArgs
    {
        public byte[] Value { get; }

        public BleCharacteristicValueChangedEventArgs(byte[] value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// GATT characteristic properties.
    /// </summary>
    [Flags]
    public enum BleCharacteristicProperties
    {
        None = 0,
        Broadcast = 1,
        Read = 2,
        WriteWithoutResponse = 4,
        Write = 8,
        Notify = 16,
        Indicate = 32,
        AuthenticatedSignedWrites = 64,
        ExtendedProperties = 128
    }
}
