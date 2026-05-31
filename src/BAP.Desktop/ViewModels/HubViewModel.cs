using System.Collections.ObjectModel;
using BAP.Core.Enums;
using BAP.Core.Interfaces;
using BAP.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BAP.Desktop.ViewModels;

public partial class HubViewModel : ObservableObject
{
    private readonly HubModel _hub;
    private IHubConnection? _connection;

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private bool _isConnected;
    [ObservableProperty] private double _batteryLevel;
    [ObservableProperty] private double _batteryVoltage;
    [ObservableProperty] private SensorColor _ledColor;

    public HubType Type => _hub.Type;
    public string TypeName => _hub.Type.ToString();
    public ObservableCollection<PortViewModel> Ports { get; } = [];

    public HubViewModel(HubModel hub)
    {
        _hub = hub;
        _name = hub.Name;
        _ledColor = hub.LedColor;
        _isConnected = hub.IsConnected;
        _batteryLevel = hub.BatteryLevel;

        foreach (var port in hub.RegisteredPorts)
            Ports.Add(new PortViewModel(port, this));
    }

    public HubModel Model => _hub;

    public void AttachConnection(IHubConnection connection)
    {
        _connection = connection;
        _connection.BatteryUpdated += (_, e) =>
        {
            BatteryLevel = e.BatteryLevel;
            BatteryVoltage = e.BatteryVoltage;
        };
        _connection.Disconnected += (_, _) => IsConnected = false;
        _connection.PortUpdated += (_, e) =>
        {
            var port = Ports.FirstOrDefault(p => p.Id == e.PortId);
            port?.UpdateFromModel();
        };
        _connection.ColorTriggered += (_, e) =>
        {
            var port = Ports.FirstOrDefault(p => p.Id == e.PortId);
            port?.UpdateFromModel();
        };
        _connection.DistanceTriggered += (_, e) =>
        {
            var port = Ports.FirstOrDefault(p => p.Id == e.PortId);
            port?.UpdateFromModel();
        };
    }

    partial void OnNameChanged(string value)
    {
        _hub.Name = value;
    }

    partial void OnLedColorChanged(SensorColor value)
    {
        _hub.LedColor = value;
        if (_connection != null)
            _ = _connection.SetLedColorAsync(value);
    }

    [RelayCommand]
    private async Task Connect()
    {
        if (_connection != null)
        {
            await _connection.ConnectAsync();
            IsConnected = true;
        }
    }

    [RelayCommand]
    private async Task Disconnect()
    {
        if (_connection != null)
        {
            await _connection.DisconnectAsync();
            IsConnected = false;
        }
    }

    [RelayCommand]
    private async Task SetMotorSpeed((string portId, int speed) args)
    {
        if (_connection != null)
            await _connection.SetMotorSpeedAsync(args.portId, args.speed);
    }

    [RelayCommand]
    private async Task ActivateSwitch((string portId, bool left) args)
    {
        if (_connection != null)
        {
            int speed = args.left ? -100 : 100;
            await _connection.SetMotorSpeedAsync(args.portId, speed);
            await Task.Delay(500);
            await _connection.StopMotorAsync(args.portId);
        }
    }

    [RelayCommand]
    private void StopAll()
    {
        foreach (var port in Ports)
            if (port.IsMotor)
                port.StopCommand.Execute(null);
    }

    public void UpdateFromModel()
    {
        IsConnected = _hub.IsConnected;
        BatteryLevel = _hub.BatteryLevel;
        BatteryVoltage = _hub.BatteryVoltage;
        foreach (var port in Ports)
            port.UpdateFromModel();
    }
}
