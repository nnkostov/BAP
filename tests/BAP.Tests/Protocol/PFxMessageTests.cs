using BAP.Ble.Protocol;

namespace BAP.Tests.Protocol;

public class PFxMessageTests
{
    [Fact]
    public void PortToMask_SingleLetter()
    {
        Assert.Equal(0x01, PFxHubConnection.PortToMask("A"));
        Assert.Equal(0x02, PFxHubConnection.PortToMask("B"));
        Assert.Equal(0x04, PFxHubConnection.PortToMask("C"));
        Assert.Equal(0x08, PFxHubConnection.PortToMask("D"));
    }

    [Fact]
    public void PortToMask_NumericChannels()
    {
        Assert.Equal(0x01, PFxHubConnection.PortToMask("1"));
        Assert.Equal(0x10, PFxHubConnection.PortToMask("5"));
        Assert.Equal(0x80, PFxHubConnection.PortToMask("8"));
    }

    [Fact]
    public void PortToMask_MultipleChannels()
    {
        // "1,2,3" → bits 0,1,2 = 0x07
        Assert.Equal(0x07, PFxHubConnection.PortToMask("1,2,3"));
    }

    [Fact]
    public void PortToMask_CaseInsensitive()
    {
        Assert.Equal(0x01, PFxHubConnection.PortToMask("a"));
        Assert.Equal(0x02, PFxHubConnection.PortToMask("b"));
    }

    [Fact]
    public void PortToMask_AllEightChannels()
    {
        Assert.Equal(0xFF, PFxHubConnection.PortToMask("12345678"));
    }
}
