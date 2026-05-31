using System.Text.Json.Serialization;
using BAP.Core.Enums;

namespace BAP.Core.Models;

/// <summary>
/// Represents a LEGO hub / smart brick.
/// Contains both persisted configuration and transient runtime state.
/// </summary>
public class HubModel
{
    // ── Persisted configuration ──

    /// <summary>
    /// User-visible name of the hub.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Unique device identifier (Bluetooth device ID or COM port).
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// Bluetooth address for reconnection.
    /// </summary>
    public ulong BluetoothAddress { get; set; }

    /// <summary>
    /// Hardware type of this hub.
    /// </summary>
    public HubType Type { get; set; }

    /// <summary>
    /// Registered I/O ports on this hub.
    /// </summary>
    public List<PortModel> RegisteredPorts { get; set; } = [];

    /// <summary>
    /// Which port drives the train motor (if any).
    /// </summary>
    public string? TrainMotorPort { get; set; }

    /// <summary>
    /// Time in ms before a cleared section is released.
    /// </summary>
    public int ClearingTimeInMs { get; set; } = 2000;

    /// <summary>
    /// Speed when approaching a stop signal.
    /// </summary>
    public int SpeedWhenAboutToStop { get; set; } = 40;

    /// <summary>
    /// Battery-based speed multiplier.
    /// </summary>
    public float SpeedCoefficient { get; set; } = 1.0f;

    /// <summary>
    /// Whether the current path should loop.
    /// </summary>
    public bool LoopCurrentPath { get; set; }

    /// <summary>
    /// The path currently assigned to this hub.
    /// </summary>
    public PathModel? CurrentPath { get; set; }

    /// <summary>
    /// LED color setting.
    /// </summary>
    public SensorColor LedColor { get; set; } = SensorColor.Green;

    // ── Runtime-only state (not serialized) ──

    [JsonIgnore] public bool IsConnected { get; set; }
    [JsonIgnore] public bool IsBusy { get; set; }
    [JsonIgnore] public double BatteryLevel { get; set; }
    [JsonIgnore] public double BatteryVoltage { get; set; }
    [JsonIgnore] public int[] State { get; set; } = new int[100];
    [JsonIgnore] public bool IsWaitingSection { get; set; }
    [JsonIgnore] public bool IsPathProgramRunning { get; set; }
    [JsonIgnore] public int CurrentPathPositionIndex { get; set; }
    [JsonIgnore] public bool AbortReserve { get; set; }

    /// <summary>
    /// Checks whether this hub has a port configured as a train motor.
    /// </summary>
    public bool IsTrain()
    {
        return RegisteredPorts.Any(p => p.Function == PortFunction.TrainMotor);
    }
}
