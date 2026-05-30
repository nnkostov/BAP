using BAP.Core.Interfaces;
using BAP.Core.Models;

namespace BAP.Core.Services;

/// <summary>
/// Global variables exposed to user scripts.
/// Users can write: Hub[0].SetMotorSpeed("A", 75); Wait(1000); WriteLine("done");
/// </summary>
public class ScriptGlobals
{
    private readonly IReadOnlyList<IHubConnection> _connections;
    private readonly CancellationToken _ct;

    public ScriptGlobals(
        IReadOnlyList<IHubConnection> connections,
        IReadOnlyList<HubModel> hubs,
        SectionsModel sections,
        Action<string> writeLine,
        CancellationToken ct)
    {
        _connections = connections;
        _ct = ct;
        Hub = hubs;
        Sections = sections;
        WriteLineAction = writeLine;
        Global = new int[100];
    }

    /// <summary>
    /// All connected hubs (indexed: Hub[0], Hub[1], ...).
    /// </summary>
    public IReadOnlyList<HubModel> Hub { get; }

    /// <summary>
    /// Track sections for self-driving.
    /// </summary>
    public SectionsModel Sections { get; }

    /// <summary>
    /// Shared global state array (100 slots).
    /// </summary>
    public int[] Global { get; set; }

    internal Action<string> WriteLineAction { get; }

    /// <summary>
    /// Write a message to the debug console.
    /// </summary>
    public void WriteLine(string text) => WriteLineAction(text);

    /// <summary>
    /// Wait for the specified number of milliseconds.
    /// </summary>
    public async Task Wait(int milliseconds)
    {
        await Task.Delay(milliseconds, _ct);
    }

    /// <summary>
    /// Set motor speed on a hub by index.
    /// </summary>
    public async Task SetMotorSpeed(int hubIndex, string port, int speed)
    {
        if (hubIndex >= 0 && hubIndex < _connections.Count)
            await _connections[hubIndex].SetMotorSpeedAsync(port, speed);
    }

    /// <summary>
    /// Stop a motor on a hub by index.
    /// </summary>
    public async Task Stop(int hubIndex, string port)
    {
        if (hubIndex >= 0 && hubIndex < _connections.Count)
            await _connections[hubIndex].StopMotorAsync(port);
    }
}
