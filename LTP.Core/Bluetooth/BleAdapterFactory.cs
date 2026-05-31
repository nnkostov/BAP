using System;
using System.Runtime.InteropServices;

namespace LegoTrainProject.Bluetooth
{
    /// <summary>
    /// Factory for creating platform-specific BLE adapter instances.
    /// </summary>
    public static class BleAdapterFactory
    {
        private static IBleAdapter _instance;
        private static readonly object _lock = new object();

        /// <summary>
        /// Gets the platform-appropriate BLE adapter.
        /// </summary>
        public static IBleAdapter GetAdapter()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = CreatePlatformAdapter();
                    }
                }
            }
            return _instance;
        }

        /// <summary>
        /// Sets a custom adapter (useful for testing or custom implementations).
        /// </summary>
        public static void SetAdapter(IBleAdapter adapter)
        {
            lock (_lock)
            {
                _instance?.Dispose();
                _instance = adapter;
            }
        }

        private static IBleAdapter CreatePlatformAdapter()
        {
#if WINDOWS
            return CreateWindowsAdapter();
#else
            return CreateLinuxAdapter();
#endif
        }

        private static IBleAdapter CreateWindowsAdapter()
        {
            // WinRT adapter - created via reflection to avoid compile-time dependency on Linux
            var type = Type.GetType("LegoTrainProject.Bluetooth.WinRtBleAdapter, LegoTrainProject");
            if (type != null)
            {
                return (IBleAdapter)Activator.CreateInstance(type);
            }
            throw new PlatformNotSupportedException("WinRT Bluetooth adapter not available on this platform.");
        }

        private static IBleAdapter CreateLinuxAdapter()
        {
            // BlueZ adapter - created via reflection to avoid compile-time dependency on Windows
            var type = Type.GetType("LegoTrainProject.Bluetooth.BlueZBleAdapter, LegoTrainProject");
            if (type != null)
            {
                return (IBleAdapter)Activator.CreateInstance(type);
            }
            throw new PlatformNotSupportedException("BlueZ Bluetooth adapter not available on this platform.");
        }

        /// <summary>
        /// Gets the current platform.
        /// </summary>
        public static BleAdapterPlatform CurrentPlatform
        {
            get
            {
#if WINDOWS
                return BleAdapterPlatform.Windows;
#else
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return BleAdapterPlatform.Linux;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    return BleAdapterPlatform.MacOS;
                return BleAdapterPlatform.Unknown;
#endif
            }
        }
    }

    /// <summary>
    /// Supported BLE platforms.
    /// </summary>
    public enum BleAdapterPlatform
    {
        Unknown,
        Windows,
        Linux,
        MacOS
    }
}
