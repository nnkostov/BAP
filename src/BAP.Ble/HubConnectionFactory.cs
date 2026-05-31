using BAP.Core.Enums;
using BAP.Core.Interfaces;
using BAP.Core.Models;
using BAP.Ble.Protocol;
using Microsoft.Extensions.Logging;

namespace BAP.Ble;

/// <summary>
/// Creates the appropriate IHubConnection based on the hub's type.
/// </summary>
public class HubConnectionFactory : IHubConnectionFactory
{
    private readonly IBleAdapter _bleAdapter;
    private readonly ILoggerFactory _loggerFactory;

    public HubConnectionFactory(IBleAdapter bleAdapter, ILoggerFactory loggerFactory)
    {
        _bleAdapter = bleAdapter;
        _loggerFactory = loggerFactory;
    }

    public IHubConnection Create(HubModel hub) => hub.Type switch
    {
        HubType.PoweredUpHub or HubType.BoostMoveHub or HubType.PoweredUpRemote =>
            new Lpf2HubConnection(hub, _bleAdapter, _loggerFactory.CreateLogger<Lpf2HubConnection>()),

        HubType.SBrick =>
            new SBrickHubConnection(hub, _bleAdapter, _loggerFactory.CreateLogger<SBrickHubConnection>()),

        HubType.WeDo2SmartHub =>
            new WedoHubConnection(hub, _bleAdapter, _loggerFactory.CreateLogger<WedoHubConnection>()),

        HubType.BuWizz =>
            new BuWizzHubConnection(hub, _bleAdapter, _loggerFactory.CreateLogger<BuWizzHubConnection>()),

        HubType.PFx =>
            new PFxHubConnection(hub, _bleAdapter, _loggerFactory.CreateLogger<PFxHubConnection>()),

        HubType.EV3 => throw new NotSupportedException(
            "EV3 uses serial/Bluetooth SPP, not BLE. Use a serial-based IHubConnection implementation."),

        _ => throw new ArgumentOutOfRangeException(nameof(hub), hub.Type, "Unsupported hub type")
    };
}
