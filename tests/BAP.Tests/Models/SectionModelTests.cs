using BAP.Core.Models;

namespace BAP.Tests.Models;

public class SectionModelTests
{
    [Fact]
    public void GetDetector_ParsesReference()
    {
        var section = new SectionModel { Detector = "A*device-123" };

        Assert.Equal("A", section.GetDetectorPort());
        Assert.Equal("device-123", section.GetDetectorDeviceId());
    }

    [Fact]
    public void GetSwitch_ParsesReference()
    {
        var section = new SectionModel { Switch = "B*device-456" };

        Assert.Equal("B", section.GetSwitchPort());
        Assert.Equal("device-456", section.GetSwitchDeviceId());
    }

    [Fact]
    public void GetDetector_ReturnsNull_WhenNoDetector()
    {
        var section = new SectionModel();

        Assert.Null(section.GetDetectorPort());
        Assert.Null(section.GetDetectorDeviceId());
    }

    [Fact]
    public void IsDetectorPresent_ReturnsTrue_WhenMatches()
    {
        var section = new SectionModel { Detector = "C*dev-789" };

        Assert.True(section.IsDetectorPresent("C", "dev-789"));
        Assert.False(section.IsDetectorPresent("A", "dev-789"));
    }
}
