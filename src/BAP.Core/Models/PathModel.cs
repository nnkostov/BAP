namespace BAP.Core.Models;

/// <summary>
/// An ordered sequence of section indices defining a route a train follows.
/// </summary>
public class PathModel
{
    public string Name { get; set; } = "New Path";

    /// <summary>
    /// Ordered section indices that compose this path.
    /// </summary>
    public int[] Sections { get; set; } = [0];

    /// <summary>
    /// Whether the path should loop back to the start.
    /// </summary>
    public bool LoopPath { get; set; }

    public override string ToString() => $"{Name} ({Sections.Length} sections)";

    public string SectionsToString()
    {
        return Sections is { Length: > 0 }
            ? string.Join(", ", Sections)
            : string.Empty;
    }

    /// <summary>
    /// Parse a comma-separated string of section indices.
    /// </summary>
    public bool FromString(string path)
    {
        var parts = path.Split(',', StringSplitOptions.TrimEntries);
        if (parts.Length == 0) return false;

        var parsed = new int[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            if (!int.TryParse(parts[i], out parsed[i]))
                return false;
        }

        Sections = parsed;
        return true;
    }
}
