using BAP.Core.Enums;
using BAP.Core.Models;

namespace BAP.Tests.Models;

public class TrainProjectModelTests
{
    [Fact]
    public void GetHubByDeviceId_ReturnsMatchingHub()
    {
        var project = new TrainProjectModel();
        var hub = new HubModel { DeviceId = "device-1", Name = "My Train" };
        project.RegisteredTrains.Add(hub);

        var found = project.GetHubByDeviceId("device-1");

        Assert.NotNull(found);
        Assert.Equal("My Train", found.Name);
    }

    [Fact]
    public void GetHubByDeviceId_ReturnsNull_WhenNotFound()
    {
        var project = new TrainProjectModel();
        project.RegisteredTrains.Add(new HubModel { DeviceId = "device-1" });

        Assert.Null(project.GetHubByDeviceId("nonexistent"));
    }

    [Fact]
    public void NewProject_HasEmptyCollections()
    {
        var project = new TrainProjectModel();

        Assert.Empty(project.RegisteredTrains);
        Assert.Empty(project.Programs);
        Assert.NotNull(project.Sections);
        Assert.NotNull(project.GlobalCode);
        Assert.Equal(ProgramEventType.GlobalCode, project.GlobalCode.Type);
    }
}
