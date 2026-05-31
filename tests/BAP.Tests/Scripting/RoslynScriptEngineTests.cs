using BAP.Ble.Scripting;
using BAP.Core.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace BAP.Tests.Scripting;

public class RoslynScriptEngineTests
{
    private readonly RoslynScriptEngine _engine = new(NullLogger<RoslynScriptEngine>.Instance);

    [Fact]
    public async Task ExecuteAsync_SimpleCode_RunsSuccessfully()
    {
        var hubs = new List<HubModel> { new() { Name = "TestHub" } };
        var sections = new SectionsModel();
        var evt = new ProgramEventModel();

        await _engine.ExecuteAsync(
            "var x = 1 + 1;",
            null,
            evt,
            hubs,
            sections);
    }

    [Fact]
    public async Task ExecuteAsync_AccessHub_Works()
    {
        var hubs = new List<HubModel> { new() { Name = "MyHub" } };
        var sections = new SectionsModel();
        var evt = new ProgramEventModel();

        await _engine.ExecuteAsync(
            "var name = Hub[0].Name;",
            null,
            evt,
            hubs,
            sections);
    }

    [Fact]
    public async Task ExecuteAsync_WithGlobalCode_Works()
    {
        var hubs = new List<HubModel>();
        var sections = new SectionsModel();
        var evt = new ProgramEventModel();

        await _engine.ExecuteAsync(
            "var result = Add(2, 3);",
            "int Add(int a, int b) => a + b;",
            evt,
            hubs,
            sections);
    }

    [Fact]
    public async Task ExecuteAsync_WriteLineCaptures_Output()
    {
        var hubs = new List<HubModel>();
        var sections = new SectionsModel();
        var evt = new ProgramEventModel();

        // Should not throw — we're just testing that WriteLine() is callable
        await _engine.ExecuteAsync(
            "WriteLine(\"hello from script\");",
            null,
            evt,
            hubs,
            sections);
    }

    [Fact]
    public async Task ValidateAsync_ValidCode_ReturnsNoErrors()
    {
        var errors = await _engine.ValidateAsync("var x = 42;", null);
        Assert.Empty(errors);
    }

    [Fact]
    public async Task ValidateAsync_InvalidCode_ReturnsErrors()
    {
        var errors = await _engine.ValidateAsync("var x = ;", null);
        Assert.NotEmpty(errors);
    }

    [Fact]
    public async Task ValidateAsync_WithGlobalCode_ValidatesBoth()
    {
        var errors = await _engine.ValidateAsync(
            "var x = MyConst;",
            "const int MyConst = 42;");
        Assert.Empty(errors);
    }
}
