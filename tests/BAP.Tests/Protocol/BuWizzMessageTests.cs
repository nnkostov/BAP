using BAP.Core.Enums;
using BAP.Core.Models;
using BAP.Ble.Protocol;
using Microsoft.Extensions.Logging.Abstractions;

namespace BAP.Tests.Protocol;

public class BuWizzMessageTests
{
    [Fact]
    public void BuildMotorPacket_AllZeros()
    {
        var hub = CreateDefaultBuWizzHub();
        var conn = new BuWizzHubConnection(hub, null!, NullLogger<BuWizzHubConnection>.Instance);

        var packet = conn.BuildMotorPacket();

        Assert.Equal(0x10, packet[0]);
        Assert.Equal(0, packet[1]); // port A speed
        Assert.Equal(0, packet[2]); // port B speed
        Assert.Equal(0, packet[3]); // port C speed
        Assert.Equal(0, packet[4]); // port D speed
        Assert.Equal(0, packet[5]); // trailing zero
    }

    [Fact]
    public void BuildMotorPacket_ReflectsPortSpeeds()
    {
        var hub = CreateDefaultBuWizzHub();
        hub.RegisteredPorts[0].Speed = 50;
        hub.RegisteredPorts[1].Speed = -30;
        hub.RegisteredPorts[2].Speed = 100;
        hub.RegisteredPorts[3].Speed = 0;

        var conn = new BuWizzHubConnection(hub, null!, NullLogger<BuWizzHubConnection>.Instance);
        var packet = conn.BuildMotorPacket();

        Assert.Equal(50, (sbyte)packet[1]);
        Assert.Equal(-30, (sbyte)packet[2]);
        Assert.Equal(100, (sbyte)packet[3]);
        Assert.Equal(0, packet[4]);
    }

    private static HubModel CreateDefaultBuWizzHub() => new()
    {
        Name = "BuWizz Test",
        Type = HubType.BuWizz,
        RegisteredPorts =
        [
            new PortModel { Id = "A", Value = 0, Function = PortFunction.Motor },
            new PortModel { Id = "B", Value = 2, Function = PortFunction.Motor },
            new PortModel { Id = "C", Value = 1, Function = PortFunction.Motor },
            new PortModel { Id = "D", Value = 3, Function = PortFunction.Motor }
        ]
    };
}
