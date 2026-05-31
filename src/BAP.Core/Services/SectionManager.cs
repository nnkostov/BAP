using BAP.Core.Enums;
using BAP.Core.Interfaces;
using BAP.Core.Models;
using Microsoft.Extensions.Logging;

namespace BAP.Core.Services;

/// <summary>
/// Manages the self-driving section network. Tracks train positions,
/// handles section reservation/release, switch activation, and speed management.
/// Ported from legacy LTP.Core/Sections.cs.
/// </summary>
public class SectionManager
{
    private readonly ILogger<SectionManager> _logger;
    private readonly IHubConnectionFactory _hubConnectionFactory;
    private SectionsModel _sections = new();
    private TrainProjectModel? _project;
    private readonly Dictionary<string, IHubConnection> _connections = new();
    private bool _isRunning;
    private CancellationTokenSource? _cts;

    public bool IsRunning => _isRunning;

    public event Action? DataUpdated;

    public SectionManager(
        ILogger<SectionManager> logger,
        IHubConnectionFactory hubConnectionFactory)
    {
        _logger = logger;
        _hubConnectionFactory = hubConnectionFactory;
    }

    /// <summary>
    /// Start the self-driving anti-collision system.
    /// Sets up a distance-triggered event for each section with a detector.
    /// </summary>
    public void Start(TrainProjectModel project)
    {
        _project = project;
        _sections = project.Sections;
        _cts = new CancellationTokenSource();
        _isRunning = true;

        _logger.LogInformation("Anti-collision active over {Count} sections", _sections.Sections.Count);

        // Initialize each section
        foreach (var section in _sections.Sections)
        {
            section.IsBeingCleared = false;
            section.ReservedBy = null;
        }
    }

    /// <summary>
    /// Stop the self-driving system.
    /// </summary>
    public void Stop()
    {
        _isRunning = false;
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        _logger.LogInformation("Anti-collision stopped");
    }

    /// <summary>
    /// Handle a sensor trigger on a section (called when a train reaches a detector).
    /// </summary>
    public async Task HandleSensorTriggerAsync(SectionModel section)
    {
        if (!_isRunning || _project == null) return;

        var currentTrain = section.CurrentHub;

        if (currentTrain == null)
        {
            _logger.LogWarning("Event discarded - Unknown train on {Section}", section.Name);
            return;
        }

        if (!currentTrain.IsPathProgramRunning)
        {
            _logger.LogDebug("{Train} - Event discarded - Train is not running", currentTrain.Name);
            return;
        }

        if (currentTrain.IsWaitingSection)
        {
            _logger.LogDebug("{Train} - Event discarded - Waiting for next section", currentTrain.Name);
            return;
        }

        if (section.IsBeingCleared)
        {
            _logger.LogDebug("{Section} - Event discarded - Leaving the section", section.Name);
            return;
        }

        await ReserveNextSectionAsync(section, currentTrain);
    }

    /// <summary>
    /// Reserve the next section for a train, waiting if occupied.
    /// </summary>
    public async Task ReserveNextSectionAsync(SectionModel currentSection, HubModel hub)
    {
        if (hub.CurrentPath == null) return;

        int nextSectionIdx = GetNextSectionIndex(hub);
        if (nextSectionIdx < 0 || nextSectionIdx >= _sections.Sections.Count)
        {
            _logger.LogWarning("{Train} - End of path reached, stopping", hub.Name);
            hub.IsPathProgramRunning = false;
            return;
        }

        var nextSection = _sections.Sections[nextSectionIdx];

        // Verify the section is connected forward
        if (!IsConnectedForward(currentSection, nextSection))
        {
            _logger.LogWarning("{Train} - Section {Next} is not a valid section forward", hub.Name, nextSection.Name);
            return;
        }

        // Wait for section to be free
        while (_isRunning && !hub.AbortReserve)
        {
            if (nextSection.CurrentHub == null &&
                (nextSection.ReservedBy == null || nextSection.ReservedBy == hub))
                break;

            hub.IsWaitingSection = true;
            _logger.LogInformation("{Train} - stopped because {Section} is occupied", hub.Name, nextSection.Name);

            try
            {
                await Task.Delay(1000, _cts?.Token ?? CancellationToken.None);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }

        if (!_isRunning || hub.AbortReserve)
        {
            hub.AbortReserve = false;
            hub.IsWaitingSection = false;
            return;
        }

        // Clear current section but keep reservation
        currentSection.IsBeingCleared = true;
        currentSection.ReservedBy = hub;
        currentSection.CurrentHub = null;

        // Step into the next section
        nextSection.CurrentHub = hub;
        MoveToNextSectionIndex(hub);

        // Check for two-section-ahead requirement
        if (nextSection.NeedsTwoSectionsReleased)
        {
            int nextNextIdx = GetNextSectionIndex(hub);
            if (nextNextIdx >= 0 && nextNextIdx < _sections.Sections.Count)
            {
                var nextNextSection = _sections.Sections[nextNextIdx];
                while (_isRunning && !hub.AbortReserve)
                {
                    if (nextNextSection.CurrentHub == null && nextNextSection.ReservedBy == null)
                        break;

                    _logger.LogInformation("{Train} - waiting for {Section} (two-section clearance)",
                        hub.Name, nextNextSection.Name);

                    try
                    {
                        await Task.Delay(1000, _cts?.Token ?? CancellationToken.None);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                }

                nextNextSection.ReservedBy = hub;
            }
        }

        // Resume train at section max speed
        int resumingSpeed = nextSection.MaxSpeed;
        _logger.LogInformation("{Train} - Allowed to move to {Section}", hub.Name, nextSection.Name);

        hub.IsWaitingSection = false;
        DataUpdated?.Invoke();

        // Wait for train to clear the section
        int waitSteps = 10;
        for (int i = 0; i < waitSteps && _isRunning && !hub.AbortReserve; i++)
        {
            try
            {
                await Task.Delay(hub.ClearingTimeInMs / waitSteps, _cts?.Token ?? CancellationToken.None);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        // Release the old section
        currentSection.ReservedBy = null;
        currentSection.IsBeingCleared = false;

        _logger.LogInformation("{Train} has cleared section {Section}", hub.Name, currentSection.Name);
        DataUpdated?.Invoke();
    }

    /// <summary>
    /// Clear a hub from all sections.
    /// </summary>
    public void ClearHub(HubModel hub)
    {
        foreach (var section in _sections.Sections)
            if (section.CurrentHub == hub)
                section.CurrentHub = null;
    }

    /// <summary>
    /// Release a section by name.
    /// </summary>
    public void Release(string sectionName)
    {
        var section = _sections.FindSection(sectionName);
        if (section != null)
            section.CurrentHub = null;
    }

    /// <summary>
    /// Check if a train is anywhere in the section network.
    /// </summary>
    public bool IsTrainInNetwork(HubModel hub)
    {
        return _sections.Sections.Any(s => s.CurrentHub == hub);
    }

    private static int GetNextSectionIndex(HubModel hub)
    {
        if (hub.CurrentPath == null || hub.CurrentPath.Sections.Length == 0)
            return -1;

        if (hub.CurrentPathPositionIndex >= hub.CurrentPath.Sections.Length)
            return hub.LoopCurrentPath ? hub.CurrentPath.Sections[0] : -1;

        return hub.CurrentPath.Sections[hub.CurrentPathPositionIndex];
    }

    private static void MoveToNextSectionIndex(HubModel hub)
    {
        hub.CurrentPathPositionIndex++;
        if (hub.LoopCurrentPath && hub.CurrentPath != null &&
            hub.CurrentPathPositionIndex >= hub.CurrentPath.Sections.Length)
        {
            hub.CurrentPathPositionIndex = 0;
        }
    }

    private static bool IsConnectedForward(SectionModel current, SectionModel next)
    {
        return current.LeftSectionName == next.Name ||
               current.RightSectionName == next.Name;
    }
}
