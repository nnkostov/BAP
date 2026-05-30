using System.Text.Json.Serialization;
using BAP.Core.Enums;

namespace BAP.Core.Models;

/// <summary>
/// Represents a single I/O port on a hub.
/// Persistent properties are serialized; runtime state is excluded.
/// </summary>
public class PortModel
{
    /// <summary>
    /// Port identifier (e.g. "A", "B", "One").
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Numeric value / index of the port on the hub.
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// Type of device attached to this port.
    /// </summary>
    public PortDevice Device { get; set; } = PortDevice.Unknown;

    /// <summary>
    /// Configured role/function of this port.
    /// </summary>
    public PortFunction Function { get; set; } = PortFunction.NotUsed;

    /// <summary>
    /// Target speed for this port's motor (persisted).
    /// </summary>
    public int TargetSpeed { get; set; } = 100;

    /// <summary>
    /// Sensor trigger cooldown in milliseconds.
    /// </summary>
    public int DistanceColorCooldownMs { get; set; } = 2000;

    /// <summary>
    /// Minimum distance calibration value.
    /// </summary>
    public int MinDistance { get; set; }

    /// <summary>
    /// Maximum distance calibration value.
    /// </summary>
    public int MaxDistance { get; set; }

    // ── Runtime-only state (not serialized) ──

    [JsonIgnore] public bool Connected { get; set; }
    [JsonIgnore] public bool Busy { get; set; }
    [JsonIgnore] public int Speed { get; set; }
    [JsonIgnore] public SensorColor LatestColor { get; set; } = SensorColor.Black;
    [JsonIgnore] public long LastColorTick { get; set; }
    [JsonIgnore] public int LatestDistance { get; set; }
    [JsonIgnore] public long LastDistanceTick { get; set; }
}
