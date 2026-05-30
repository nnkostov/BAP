namespace BAP.Core.Models;

/// <summary>
/// An automation program consisting of a list of event rules.
/// </summary>
public class TrainProgramModel
{
    /// <summary>
    /// Name of this program.
    /// </summary>
    public string Name { get; set; } = "New Program";

    /// <summary>
    /// All event rules in this program.
    /// </summary>
    public List<ProgramEventModel> Events { get; set; } = [];
}
