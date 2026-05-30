using System.Collections.ObjectModel;
using BAP.Core.Interfaces;
using BAP.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BAP.Desktop.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IProjectRepository _projectRepository;
    private readonly IHubConnectionFactory _hubConnectionFactory;
    private readonly ConsoleViewModel _console;

    private TrainProjectModel _project = new();

    [ObservableProperty] private string _title = "Brick Automation Project";
    [ObservableProperty] private bool _isScanning;
    [ObservableProperty] private int _selectedTabIndex;

    public ObservableCollection<HubViewModel> Hubs { get; } = [];
    public ObservableCollection<ProgramViewModel> Programs { get; } = [];
    public ConsoleViewModel Console => _console;

    public MainWindowViewModel(
        IProjectRepository projectRepository,
        IHubConnectionFactory hubConnectionFactory,
        ConsoleViewModel console)
    {
        _projectRepository = projectRepository;
        _hubConnectionFactory = hubConnectionFactory;
        _console = console;

        _console.WriteLine("Brick Automation Project initialized", ConsoleEntryLevel.Success);
        _console.WriteLine("Ready to scan for LEGO hubs...");
    }

    [RelayCommand]
    private Task NewProject()
    {
        _project = new TrainProjectModel();
        Hubs.Clear();
        Programs.Clear();
        _console.WriteLine("New project created");
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task SaveProject()
    {
        try
        {
            SyncViewModelsToModel();
            await _projectRepository.SaveAsync(_project, "project.bap");
            _console.WriteLine("Project saved", ConsoleEntryLevel.Success);
        }
        catch (Exception ex)
        {
            _console.WriteLine($"Save failed: {ex.Message}", ConsoleEntryLevel.Error);
        }
    }

    [RelayCommand]
    private async Task LoadProject()
    {
        try
        {
            _project = await _projectRepository.LoadAsync("project.bap") ?? new TrainProjectModel();
            RefreshFromModel();
            _console.WriteLine("Project loaded", ConsoleEntryLevel.Success);
        }
        catch (Exception ex)
        {
            _console.WriteLine($"Load failed: {ex.Message}", ConsoleEntryLevel.Error);
        }
    }

    [RelayCommand]
    private void AddProgram()
    {
        var program = new TrainProgramModel { Name = $"Program {Programs.Count + 1}" };
        _project.Programs.Add(program);
        Programs.Add(new ProgramViewModel(program));
        _console.WriteLine($"Added program: {program.Name}");
    }

    [RelayCommand]
    private void RemoveProgram(ProgramViewModel? programVm)
    {
        if (programVm == null) return;
        _project.Programs.RemoveAll(p => p.Name == programVm.Name);
        Programs.Remove(programVm);
    }

    private void SyncViewModelsToModel()
    {
        _project.RegisteredTrains.Clear();
        foreach (var hub in Hubs)
            _project.RegisteredTrains.Add(hub.Model);
    }

    private void RefreshFromModel()
    {
        Hubs.Clear();
        Programs.Clear();

        foreach (var hub in _project.RegisteredTrains)
            Hubs.Add(new HubViewModel(hub));

        foreach (var program in _project.Programs)
            Programs.Add(new ProgramViewModel(program));
    }
}
