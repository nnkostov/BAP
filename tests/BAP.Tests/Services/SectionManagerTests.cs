using BAP.Core.Enums;
using BAP.Core.Interfaces;
using BAP.Core.Models;
using BAP.Core.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace BAP.Tests.Services;

public class SectionManagerTests
{
    private static SectionManager CreateManager()
    {
        return new SectionManager(
            NullLogger<SectionManager>.Instance,
            new FakeHubConnectionFactory());
    }

    [Fact]
    public void Start_SetsIsRunningTrue()
    {
        var mgr = CreateManager();
        var project = new TrainProjectModel();
        project.Sections.Sections.Add(new SectionModel { Name = "S1" });

        mgr.Start(project);

        Assert.True(mgr.IsRunning);
    }

    [Fact]
    public void Stop_SetsIsRunningFalse()
    {
        var mgr = CreateManager();
        var project = new TrainProjectModel();
        mgr.Start(project);

        mgr.Stop();

        Assert.False(mgr.IsRunning);
    }

    [Fact]
    public void ClearHub_RemovesHubFromAllSections()
    {
        var mgr = CreateManager();
        var hub = new HubModel { Name = "Train1" };
        var section = new SectionModel { Name = "S1", CurrentHub = hub };

        var project = new TrainProjectModel();
        project.Sections.Sections.Add(section);
        mgr.Start(project);

        mgr.ClearHub(hub);

        Assert.Null(section.CurrentHub);
    }

    [Fact]
    public void Release_ClearsSection()
    {
        var mgr = CreateManager();
        var hub = new HubModel { Name = "Train1" };
        var section = new SectionModel { Name = "S1", CurrentHub = hub };

        var project = new TrainProjectModel();
        project.Sections.Sections.Add(section);
        mgr.Start(project);

        mgr.Release("S1");

        Assert.Null(section.CurrentHub);
    }

    [Fact]
    public void IsTrainInNetwork_ReturnsTrueWhenPresent()
    {
        var mgr = CreateManager();
        var hub = new HubModel { Name = "Train1" };
        var section = new SectionModel { Name = "S1", CurrentHub = hub };

        var project = new TrainProjectModel();
        project.Sections.Sections.Add(section);
        mgr.Start(project);

        Assert.True(mgr.IsTrainInNetwork(hub));
    }

    [Fact]
    public void IsTrainInNetwork_ReturnsFalseWhenAbsent()
    {
        var mgr = CreateManager();
        var hub = new HubModel { Name = "Train1" };
        var section = new SectionModel { Name = "S1" };

        var project = new TrainProjectModel();
        project.Sections.Sections.Add(section);
        mgr.Start(project);

        Assert.False(mgr.IsTrainInNetwork(hub));
    }

    [Fact]
    public async Task HandleSensorTriggerAsync_NoHub_DoesNotThrow()
    {
        var mgr = CreateManager();
        var section = new SectionModel { Name = "S1" };
        var project = new TrainProjectModel();
        project.Sections.Sections.Add(section);
        mgr.Start(project);

        // Should not throw
        await mgr.HandleSensorTriggerAsync(section);
    }

    [Fact]
    public void Start_ResetsReservations()
    {
        var mgr = CreateManager();
        var hub = new HubModel { Name = "Train1" };
        var section = new SectionModel
        {
            Name = "S1",
            ReservedBy = hub,
            IsBeingCleared = true
        };

        var project = new TrainProjectModel();
        project.Sections.Sections.Add(section);
        mgr.Start(project);

        Assert.Null(section.ReservedBy);
        Assert.False(section.IsBeingCleared);
    }

    private class FakeHubConnectionFactory : IHubConnectionFactory
    {
        public IHubConnection Create(HubModel hub) =>
            throw new NotImplementedException();
    }
}
