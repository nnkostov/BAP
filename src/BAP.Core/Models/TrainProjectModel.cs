using System.Text.Json.Serialization;
using BAP.Core.Enums;

namespace BAP.Core.Models;

/// <summary>
/// Root aggregate – a BAP project containing hubs, programs, sections, and global code.
/// This is the top-level object that gets serialized to a .bap JSON file.
/// </summary>
public class TrainProjectModel
{
    /// <summary>
    /// All registered hubs/devices in this project.
    /// </summary>
    public List<HubModel> RegisteredTrains { get; set; } = [];

    /// <summary>
    /// All automation programs in this project.
    /// </summary>
    public List<TrainProgramModel> Programs { get; set; } = [];

    /// <summary>
    /// Global C# code available to all programs.
    /// </summary>
    public ProgramEventModel GlobalCode { get; set; } = new(ProgramEventType.GlobalCode);

    /// <summary>
    /// Track section network for self-driving.
    /// </summary>
    public SectionsModel Sections { get; set; } = new();

    /// <summary>
    /// Whether to show the self-driving section UI.
    /// </summary>
    public bool ShowSectionProgram { get; set; }

    /// <summary>
    /// Find a hub by its device ID.
    /// </summary>
    public HubModel? GetHubByDeviceId(string deviceId)
    {
        return RegisteredTrains.FirstOrDefault(h => h.DeviceId == deviceId);
    }
}
