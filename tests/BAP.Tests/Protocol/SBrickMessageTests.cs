using BAP.Ble.Protocol;

namespace BAP.Tests.Protocol;

public class SBrickMessageTests
{
    [Fact]
    public void BuildMotorCommand_Forward()
    {
        var cmd = SBrickHubConnection.BuildMotorCommand(0x00, 50);

        Assert.Equal(0x01, cmd[0]);
        Assert.Equal(0x00, cmd[1]); // channel
        Assert.Equal(0x00, cmd[2]); // direction: forward
        Assert.Equal((byte)(50 * 2.5), cmd[3]); // power
    }

    [Fact]
    public void BuildMotorCommand_Reverse()
    {
        var cmd = SBrickHubConnection.BuildMotorCommand(0x02, -40);

        Assert.Equal(0x01, cmd[0]);
        Assert.Equal(0x02, cmd[1]); // channel
        Assert.Equal(0x01, cmd[2]); // direction: reverse
        Assert.Equal((byte)(40 * 2.5), cmd[3]); // power (absolute)
    }

    [Fact]
    public void BuildMotorCommand_Stop()
    {
        var cmd = SBrickHubConnection.BuildMotorCommand(0x01, 0);

        Assert.Equal(0x01, cmd[2]); // reverse direction (speed 0 is not > 0)
        Assert.Equal(0, cmd[3]);    // zero power
    }
}
