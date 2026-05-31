#if WINDOWS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace LegoTrainProject.Bluetooth
{
    /// <summary>
    /// Windows WinRT implementation of IBleAdapter.
    /// </summary>
    public class WinRtBleAdapter : IBleAdapter
    {
        private BluetoothLEAdvertisementWatcher _watcher;
        private bool _disposed;

        public event EventHandler<BleDeviceDiscoveredEventArgs> DeviceDiscovered;
        public event EventHandler<BleAdvertisementEventArgs> AdvertisementReceived;

        public bool IsScanning => _watcher?.Status == BluetoothLEAdvertisementWatcherStatus.Started;

        public WinRtBleAdapter()
        {
            _watcher = new BluetoothLEAdvertisementWatcher
            {
                ScanningMode = BluetoothLEScanningMode.Active
            };
            _watcher.Received += OnAdvertisementReceived;
        }

        public Task StartScanningAsync()
        {
            if (_watcher.Status != BluetoothLEAdvertisementWatcherStatus.Started)
            {
                _watcher.Start();
            }
            return Task.CompletedTask;
        }

        public Task StopScanningAsync()
        {
            if (_watcher.Status == BluetoothLEAdvertisementWatcherStatus.Started)
            {
                _watcher.Stop();
            }
            return Task.CompletedTask;
        }

        public async Task<IBleDevice> ConnectAsync(ulong bluetoothAddress)
        {
            var device = await BluetoothLEDevice.FromBluetoothAddressAsync(bluetoothAddress);
            if (device == null)
                return null;
            return new WinRtBleDevice(device);
        }

        public async Task<IBleDevice> ConnectAsync(string deviceId)
        {
            var device = await BluetoothLEDevice.FromIdAsync(deviceId);
            if (device == null)
                return null;
            return new WinRtBleDevice(device);
        }

        private void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            var manufacturerData = new Dictionary<ushort, byte[]>();
            foreach (var section in args.Advertisement.ManufacturerData)
            {
                var data = new byte[section.Data.Length];
                using (var reader = Windows.Storage.Streams.DataReader.FromBuffer(section.Data))
                {
                    reader.ReadBytes(data);
                }
                manufacturerData[section.CompanyId] = data;
            }

            var serviceUuids = args.Advertisement.ServiceUuids.ToList();

            AdvertisementReceived?.Invoke(this, new BleAdvertisementEventArgs(
                args.BluetoothAddress,
                args.Advertisement.LocalName,
                args.RawSignalStrengthInDBm,
                manufacturerData,
                serviceUuids
            ));

            byte[] firstManufacturerData = manufacturerData.Values.FirstOrDefault();
            DeviceDiscovered?.Invoke(this, new BleDeviceDiscoveredEventArgs(
                args.BluetoothAddress,
                args.Advertisement.LocalName,
                args.RawSignalStrengthInDBm,
                firstManufacturerData
            ));
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                if (_watcher != null)
                {
                    _watcher.Received -= OnAdvertisementReceived;
                    _watcher.Stop();
                    _watcher = null;
                }
            }
        }
    }

    /// <summary>
    /// Windows WinRT implementation of IBleDevice.
    /// </summary>
    public class WinRtBleDevice : IBleDevice
    {
        private BluetoothLEDevice _device;
        private bool _disposed;

        public ulong BluetoothAddress => _device?.BluetoothAddress ?? 0;
        public string Name => _device?.Name;
        public bool IsConnected => _device?.ConnectionStatus == BluetoothConnectionStatus.Connected;

        public event EventHandler<BleConnectionChangedEventArgs> ConnectionChanged;

        internal WinRtBleDevice(BluetoothLEDevice device)
        {
            _device = device;
            _device.ConnectionStatusChanged += OnConnectionStatusChanged;
        }

        public async Task<IReadOnlyList<IBleService>> GetServicesAsync()
        {
            var result = await _device.GetGattServicesAsync(BluetoothCacheMode.Uncached);
            if (result.Status != GattCommunicationStatus.Success)
                return Array.Empty<IBleService>();

            return result.Services.Select(s => new WinRtBleService(s)).ToList();
        }

        public async Task<IBleService> GetServiceAsync(Guid serviceUuid)
        {
            var result = await _device.GetGattServicesForUuidAsync(serviceUuid, BluetoothCacheMode.Uncached);
            if (result.Status != GattCommunicationStatus.Success || result.Services.Count == 0)
                return null;

            return new WinRtBleService(result.Services[0]);
        }

        public Task DisconnectAsync()
        {
            Dispose();
            return Task.CompletedTask;
        }

        private void OnConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            ConnectionChanged?.Invoke(this, new BleConnectionChangedEventArgs(IsConnected));
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                if (_device != null)
                {
                    _device.ConnectionStatusChanged -= OnConnectionStatusChanged;
                    _device.Dispose();
                    _device = null;
                }
            }
        }
    }

    /// <summary>
    /// Windows WinRT implementation of IBleService.
    /// </summary>
    public class WinRtBleService : IBleService
    {
        private GattDeviceService _service;
        private bool _disposed;

        public Guid Uuid => _service?.Uuid ?? Guid.Empty;

        internal WinRtBleService(GattDeviceService service)
        {
            _service = service;
        }

        public async Task<IReadOnlyList<IBleCharacteristic>> GetCharacteristicsAsync()
        {
            var result = await _service.GetCharacteristicsAsync(BluetoothCacheMode.Uncached);
            if (result.Status != GattCommunicationStatus.Success)
                return Array.Empty<IBleCharacteristic>();

            return result.Characteristics.Select(c => new WinRtBleCharacteristic(c)).ToList();
        }

        public async Task<IBleCharacteristic> GetCharacteristicAsync(Guid characteristicUuid)
        {
            var result = await _service.GetCharacteristicsForUuidAsync(characteristicUuid, BluetoothCacheMode.Uncached);
            if (result.Status != GattCommunicationStatus.Success || result.Characteristics.Count == 0)
                return null;

            return new WinRtBleCharacteristic(result.Characteristics[0]);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _service?.Dispose();
                _service = null;
            }
        }
    }

    /// <summary>
    /// Windows WinRT implementation of IBleCharacteristic.
    /// </summary>
    public class WinRtBleCharacteristic : IBleCharacteristic
    {
        private GattCharacteristic _characteristic;
        private bool _disposed;
        private bool _notificationsEnabled;

        public Guid Uuid => _characteristic?.Uuid ?? Guid.Empty;

        public BleCharacteristicProperties Properties
        {
            get
            {
                if (_characteristic == null)
                    return BleCharacteristicProperties.None;

                var props = BleCharacteristicProperties.None;
                var winProps = _characteristic.CharacteristicProperties;

                if (winProps.HasFlag(GattCharacteristicProperties.Broadcast))
                    props |= BleCharacteristicProperties.Broadcast;
                if (winProps.HasFlag(GattCharacteristicProperties.Read))
                    props |= BleCharacteristicProperties.Read;
                if (winProps.HasFlag(GattCharacteristicProperties.WriteWithoutResponse))
                    props |= BleCharacteristicProperties.WriteWithoutResponse;
                if (winProps.HasFlag(GattCharacteristicProperties.Write))
                    props |= BleCharacteristicProperties.Write;
                if (winProps.HasFlag(GattCharacteristicProperties.Notify))
                    props |= BleCharacteristicProperties.Notify;
                if (winProps.HasFlag(GattCharacteristicProperties.Indicate))
                    props |= BleCharacteristicProperties.Indicate;

                return props;
            }
        }

        public event EventHandler<BleCharacteristicValueChangedEventArgs> ValueChanged;

        internal WinRtBleCharacteristic(GattCharacteristic characteristic)
        {
            _characteristic = characteristic;
        }

        public async Task<byte[]> ReadAsync()
        {
            var result = await _characteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
            if (result.Status != GattCommunicationStatus.Success)
                return null;

            var data = new byte[result.Value.Length];
            using (var reader = Windows.Storage.Streams.DataReader.FromBuffer(result.Value))
            {
                reader.ReadBytes(data);
            }
            return data;
        }

        public async Task<bool> WriteAsync(byte[] data, bool withResponse = true)
        {
            var writer = new Windows.Storage.Streams.DataWriter();
            writer.WriteBytes(data);

            var writeOption = withResponse
                ? GattWriteOption.WriteWithResponse
                : GattWriteOption.WriteWithoutResponse;

            var result = await _characteristic.WriteValueAsync(writer.DetachBuffer(), writeOption);
            return result == GattCommunicationStatus.Success;
        }

        public async Task<bool> EnableNotificationsAsync()
        {
            if (_notificationsEnabled)
                return true;

            var cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Notify;
            var result = await _characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(cccdValue);

            if (result == GattCommunicationStatus.Success)
            {
                _notificationsEnabled = true;
                _characteristic.ValueChanged += OnValueChanged;
                return true;
            }
            return false;
        }

        public async Task<bool> DisableNotificationsAsync()
        {
            if (!_notificationsEnabled)
                return true;

            var cccdValue = GattClientCharacteristicConfigurationDescriptorValue.None;
            var result = await _characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(cccdValue);

            if (result == GattCommunicationStatus.Success)
            {
                _notificationsEnabled = false;
                _characteristic.ValueChanged -= OnValueChanged;
                return true;
            }
            return false;
        }

        private void OnValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            var data = new byte[args.CharacteristicValue.Length];
            using (var reader = Windows.Storage.Streams.DataReader.FromBuffer(args.CharacteristicValue))
            {
                reader.ReadBytes(data);
            }
            ValueChanged?.Invoke(this, new BleCharacteristicValueChangedEventArgs(data));
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                if (_notificationsEnabled && _characteristic != null)
                {
                    _characteristic.ValueChanged -= OnValueChanged;
                }
                _characteristic = null;
            }
        }
    }
}
#endif
