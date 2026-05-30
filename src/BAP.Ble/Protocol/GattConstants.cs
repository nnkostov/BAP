namespace BAP.Ble.Protocol;

/// <summary>
/// GATT service and characteristic UUIDs for each supported hub type.
/// </summary>
internal static class GattConstants
{
    // ── LPF2 (Powered Up / Boost) ──
    public static readonly Guid Lpf2Service = Guid.Parse("00001623-1212-efde-1623-785feabcd123");
    public static readonly Guid Lpf2Characteristic = Guid.Parse("00001624-1212-efde-1623-785feabcd123");

    // ── WeDo 2.0 ──
    public static readonly Guid WedoService = Guid.Parse("00001523-1212-efde-1523-785feabcd123");
    public static readonly Guid WedoPortType = Guid.Parse("00001527-1212-efde-1523-785feabcd123");
    public static readonly Guid WedoButton = Guid.Parse("00001526-1212-efde-1523-785feabcd123");
    public static readonly Guid WedoDisconnect = Guid.Parse("0000152B-1212-efde-1523-785feabcd123");

    public static readonly Guid WedoSensorService = Guid.Parse("00004f0e-1212-EFDE-1523-785FEABCD123");
    public static readonly Guid WedoSensorValue = Guid.Parse("00001560-1212-efde-1523-785feabcd123");
    public static readonly Guid WedoPortTypeWrite = Guid.Parse("00001563-1212-efde-1523-785feabcd123");
    public static readonly Guid WedoMotorWrite = Guid.Parse("00001565-1212-efde-1523-785feabcd123");

    public static readonly Guid WedoBatteryService = Guid.Parse("0000180F-0000-1000-8000-00805F9B34FB");
    public static readonly Guid WedoBattery = Guid.Parse("00002A19-0000-1000-8000-00805F9B34FB");

    // ── SBrick ──
    public static readonly Guid SBrickService = Guid.Parse("4dc591b0-857c-41de-b5f1-15abda665b0c");
    public static readonly Guid SBrickCharacteristic = Guid.Parse("489a6ae0-c1ab-4c9c-bdb2-11d373c1b7fb");
    public static readonly Guid SBrickCommands = Guid.Parse("02b8cbcc-0e25-4bda-8790-a15f53e6010f");

    // ── BuWizz ──
    public static readonly Guid BuWizzService = Guid.Parse("4E050000-74FB-4481-88B3-9919B1676E93");
    public static readonly Guid BuWizzCharacteristic = Guid.Parse("000092d1-0000-1000-8000-00805f9b34fb");

    // ── PFx Brick ──
    public static readonly Guid PFxService = Guid.Parse("49535343-FE7D-4AE5-8FA9-9FAFD205E455");
    public static readonly Guid PFxRxCharacteristic = Guid.Parse("49535343-1E4D-4BD9-BA61-23C647249616");
    public static readonly Guid PFxTxCharacteristic = Guid.Parse("49535343-8841-43F4-A8D4-ECBE34729BB3");
}
