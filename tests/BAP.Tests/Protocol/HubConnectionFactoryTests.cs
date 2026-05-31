using BAP.Core.Enums;
using BAP.Core.Interfaces;
using BAP.Core.Models;
using BAP.Ble;
using BAP.Ble.Protocol;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace BAP.Tests.Protocol;

public class HubConnectionFactoryTests
{
    private readonly HubConnectionFactory _factory;

    public HubConnectionFactoryTests()
    {
        var bleAdapter = new FakeBleAdapter();
        _factory = new HubConnectionFactory(bleAdapter, NullLoggerFactory.Instance);
    }

    [Theory]
    [InlineData(HubType.PoweredUpHub, typeof(Lpf2HubConnection))]
    [InlineData(HubType.BoostMoveHub, typeof(Lpf2HubConnection))]
    [InlineData(HubType.PoweredUpRemote, typeof(Lpf2HubConnection))]
    [InlineData(HubType.SBrick, typeof(SBrickHubConnection))]
    [InlineData(HubType.WeDo2SmartHub, typeof(WedoHubConnection))]
    [InlineData(HubType.BuWizz, typeof(BuWizzHubConnection))]
    [InlineData(HubType.PFx, typeof(PFxHubConnection))]
    public void Create_ReturnsCorrectType(HubType hubType, Type expectedType)
    {
        var hub = new HubModel { Name = "Test", Type = hubType };
        var connection = _factory.Create(hub);
        Assert.IsType(expectedType, connection);
    }

    [Fact]
    public void Create_EV3_ThrowsNotSupported()
    {
        var hub = new HubModel { Name = "EV3", Type = HubType.EV3 };
        Assert.Throws<NotSupportedException>(() => _factory.Create(hub));
    }

    private class FakeBleAdapter : IBleAdapter
    {
        public bool IsScanning => false;
#pragma warning disable CS0067
        public event EventHandler<BleDeviceDiscoveredEventArgs>? DeviceDiscovered;
#pragma warning restore CS0067
        public Task StartScanningAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task StopScanningAsync() => Task.CompletedTask;
        public Task<IBleDevice?> ConnectAsync(ulong address, CancellationToken ct = default) => Task.FromResult<IBleDevice?>(null);
    }
}
