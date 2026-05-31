using System.Text.Json.Serialization;
using BAP.Core.Enums;

namespace BAP.Core.Models;

/// <summary>
/// A single track section in the self-driving network.
/// </summary>
public class SectionModel
{
    /// <summary>
    /// Name of this section.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detector reference string: "portId*deviceId".
    /// </summary>
    public string? Detector { get; set; }

    /// <summary>
    /// Switch reference string: "portId*deviceId".
    /// </summary>
    public string? Switch { get; set; }

    /// <summary>
    /// Name/index of the left (or straight-ahead) next section.
    /// </summary>
    public string? LeftSectionName { get; set; }

    /// <summary>
    /// Name/index of the right next section.
    /// </summary>
    public string? RightSectionName { get; set; }

    /// <summary>
    /// Maximum speed allowed on this section.
    /// </summary>
    public int MaxSpeed { get; set; } = 100;

    /// <summary>
    /// Whether the train needs two sections ahead to be free before moving.
    /// </summary>
    public bool NeedsTwoSectionsReleased { get; set; }

    /// <summary>
    /// What happens when the train exits this section.
    /// </summary>
    public SectionReleaseAction Action { get; set; } = SectionReleaseAction.ResumeSpeed;

    /// <summary>
    /// Custom code event to execute on release (if Action == ExecuteCode).
    /// </summary>
    public ProgramEventModel? CustomCodeEvent { get; set; }

    // ── Runtime-only state ──

    [JsonIgnore] public HubModel? CurrentHub { get; set; }
    [JsonIgnore] public HubModel? ReservedBy { get; set; }
    [JsonIgnore] public bool IsBeingCleared { get; set; }

    public string? GetDetectorDeviceId() => ParsePart(Detector, 1);
    public string? GetDetectorPort() => ParsePart(Detector, 0);
    public string? GetSwitchDeviceId() => ParsePart(Switch, 1);
    public string? GetSwitchPort() => ParsePart(Switch, 0);

    public bool IsDetectorPresent(string portId, string deviceId)
    {
        return GetDetectorPort() == portId && GetDetectorDeviceId() == deviceId;
    }

    public bool IsSwitchPresent(string portId, string deviceId)
    {
        return GetSwitchPort() == portId && GetSwitchDeviceId() == deviceId;
    }

    public override string ToString() => Name;

    private static string? ParsePart(string? reference, int index)
    {
        if (reference == null) return null;
        var parts = reference.Split('*');
        return parts.Length > index ? parts[index] : null;
    }
}
