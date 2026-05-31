using System.Collections.ObjectModel;
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

    public ObservableCollection<TrackNodeViewModel> Nodes { get; } = [];
    public ObservableCollection<TrackConnectionViewModel> Connections { get; } = [];
    public ObservableCollection<TrackPathViewModel> Paths { get; } = [];

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
