using System;

namespace LegoTrainProject.Constants
{
    /// <summary>
    /// Bluetooth GATT Service and Characteristic UUIDs for LEGO devices.
    /// </summary>
    public static class BluetoothUuids
    {
        // WeDo 2.0 Smart Hub
        public const string WEDO2_SMART_HUB_SERVICE = "00001523-1212-efde-1523-785feabcd123";

        // Powered UP / LPF2 Hub
        public const string LPF2_HUB_SERVICE = "00001623-1212-efde-1623-785feabcd123";
        public const string LPF2_HUB_CHARACTERISTIC = "00001624-1212-efde-1623-785feabcd123";

        // SBrick
        public const string SBRICK_SERVICE = "4dc591b0-857c-41de-b5f1-15abda665b0c";
        public const string SBRICK_CHARACTERISTIC = "02b8cbcc-0e25-4bda-8790-a15f53e6010f";

        // BuWizz
        public const string BUWIZZ_SERVICE = "4e050000-74fb-4481-88b3-9919b1676e93";
        public const string BUWIZZ_CHARACTERISTIC = "000092d1-0000-1000-8000-00805f9b34fb";

        // PFx Brick
        public const string PFX_SERVICE = "5e3d92a3-b28e-4d60-a7f1-c691ee03e2d4";
    }

    /// <summary>
    /// LPF2 (Powered UP) Protocol message types.
    /// </summary>
    public static class Lpf2MessageTypes
    {
        public const byte HUB_PROPERTIES = 0x01;
        public const byte HUB_ACTIONS = 0x02;
        public const byte HUB_ALERTS = 0x03;
        public const byte HUB_ATTACHED_IO = 0x04;
        public const byte GENERIC_ERROR = 0x05;
        public const byte PORT_INPUT_FORMAT_SETUP = 0x41;
        public const byte PORT_INPUT_FORMAT = 0x45;
        public const byte PORT_OUTPUT_COMMAND = 0x81;
        public const byte PORT_OUTPUT_COMMAND_FEEDBACK = 0x82;
    }

    /// <summary>
    /// Hub property types for LPF2 protocol.
    /// </summary>
    public static class Lpf2HubProperties
    {
        public const byte ADVERTISING_NAME = 0x01;
        public const byte BUTTON = 0x02;
        public const byte FW_VERSION = 0x03;
        public const byte HW_VERSION = 0x04;
        public const byte RSSI = 0x05;
        public const byte BATTERY_VOLTAGE = 0x06;
        public const byte BATTERY_TYPE = 0x07;
        public const byte MANUFACTURER_NAME = 0x08;
        public const byte RADIO_FIRMWARE_VERSION = 0x09;
        public const byte PRIMARY_MAC_ADDRESS = 0x0D;
    }

    /// <summary>
    /// Hub action commands for LPF2 protocol.
    /// </summary>
    public static class Lpf2HubActions
    {
        public const byte SWITCH_OFF = 0x01;
        public const byte DISCONNECT = 0x02;
        public const byte VCC_PORT_CONTROL_ON = 0x03;
        public const byte VCC_PORT_CONTROL_OFF = 0x04;
        public const byte ACTIVATE_BUSY_INDICATION = 0x05;
        public const byte RESET_BUSY_INDICATION = 0x06;
    }

    /// <summary>
    /// Port output sub-commands for LPF2 protocol.
    /// </summary>
    public static class Lpf2PortOutputSubCommands
    {
        public const byte START_POWER = 0x01;
        public const byte START_POWER_PAIR = 0x02;
        public const byte SET_ACC_TIME = 0x05;
        public const byte SET_DEC_TIME = 0x06;
        public const byte START_SPEED = 0x07;
        public const byte START_SPEED_PAIR = 0x08;
        public const byte START_SPEED_FOR_TIME = 0x09;
        public const byte START_SPEED_FOR_TIME_PAIR = 0x0A;
        public const byte START_SPEED_FOR_DEGREES = 0x0B;
        public const byte START_SPEED_FOR_DEGREES_PAIR = 0x0C;
        public const byte GOTO_ABS_POSITION = 0x0D;
        public const byte GOTO_ABS_POSITION_PAIR = 0x0E;
        public const byte WRITE_DIRECT_MODE_DATA = 0x51;
    }

    /// <summary>
    /// Motor control constants.
    /// </summary>
    public static class MotorConstants
    {
        public const byte BRAKE = 127;
        public const byte FLOAT = 0;
        public const int MAX_POWER = 100;
        public const int MIN_POWER = -100;
    }

    /// <summary>
    /// Default port values for different hub types.
    /// </summary>
    public static class DefaultPorts
    {
        // Standard hub ports
        public const byte PORT_A = 0x00;
        public const byte PORT_B = 0x01;
        public const byte PORT_C = 0x02;
        public const byte PORT_D = 0x03;

        // Boost Move Hub internal ports
        public const byte BOOST_PORT_A = 55;
        public const byte BOOST_PORT_B = 56;
        public const byte BOOST_PORT_C = 1;
        public const byte BOOST_PORT_D = 2;

        // Virtual combined ports
        public const byte VIRTUAL_PORT_AB = 57;

        // Internal sensor ports
        public const byte INTERNAL_LED = 0x32;
        public const byte INTERNAL_VOLTAGE = 0x3C;
        public const byte INTERNAL_VOLTAGE_REMOTE = 0x3B;
    }

    /// <summary>
    /// Timing constants for device operations.
    /// </summary>
    public static class TimingConstants
    {
        public const int CONNECTION_TIMEOUT_MS = 1000;
        public const int SWITCH_ACTIVATION_MS = 500;
        public const int SWITCH_DOUBLECROSS_MS = 700;
        public const int DEFAULT_SENSOR_COOLDOWN_MS = 2000;
        public const int SECTION_CLEARING_DEFAULT_MS = 2000;
    }

    /// <summary>
    /// Firmware version thresholds.
    /// </summary>
    public static class FirmwareVersions
    {
        // Boost Move Hub firmware version 1.0.00.0224 as little-endian int32
        public static readonly byte[] BOOST_FW_10000224_BYTES = { 0x24, 0x02, 0x00, 0x10 };
        public static readonly int BOOST_FW_10000224 = BitConverter.ToInt32(BOOST_FW_10000224_BYTES, 0);
    }
}
