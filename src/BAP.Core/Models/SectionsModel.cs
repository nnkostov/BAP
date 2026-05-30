namespace BAP.Core.Models;

/// <summary>
/// The complete track section network for self-driving.
/// </summary>
public class SectionsModel
{
    /// <summary>
    /// All track sections.
    /// </summary>
    public List<SectionModel> Sections { get; set; } = [];

    /// <summary>
    /// All defined paths through the section graph.
    /// </summary>
    public List<PathModel> Paths { get; set; } = [];

    public SectionModel? FindSection(string name)
    {
        return Sections.FirstOrDefault(s => s.Name == name);
    }

    public int FindSectionIndex(string name)
    {
        for (int i = 0; i < Sections.Count; i++)
            if (Sections[i].Name == name)
                return i;
        return -1;
    }
}
