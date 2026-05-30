using BAP.Core.Enums;

namespace BAP.Core.Models;

/// <summary>
/// Settings for limiting which Bluetooth devices can connect (convention mode).
/// </summary>
public class ConnectionLimitSettings
{
    public ConnectionLimitMode Mode { get; set; } = ConnectionLimitMode.None;

    /// <summary>
    /// Newline-separated MAC addresses (hex, no delimiters) when Mode == OnlySetList.
    /// </summary>
    public string AllowedDevices { get; set; } = string.Empty;

    public bool IsMacAddressAllowed(ulong bluetoothAddress, TrainProjectModel project)
    {
        return Mode switch
        {
            ConnectionLimitMode.None => true,
            ConnectionLimitMode.OnlyProject => project.RegisteredTrains.Any(h => h.BluetoothAddress == bluetoothAddress),
            ConnectionLimitMode.OnlySetList => IsInSetList(bluetoothAddress),
            _ => false
        };
    }

    private bool IsInSetList(ulong bluetoothAddress)
    {
        var macAddress = string.Format("{0:X}", bluetoothAddress);
        var lines = AllowedDevices.Split(["\r\n", "\r", "\n"], StringSplitOptions.RemoveEmptyEntries);
        return lines.Any(line => line.Trim() == macAddress);
    }
}
