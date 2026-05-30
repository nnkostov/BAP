using System.Collections.ObjectModel;
using BAP.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BAP.Desktop.ViewModels;

public partial class ProgramViewModel : ObservableObject
{
    private readonly TrainProgramModel _program;

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private bool _isRunning;

    public ObservableCollection<EventViewModel> Events { get; } = [];

    public ProgramViewModel(TrainProgramModel program)
    {
        _program = program;
        _name = program.Name;

        foreach (var evt in program.Events)
            Events.Add(new EventViewModel(evt));
    }

    partial void OnNameChanged(string value) => _program.Name = value;

    [RelayCommand]
    private void ToggleRunning()
    {
        IsRunning = !IsRunning;
    }

    [RelayCommand]
    private void AddEvent()
    {
        var evt = new ProgramEventModel();
        _program.Events.Add(evt);
        Events.Add(new EventViewModel(evt));
    }

    [RelayCommand]
    private void RemoveEvent(EventViewModel? eventVm)
    {
        if (eventVm == null) return;
        _program.Events.Remove(eventVm.Model);
        Events.Remove(eventVm);
    }
}

public partial class EventViewModel : ObservableObject
{
    internal readonly ProgramEventModel Model;

    [ObservableProperty] private string _triggerDescription = string.Empty;
    [ObservableProperty] private string _actionDescription = string.Empty;

    public EventViewModel(ProgramEventModel model)
    {
        Model = model;
        _triggerDescription = $"{model.Trigger}: {model.TrainDeviceId} / {model.TrainPort}";
        _actionDescription = $"{model.Action}: {model.TargetDeviceId} / {model.TargetPort}";
    }
}
