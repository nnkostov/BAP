using BAP.Core.Models;

namespace BAP.Core.Interfaces;

/// <summary>
/// Persistence abstraction for loading and saving BAP projects.
/// </summary>
public interface IProjectRepository
{
    /// <summary>
    /// Load a project from the given file path.
    /// </summary>
    Task<TrainProjectModel?> LoadAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save a project to the given file path.
    /// </summary>
    Task SaveAsync(TrainProjectModel project, string path, CancellationToken cancellationToken = default);
}
