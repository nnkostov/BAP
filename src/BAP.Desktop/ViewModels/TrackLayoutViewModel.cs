using System.Collections.ObjectModel;
using Avalonia.Threading;
using BAP.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BAP.Desktop.ViewModels;

public partial class TrackNodeViewModel : ObservableObject
{
    private readonly SectionModel _model;

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private double _x;
    [ObservableProperty] private double _y;
    [ObservableProperty] private string _statusLabel = "Free";
    [ObservableProperty] private bool _isOccupied;
    [ObservableProperty] private bool _isReserved;
    [ObservableProperty] private bool _isSelected;

    public string? LeftSectionName => _model.LeftSectionName;
    public string? RightSectionName => _model.RightSectionName;
    public bool HasSwitch => _model.RightSectionName != null;
    public bool HasDetector => _model.Detector != null;
    public int MaxSpeed => _model.MaxSpeed;
    public SectionModel Model => _model;

    public string StatusColor => IsOccupied ? "#EF4444" : IsReserved ? "#FBBF24" : "#10B981";
    public string BorderColor => IsSelected ? "#5B8DEF" : StatusColor;

    public TrackNodeViewModel(SectionModel model, double x, double y)
    {
        _model = model;
        _name = model.Name;
        _x = x;
        _y = y;
    }

    public void RefreshState()
    {
        if (_model.CurrentHub != null)
        {
            IsOccupied = true;
            IsReserved = false;
            StatusLabel = _model.CurrentHub.Name;
        }
        else if (_model.ReservedBy != null)
        {
            IsOccupied = false;
            IsReserved = true;
            StatusLabel = "Reserved";
        }
        else
        {
            IsOccupied = false;
            IsReserved = false;
            StatusLabel = "Free";
        }

        OnPropertyChanged(nameof(StatusColor));
        OnPropertyChanged(nameof(BorderColor));
    }

    partial void OnIsSelectedChanged(bool value)
    {
        OnPropertyChanged(nameof(BorderColor));
    }

    public void UpdateName(string newName)
    {
        // Update references in other sections
        Name = newName;
        _model.Name = newName;
    }

    public void UpdateMaxSpeed(int speed)
    {
        _model.MaxSpeed = Math.Clamp(speed, 0, 100);
        OnPropertyChanged(nameof(MaxSpeed));
    }

    public void SetLeftSection(string? name)
    {
        _model.LeftSectionName = name;
        OnPropertyChanged(nameof(LeftSectionName));
    }

    public void SetRightSection(string? name)
    {
        _model.RightSectionName = name;
        OnPropertyChanged(nameof(RightSectionName));
        OnPropertyChanged(nameof(HasSwitch));
    }
}

public record TrackConnectionViewModel(
    double X1, double Y1,
    double X2, double Y2,
    bool IsBranch);

public partial class TrainMarkerViewModel : ObservableObject
{
    private static readonly string[] TrainColors = ["#F59E0B", "#3B82F6", "#EF4444", "#8B5CF6", "#EC4899", "#06B6D4"];
    private static int _colorIndex;

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _color;
    [ObservableProperty] private double _x;
    [ObservableProperty] private double _y;
    [ObservableProperty] private string _currentSectionName = string.Empty;
    [ObservableProperty] private int _speed;
    [ObservableProperty] private double _progress; // 0..1 within current section
    [ObservableProperty] private bool _isWaiting;

    public int PathIndex { get; set; }
    public int PathSectionIndex { get; set; }

    public TrainMarkerViewModel(string name)
    {
        _name = name;
        _color = TrainColors[_colorIndex++ % TrainColors.Length];
    }
}

public partial class TrackPathViewModel : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private int _sectionCount;
    [ObservableProperty] private bool _isLoop;
    [ObservableProperty] private string _color = "#5B8DEF";
}

public partial class TrackLayoutViewModel : ObservableObject
{
    private readonly SectionsModel _sections;
    private readonly ConsoleViewModel _console;

    private static readonly string[] PathColors = ["#5B8DEF", "#22D3EE", "#10B981", "#FBBF24", "#EF4444", "#A78BFA"];

    [ObservableProperty] private TrackNodeViewModel? _selectedNode;
    [ObservableProperty] private string _editName = string.Empty;
    [ObservableProperty] private int _editMaxSpeed;
    [ObservableProperty] private string? _editLeftSection;
    [ObservableProperty] private string? _editRightSection;
    [ObservableProperty] private bool _isSimulationRunning;
    [ObservableProperty] private int _simulationSpeed = 50;

    public string SimulationButtonText => IsSimulationRunning ? "⏹ Stop" : "▶ Start";

    partial void OnIsSimulationRunningChanged(bool value)
    {
        OnPropertyChanged(nameof(SimulationButtonText));
    }

    private DispatcherTimer? _simTimer;
    private int _trainCounter;

    public ObservableCollection<TrackNodeViewModel> Nodes { get; } = [];
    public ObservableCollection<TrackConnectionViewModel> Connections { get; } = [];
    public ObservableCollection<TrackPathViewModel> Paths { get; } = [];
    public ObservableCollection<TrainMarkerViewModel> Trains { get; } = [];

    public TrackLayoutViewModel(SectionsModel sections, ConsoleViewModel console)
    {
        _sections = sections;
        _console = console;
    }

    public void SelectNode(TrackNodeViewModel? node)
    {
        if (SelectedNode != null)
            SelectedNode.IsSelected = false;

        SelectedNode = node;

        if (node != null)
        {
            node.IsSelected = true;
            EditName = node.Name;
            EditMaxSpeed = node.MaxSpeed;
            EditLeftSection = node.LeftSectionName;
            EditRightSection = node.RightSectionName;
        }
    }

    [RelayCommand]
    public void DeselectAll()
    {
        SelectNode(null);
    }

    [RelayCommand]
    private void ApplyEdits()
    {
        if (SelectedNode == null) return;

        var oldName = SelectedNode.Name;
        if (EditName != oldName && !string.IsNullOrWhiteSpace(EditName))
        {
            // Update references in all other sections
            foreach (var s in _sections.Sections)
            {
                if (s.LeftSectionName == oldName)
                    s.LeftSectionName = EditName;
                if (s.RightSectionName == oldName)
                    s.RightSectionName = EditName;
            }
            SelectedNode.UpdateName(EditName);
        }

        SelectedNode.UpdateMaxSpeed(EditMaxSpeed);
        SelectedNode.SetLeftSection(string.IsNullOrWhiteSpace(EditLeftSection) ? null : EditLeftSection);
        SelectedNode.SetRightSection(string.IsNullOrWhiteSpace(EditRightSection) ? null : EditRightSection);

        RebuildConnections();
        _console.WriteLine($"Updated section: {SelectedNode.Name}");
    }

    [RelayCommand]
    private void DeleteSelectedSection()
    {
        if (SelectedNode == null) return;

        var name = SelectedNode.Name;
        var model = SelectedNode.Model;
        _sections.Sections.Remove(model);

        foreach (var s in _sections.Sections)
        {
            if (s.LeftSectionName == name) s.LeftSectionName = null;
            if (s.RightSectionName == name) s.RightSectionName = null;
        }

        SelectNode(null);
        RebuildLayout();
        _console.WriteLine($"Deleted section: {name}");
    }

    public void MoveNode(TrackNodeViewModel node, double newX, double newY)
    {
        node.X = Math.Max(0, newX);
        node.Y = Math.Max(0, newY);
        RebuildConnections();
    }

    private void RebuildConnections()
    {
        Connections.Clear();
        const double nodeW = 136;
        const double nodeH = 52;
        foreach (var node in Nodes)
        {
            if (node.LeftSectionName != null)
            {
                var target = FindNode(node.LeftSectionName);
                if (target != null)
                    Connections.Add(new TrackConnectionViewModel(
                        node.X + nodeW, node.Y + nodeH / 2,
                        target.X, target.Y + nodeH / 2, false));
            }
            if (node.RightSectionName != null)
            {
                var target = FindNode(node.RightSectionName);
                if (target != null)
                    Connections.Add(new TrackConnectionViewModel(
                        node.X + nodeW, node.Y + nodeH / 2,
                        target.X, target.Y + nodeH / 2, true));
            }
        }
    }

    [RelayCommand]
    private void AddSection()
    {
        var name = $"Section {_sections.Sections.Count + 1}";
        var section = new SectionModel { Name = name };
        _sections.Sections.Add(section);

        // Connect to previous section if it exists
        if (_sections.Sections.Count > 1)
        {
            var prev = _sections.Sections[^2];
            prev.LeftSectionName = name;
        }

        RebuildLayout();
        _console.WriteLine($"Added section: {name}");
    }

    [RelayCommand]
    private void RemoveLastSection()
    {
        if (_sections.Sections.Count == 0) return;

        var removed = _sections.Sections[^1];
        _sections.Sections.RemoveAt(_sections.Sections.Count - 1);

        // Clean up references
        foreach (var s in _sections.Sections)
        {
            if (s.LeftSectionName == removed.Name)
                s.LeftSectionName = null;
            if (s.RightSectionName == removed.Name)
                s.RightSectionName = null;
        }

        RebuildLayout();
        _console.WriteLine($"Removed section: {removed.Name}");
    }

    // ── Simulation ──

    [RelayCommand]
    private void ToggleSimulation()
    {
        if (IsSimulationRunning)
            StopSimulation();
        else
            StartSimulation();
    }

    private void StartSimulation()
    {
        if (Nodes.Count == 0 || _sections.Paths.Count == 0) return;

        IsSimulationRunning = true;
        _simTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        _simTimer.Tick += SimulationTick;
        _simTimer.Start();
        _console.WriteLine("Simulation started", ConsoleEntryLevel.Success);
    }

    private void StopSimulation()
    {
        IsSimulationRunning = false;
        _simTimer?.Stop();
        _simTimer = null;

        // Clear all section states
        foreach (var node in Nodes)
        {
            node.Model.CurrentHub = null;
            node.Model.ReservedBy = null;
            node.RefreshState();
        }
        _console.WriteLine("Simulation stopped");
    }

    [RelayCommand]
    private void AddTrain()
    {
        if (Nodes.Count == 0 || _sections.Paths.Count == 0) return;

        _trainCounter++;
        var train = new TrainMarkerViewModel($"Train {_trainCounter}")
        {
            PathIndex = (_trainCounter - 1) % _sections.Paths.Count,
            PathSectionIndex = 0,
            Speed = SimulationSpeed
        };

        // Place on first section of its assigned path
        var path = _sections.Paths[train.PathIndex];
        if (path.Sections.Length == 0) return;

        var sectionIdx = path.Sections[0];
        if (sectionIdx < _sections.Sections.Count)
        {
            var section = _sections.Sections[sectionIdx];
            train.CurrentSectionName = section.Name;

            var node = FindNode(section.Name);
            if (node != null)
            {
                train.X = node.X + 68; // Center of node
                train.Y = node.Y + 26;
                section.CurrentHub = new HubModel { Name = train.Name };
                node.RefreshState();
            }
        }

        Trains.Add(train);
        _console.WriteLine($"Added {train.Name} on {train.CurrentSectionName} ({_sections.Paths[train.PathIndex].Name})");

        // Auto-start simulation if not running
        if (!IsSimulationRunning)
            StartSimulation();
    }

    [RelayCommand]
    private void RemoveAllTrains()
    {
        StopSimulation();
        Trains.Clear();
        _trainCounter = 0;
        _console.WriteLine("All trains removed");
    }

    private void SimulationTick(object? sender, EventArgs e)
    {
        double speedFactor = SimulationSpeed / 100.0;

        foreach (var train in Trains)
        {
            if (train.IsWaiting)
            {
                // Check if the next section is now free
                var nextSection = GetNextSectionForTrain(train);
                if (nextSection != null && nextSection.CurrentHub == null && nextSection.ReservedBy == null)
                {
                    train.IsWaiting = false;
                    // Reserve ahead
                    nextSection.ReservedBy = new HubModel { Name = train.Name };
                    var nextNode = FindNode(nextSection.Name);
                    nextNode?.RefreshState();
                }
                continue;
            }

            // Get current section's max speed to scale progress
            var currentNode = FindNode(train.CurrentSectionName);
            double sectionSpeedFactor = currentNode != null ? currentNode.MaxSpeed / 100.0 : 0.5;
            double progressDelta = 0.02 * speedFactor * sectionSpeedFactor;

            train.Progress += progressDelta;

            if (train.Progress >= 1.0)
            {
                // Move to next section
                MoveTrainToNextSection(train);
            }
            else
            {
                // Interpolate position between current and next section
                UpdateTrainPosition(train);
            }
        }
    }

    private void MoveTrainToNextSection(TrainMarkerViewModel train)
    {
        var path = _sections.Paths[train.PathIndex];

        // Release current section
        var currentSection = _sections.FindSection(train.CurrentSectionName);
        if (currentSection != null)
        {
            currentSection.CurrentHub = null;
            currentSection.ReservedBy = null;
            FindNode(currentSection.Name)?.RefreshState();
        }

        // Advance path index
        train.PathSectionIndex++;
        if (train.PathSectionIndex >= path.Sections.Length)
        {
            if (path.LoopPath)
                train.PathSectionIndex = 0;
            else
            {
                train.IsWaiting = true;
                train.Progress = 1.0;
                return;
            }
        }

        var nextSectionIdx = path.Sections[train.PathSectionIndex];
        if (nextSectionIdx >= _sections.Sections.Count) return;

        var nextSection = _sections.Sections[nextSectionIdx];

        // Check if section is occupied by another train
        if (nextSection.CurrentHub != null)
        {
            train.IsWaiting = true;
            train.Progress = 1.0;
            return;
        }

        // Occupy new section
        nextSection.CurrentHub = new HubModel { Name = train.Name };
        nextSection.ReservedBy = null;
        train.CurrentSectionName = nextSection.Name;
        train.Progress = 0;

        var node = FindNode(nextSection.Name);
        node?.RefreshState();

        // Reserve ahead
        var aheadSection = GetNextSectionForTrain(train);
        if (aheadSection != null && aheadSection.CurrentHub == null)
        {
            aheadSection.ReservedBy = new HubModel { Name = train.Name };
            FindNode(aheadSection.Name)?.RefreshState();
        }

        UpdateTrainPosition(train);
    }

    private SectionModel? GetNextSectionForTrain(TrainMarkerViewModel train)
    {
        var path = _sections.Paths[train.PathIndex];
        int nextIdx = train.PathSectionIndex + 1;
        if (nextIdx >= path.Sections.Length)
            nextIdx = path.LoopPath ? 0 : -1;

        if (nextIdx < 0 || nextIdx >= path.Sections.Length) return null;
        var sectionIdx = path.Sections[nextIdx];
        return sectionIdx < _sections.Sections.Count ? _sections.Sections[sectionIdx] : null;
    }

    private void UpdateTrainPosition(TrainMarkerViewModel train)
    {
        var currentNode = FindNode(train.CurrentSectionName);
        if (currentNode == null) return;

        // Find next section's node for interpolation
        var nextSection = GetNextSectionForTrain(train);
        var nextNode = nextSection != null ? FindNode(nextSection.Name) : null;

        const double nodeW = 136;
        const double nodeH = 52;

        double startX = currentNode.X + nodeW / 2;
        double startY = currentNode.Y + nodeH / 2;

        if (nextNode != null)
        {
            double endX = nextNode.X + nodeW / 2;
            double endY = nextNode.Y + nodeH / 2;

            // Smooth Bezier interpolation matching the connection curves
            double t = train.Progress;
            double dx = endX - startX;
            double dist = Math.Sqrt(dx * dx + (endY - startY) * (endY - startY));
            double curvature = Math.Min(dist * 0.4, 60);

            double cx1, cy1, cx2, cy2;
            if (dx >= 0)
            {
                cx1 = startX + curvature;
                cy1 = startY;
                cx2 = endX - curvature;
                cy2 = endY;
            }
            else
            {
                double archHeight = Math.Max(80, Math.Abs(endY - startY) + 40);
                cx1 = startX + 60;
                cy1 = startY + archHeight;
                cx2 = endX - 60;
                cy2 = endY + archHeight;
            }

            // Cubic Bezier: B(t) = (1-t)^3*P0 + 3*(1-t)^2*t*P1 + 3*(1-t)*t^2*P2 + t^3*P3
            double u = 1 - t;
            train.X = u * u * u * startX + 3 * u * u * t * cx1 + 3 * u * t * t * cx2 + t * t * t * endX;
            train.Y = u * u * u * startY + 3 * u * u * t * cy1 + 3 * u * t * t * cy2 + t * t * t * endY;
        }
        else
        {
            train.X = startX;
            train.Y = startY;
        }
    }

    [RelayCommand]
    private void LoadDemoTrack()
    {
        _sections.Sections.Clear();

        // Create a realistic oval track with a siding
        _sections.Sections.AddRange([
            new SectionModel { Name = "Station",    LeftSectionName = "North Curve",  MaxSpeed = 40 },
            new SectionModel { Name = "North Curve", LeftSectionName = "Switch A",    MaxSpeed = 60 },
            new SectionModel
            {
                Name = "Switch A",
                LeftSectionName = "Main Line",
                RightSectionName = "Siding",
                MaxSpeed = 50
            },
            new SectionModel { Name = "Main Line",  LeftSectionName = "South Curve",  MaxSpeed = 100 },
            new SectionModel { Name = "Siding",     LeftSectionName = "South Curve",  MaxSpeed = 30 },
            new SectionModel { Name = "South Curve", LeftSectionName = "Return",      MaxSpeed = 60 },
            new SectionModel { Name = "Return",     LeftSectionName = "Station",      MaxSpeed = 80 },
        ]);

        _sections.Paths.Clear();
        _sections.Paths.Add(new PathModel
        {
            Name = "Main Loop",
            Sections = [0, 1, 2, 3, 5, 6],
            LoopPath = true
        });
        _sections.Paths.Add(new PathModel
        {
            Name = "Via Siding",
            Sections = [0, 1, 2, 4, 5, 6],
            LoopPath = true
        });

        RebuildLayout();
        _console.WriteLine("Loaded demo track: 7 sections, 1 switch, 2 paths", ConsoleEntryLevel.Success);
    }

    public void RebuildLayout()
    {
        Nodes.Clear();
        Connections.Clear();
        Paths.Clear();

        if (_sections.Sections.Count == 0) return;

        // Auto-layout: arrange sections in a visual pattern
        LayoutSections();

        // Build connections from node positions (wider nodes now)
        const double nodeW = 136;
        const double nodeH = 52;
        foreach (var node in Nodes)
        {
            if (node.LeftSectionName != null)
            {
                var target = FindNode(node.LeftSectionName);
                if (target != null)
                {
                    Connections.Add(new TrackConnectionViewModel(
                        node.X + nodeW, node.Y + nodeH / 2,
                        target.X, target.Y + nodeH / 2,
                        false));
                }
            }

            if (node.RightSectionName != null)
            {
                var target = FindNode(node.RightSectionName);
                if (target != null)
                {
                    Connections.Add(new TrackConnectionViewModel(
                        node.X + nodeW, node.Y + nodeH / 2,
                        target.X, target.Y + nodeH / 2,
                        true));
                }
            }
        }

        // Build path view models
        for (int i = 0; i < _sections.Paths.Count; i++)
        {
            var p = _sections.Paths[i];
            Paths.Add(new TrackPathViewModel
            {
                Name = p.Name,
                SectionCount = p.Sections.Length,
                IsLoop = p.LoopPath,
                Color = PathColors[i % PathColors.Length]
            });
        }
    }

    private void LayoutSections()
    {
        var placed = new HashSet<string>();
        const double spacingX = 175;
        const double spacingY = 85;
        const int maxCols = 5;

        // Find starting section (one not referenced as Left/Right by any other)
        var referenced = new HashSet<string>();
        foreach (var s in _sections.Sections)
        {
            if (s.LeftSectionName != null) referenced.Add(s.LeftSectionName);
            if (s.RightSectionName != null) referenced.Add(s.RightSectionName);
        }

        var startSection = _sections.Sections.FirstOrDefault(s => !referenced.Contains(s.Name))
                           ?? _sections.Sections[0];

        // BFS layout with column wrapping
        var queue = new Queue<(SectionModel section, int column, int row)>();
        queue.Enqueue((startSection, 0, 0));
        int maxRowUsed = 0;

        while (queue.Count > 0)
        {
            var (section, c, r) = queue.Dequeue();
            if (placed.Contains(section.Name)) continue;
            placed.Add(section.Name);

            // Wrap to next row if exceeding max columns
            int actualCol = c % maxCols;
            int wrapRow = r + (c / maxCols) * 2;

            var node = new TrackNodeViewModel(section, 30 + actualCol * spacingX, 30 + wrapRow * spacingY);
            node.RefreshState();
            Nodes.Add(node);
            if (wrapRow > maxRowUsed) maxRowUsed = wrapRow;

            if (section.LeftSectionName != null)
            {
                var next = _sections.FindSection(section.LeftSectionName);
                if (next != null && !placed.Contains(next.Name))
                    queue.Enqueue((next, c + 1, r));
            }

            if (section.RightSectionName != null)
            {
                var next = _sections.FindSection(section.RightSectionName);
                if (next != null && !placed.Contains(next.Name))
                    queue.Enqueue((next, c + 1, r + 1));
            }
        }

        // Place any disconnected sections
        double disCol = 0;
        foreach (var section in _sections.Sections)
        {
            if (placed.Contains(section.Name)) continue;
            placed.Add(section.Name);
            Nodes.Add(new TrackNodeViewModel(section,
                30 + disCol * spacingX, 30 + (maxRowUsed + 2) * spacingY));
            disCol++;
        }
    }

    private TrackNodeViewModel? FindNode(string name)
        => Nodes.FirstOrDefault(n => n.Name == name);
}
