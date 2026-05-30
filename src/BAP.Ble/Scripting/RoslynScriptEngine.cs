using BAP.Core.Interfaces;
using BAP.Core.Models;
using BAP.Core.Services;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Logging;

namespace BAP.Ble.Scripting;

/// <summary>
/// Roslyn-based scripting engine replacing the legacy CSharpCodeProvider.
/// Scripts run with ScriptGlobals as the host object, providing:
///   Hub[i], Sections, Wait(ms), WriteLine(text), Global[]
/// </summary>
public class RoslynScriptEngine : IScriptEngine
{
    private readonly ILogger<RoslynScriptEngine> _logger;

    private static readonly ScriptOptions DefaultOptions = ScriptOptions.Default
        .AddReferences(
            typeof(object).Assembly,
            typeof(Task).Assembly,
            typeof(Enumerable).Assembly,
            typeof(HubModel).Assembly)
        .AddImports(
            "System",
            "System.Linq",
            "System.Threading.Tasks",
            "System.Collections.Generic",
            "BAP.Core.Models",
            "BAP.Core.Enums");

    public RoslynScriptEngine(ILogger<RoslynScriptEngine> logger)
    {
        _logger = logger;
    }

    public async Task ExecuteAsync(
        string code,
        string? globalCode,
        ProgramEventModel programEvent,
        IReadOnlyList<HubModel> hubs,
        SectionsModel sections,
        CancellationToken cancellationToken = default)
    {
        var globals = new ScriptGlobals(
            Array.Empty<IHubConnection>(),
            hubs,
            sections,
            text => _logger.LogInformation("[Script] {Text}", text),
            cancellationToken);

        var fullCode = BuildFullCode(code, globalCode);

        try
        {
            await CSharpScript.RunAsync(
                fullCode,
                DefaultOptions,
                globals,
                typeof(ScriptGlobals),
                cancellationToken);
        }
        catch (CompilationErrorException ex)
        {
            _logger.LogError("Script compilation error: {Errors}",
                string.Join(Environment.NewLine, ex.Diagnostics));
            throw;
        }
    }

    public Task<IReadOnlyList<string>> ValidateAsync(string code, string? globalCode)
    {
        var fullCode = BuildFullCode(code, globalCode);

        try
        {
            var script = CSharpScript.Create(
                fullCode,
                DefaultOptions,
                typeof(ScriptGlobals));

            var diagnostics = script.Compile();
            IReadOnlyList<string> errors = diagnostics
                .Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
                .Select(d => d.GetMessage())
                .ToList();
            return Task.FromResult(errors);
        }
        catch (Exception ex)
        {
            return Task.FromResult<IReadOnlyList<string>>(new[] { ex.Message });
        }
    }

    private static string BuildFullCode(string code, string? globalCode)
    {
        var sb = new System.Text.StringBuilder();

        if (!string.IsNullOrWhiteSpace(globalCode))
            sb.AppendLine(globalCode);

        sb.AppendLine(code);
        return sb.ToString();
    }
}
