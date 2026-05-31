using BAP.Core.Models;

namespace BAP.Core.Interfaces;

/// <summary>
/// Abstraction for runtime C# scripting (replaces CSharpCodeProvider).
/// </summary>
public interface IScriptEngine
{
    /// <summary>
    /// Execute a C# code snippet in the context of the given hubs and sections.
    /// </summary>
    Task ExecuteAsync(
        string code,
        string? globalCode,
        ProgramEventModel programEvent,
        IReadOnlyList<HubModel> hubs,
        SectionsModel sections,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate a code snippet without executing it. Returns error messages if any.
    /// </summary>
    Task<IReadOnlyList<string>> ValidateAsync(string code, string? globalCode);
}
