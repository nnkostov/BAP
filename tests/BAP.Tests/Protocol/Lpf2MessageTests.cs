using BAP.Core.Enums;
using BAP.Ble.Protocol;

namespace BAP.Tests.Protocol;

public class Lpf2MessageTests
{
    [Fact]
    public void BuildMotorMessage_PoweredUpHub_SinglePort()
    {
        var msg = Lpf2HubConnection.BuildMotorMessage(HubType.PoweredUpHub, 0x00, 50, isAbPort: false);

        Assert.Equal(0x81, msg[0]);
        Assert.Equal(0x00, msg[1]);
        Assert.Equal(0x11, msg[2]);
        Assert.Equal(0x51, msg[3]);
        Assert.Equal(0x00, msg[4]);
        Assert.Equal(50, (int)(sbyte)msg[5]);
    }

    [Fact]
    public void BuildMotorMessage_BoostMoveHub_SinglePort()
    {
        var msg = Lpf2HubConnection.BuildMotorMessage(HubType.BoostMoveHub, 0x00, 75, isAbPort: false);

        Assert.Equal(0x81, msg[0]);
        Assert.Equal(8, msg.Length);
        Assert.Equal(0x01, msg[3]); // single-motor opcode for Boost
    }

    [Fact]
    public void BuildMotorMessage_BoostMoveHub_AbPort()
    {
        var msg = Lpf2HubConnection.BuildMotorMessage(HubType.BoostMoveHub, 0x39, 60, isAbPort: true);

        Assert.Equal(9, msg.Length);
        Assert.Equal(0x02, msg[3]); // dual-motor opcode
        Assert.Equal(60, (int)(sbyte)msg[4]); // speedA
        Assert.Equal(60, (int)(sbyte)msg[5]); // speedB
    }

    [Fact]
    public void BuildMotorMessage_PoweredUpHub_AbPort()
    {
        var msg = Lpf2HubConnection.BuildMotorMessage(HubType.PoweredUpHub, 0x00, 80, isAbPort: true);

        Assert.Equal(57, msg[1]); // AB virtual port ID = 57
        Assert.Equal(0x02, msg[3]);
    }

    [Fact]
    public void BuildLedMessage_CorrectFormat()
    {
        var msg = Lpf2HubConnection.BuildLedMessage(0x32, SensorColor.Red);

        Assert.Equal(0x81, msg[0]);
        Assert.Equal(0x32, msg[1]);
        Assert.Equal(0x11, msg[2]);
        Assert.Equal(0x51, msg[3]);
        Assert.Equal(0x00, msg[4]);
        Assert.Equal((byte)SensorColor.Red, msg[5]);
    }

    [Fact]
    public void BuildLedMessage_RemotePort()
    {
        var msg = Lpf2HubConnection.BuildLedMessage(0x34, SensorColor.Green);
        Assert.Equal(0x34, msg[1]);
        Assert.Equal((byte)SensorColor.Green, msg[5]);
    }
}
