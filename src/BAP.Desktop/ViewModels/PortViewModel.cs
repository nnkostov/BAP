using BAP.Core.Enums;
using BAP.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BAP.Desktop.ViewModels;

public partial class PortViewModel : ObservableObject
{
    private readonly PortModel _port;
    private readonly HubViewModel _parent;

    [ObservableProperty] private int _speed;
    [ObservableProperty] private string _colorLabel = "None";
    [ObservableProperty] private int _distance;
    [ObservableProperty] private bool _isConnected;
    [ObservableProperty] private PortFunction _function;

    public string Id => _port.Id;
    public int Value => _port.Value;
    public PortDevice Device => _port.Device;

    public bool IsMotor => Function is PortFunction.Motor or PortFunction.TrainMotor;
    public bool IsSwitch => Function is PortFunction.SwitchStandard or PortFunction.SwitchDoublecross or PortFunction.SwitchTrixBrix;
    public bool IsSensor => Function == PortFunction.Sensor;
    public bool IsLight => Function == PortFunction.Light;
    public bool HasUI => IsMotor || IsSwitch || IsSensor || IsLight;

    public PortViewModel(PortModel port, HubViewModel parent)
    {
        _port = port;
        _parent = parent;
        _speed = port.Speed;
        _function = port.Function;
        _isConnected = port.Connected;
    }

    partial void OnSpeedChanged(int value)
    {
        _port.Speed = value;
        _parent.SetMotorSpeedCommand.Execute((Id, value));
    }

    partial void OnFunctionChanged(PortFunction value)
    {
        _port.Function = value;
        OnPropertyChanged(nameof(IsMotor));
        OnPropertyChanged(nameof(IsSwitch));
        OnPropertyChanged(nameof(IsSensor));
        OnPropertyChanged(nameof(IsLight));
        OnPropertyChanged(nameof(HasUI));
    }

    public void UpdateFromModel()
    {
        Speed = _port.Speed;
        IsConnected = _port.Connected;
        Function = _port.Function;
        Distance = _port.LatestDistance;
        ColorLabel = _port.LatestColor.ToString();
    }

    [RelayCommand]
    private void Stop()
    {
        Speed = 0;
    }

    [RelayCommand]
    private void SwitchLeft()
    {
        _parent.ActivateSwitchCommand.Execute((Id, true));
    }

    [RelayCommand]
    private void SwitchRight()
    {
        _parent.ActivateSwitchCommand.Execute((Id, false));
    }
}
