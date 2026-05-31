using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegoTrainProject.Bluetooth
{
    /// <summary>
    /// Abstraction for Bluetooth LE device discovery and connection.
    /// Implementations: WinRtBleAdapter (Windows), BlueZBleAdapter (Linux)
    /// </summary>
    public interface IBleAdapter : IDisposable
    {
        /// <summary>
        /// Raised when a new BLE device is discovered during scanning.
        /// </summary>
        event EventHandler<BleDeviceDiscoveredEventArgs> DeviceDiscovered;

        /// <summary>
        /// Raised when an advertisement is received from a device.
        /// </summary>
        event EventHandler<BleAdvertisementEventArgs> AdvertisementReceived;

        /// <summary>
        /// Gets whether the adapter is currently scanning.
        /// </summary>
        bool IsScanning { get; }

        /// <summary>
        /// Starts scanning for BLE devices.
        /// </summary>
        Task StartScanningAsync();

        /// <summary>
        /// Stops scanning for BLE devices.
        /// </summary>
        Task StopScanningAsync();

        /// <summary>
        /// Connects to a BLE device by its address.
        /// </summary>
        Task<IBleDevice> ConnectAsync(ulong bluetoothAddress);

        /// <summary>
        /// Connects to a BLE device by its address string (platform-specific format).
        /// </summary>
        Task<IBleDevice> ConnectAsync(string deviceId);
    }

    /// <summary>
    /// Event args for device discovery.
    /// </summary>
    public class BleDeviceDiscoveredEventArgs : EventArgs
    {
        public ulong BluetoothAddress { get; }
        public string Name { get; }
        public short Rssi { get; }
        public byte[] ManufacturerData { get; }

        public BleDeviceDiscoveredEventArgs(ulong bluetoothAddress, string name, short rssi, byte[] manufacturerData)
        {
            BluetoothAddress = bluetoothAddress;
            Name = name;
            Rssi = rssi;
            ManufacturerData = manufacturerData;
        }
    }

    /// <summary>
    /// Event args for BLE advertisements.
    /// </summary>
    public class BleAdvertisementEventArgs : EventArgs
    {
        public ulong BluetoothAddress { get; }
        public string LocalName { get; }
        public short Rssi { get; }
        public IReadOnlyDictionary<ushort, byte[]> ManufacturerData { get; }
        public IReadOnlyList<Guid> ServiceUuids { get; }

        public BleAdvertisementEventArgs(
            ulong bluetoothAddress,
            string localName,
            short rssi,
            IReadOnlyDictionary<ushort, byte[]> manufacturerData,
            IReadOnlyList<Guid> serviceUuids)
        {
            BluetoothAddress = bluetoothAddress;
            LocalName = localName;
            Rssi = rssi;
            ManufacturerData = manufacturerData;
            ServiceUuids = serviceUuids;
        }
    }
}
