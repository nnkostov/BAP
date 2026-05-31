using System.Text.Json;
using System.Text.Json.Serialization;
using BAP.Core.Interfaces;
using BAP.Core.Models;
using Microsoft.Extensions.Logging;

namespace BAP.Core.Services;

/// <summary>
/// Loads and saves BAP projects as JSON files (replaces BinaryFormatter serialization).
/// </summary>
public class JsonProjectRepository : IProjectRepository
{
    private readonly ILogger<JsonProjectRepository> _logger;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public JsonProjectRepository(ILogger<JsonProjectRepository> logger)
    {
        _logger = logger;
    }

    public async Task<TrainProjectModel?> LoadAsync(string path, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var stream = File.OpenRead(path);
            var project = await JsonSerializer.DeserializeAsync<TrainProjectModel>(stream, SerializerOptions, cancellationToken);
            _logger.LogInformation("Loaded project from {Path}", path);
            return project;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not open project file: {Path}", path);
            return null;
        }
    }

    public async Task SaveAsync(TrainProjectModel project, string path, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var stream = File.Create(path);
            await JsonSerializer.SerializeAsync(stream, project, SerializerOptions, cancellationToken);
            _logger.LogInformation("Saved project to {Path}", path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not save project file: {Path}", path);
            throw;
        }
    }

    /// <summary>
    /// Exposes the serializer options for external use (e.g. testing).
    /// </summary>
    public static JsonSerializerOptions Options => SerializerOptions;
}
