using BAP.Core.Enums;
using BAP.Core.Models;
using BAP.Core.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace BAP.Tests.Services;

public class JsonProjectRepositoryTests : IDisposable
{
    private readonly string _tempDir;
    private readonly JsonProjectRepository _repository;

    public JsonProjectRepositoryTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
        _repository = new JsonProjectRepository(NullLogger<JsonProjectRepository>.Instance);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Fact]
    public async Task RoundTrip_PreservesAllFields()
    {
        var project = CreateSampleProject();
        var path = Path.Combine(_tempDir, "test.bap");

        await _repository.SaveAsync(project, path);
        var loaded = await _repository.LoadAsync(path);

        Assert.NotNull(loaded);
        Assert.Single(loaded.RegisteredTrains);
        Assert.Equal("Express", loaded.RegisteredTrains[0].Name);
        Assert.Equal(HubType.PoweredUpHub, loaded.RegisteredTrains[0].Type);
        Assert.Equal("dev-001", loaded.RegisteredTrains[0].DeviceId);
        Assert.Equal(2, loaded.RegisteredTrains[0].RegisteredPorts.Count);

        Assert.Equal("A", loaded.RegisteredTrains[0].RegisteredPorts[0].Id);
        Assert.Equal(PortFunction.TrainMotor, loaded.RegisteredTrains[0].RegisteredPorts[0].Function);
        Assert.Equal(PortDevice.TrainMotor, loaded.RegisteredTrains[0].RegisteredPorts[0].Device);

        Assert.Single(loaded.Programs);
        Assert.Equal("My Program", loaded.Programs[0].Name);
        Assert.Single(loaded.Programs[0].Events);
        Assert.Equal(EventTriggerType.ColorChangeTo, loaded.Programs[0].Events[0].Trigger);
        Assert.Equal(SensorColor.Red, loaded.Programs[0].Events[0].TriggerColorParam);
        Assert.Equal(EventActionType.StopMotor, loaded.Programs[0].Events[0].Action);

        Assert.Equal(2, loaded.Sections.Sections.Count);
        Assert.Equal("Station", loaded.Sections.Sections[0].Name);
        Assert.Equal("A*dev-001", loaded.Sections.Sections[0].Detector);

        Assert.Single(loaded.Sections.Paths);
        Assert.Equal("Main Loop", loaded.Sections.Paths[0].Name);
        Assert.Equal([0, 1], loaded.Sections.Paths[0].Sections);
        Assert.True(loaded.Sections.Paths[0].LoopPath);

        Assert.Equal(ProgramEventType.GlobalCode, loaded.GlobalCode.Type);
    }

    [Fact]
    public async Task SavedFile_IsHumanReadableJson()
    {
        var project = CreateSampleProject();
        var path = Path.Combine(_tempDir, "readable.bap");

        await _repository.SaveAsync(project, path);
        var content = await File.ReadAllTextAsync(path);

        Assert.Contains("\"name\":", content);
        Assert.Contains("Express", content);
        Assert.Contains("poweredUpHub", content);
    }

    [Fact]
    public async Task LoadAsync_ReturnsNull_ForMissingFile()
    {
        var result = await _repository.LoadAsync("/nonexistent/path.bap");
        Assert.Null(result);
    }

    [Fact]
    public async Task LoadAsync_ReturnsNull_ForInvalidJson()
    {
        var path = Path.Combine(_tempDir, "bad.bap");
        await File.WriteAllTextAsync(path, "not json at all");

        var result = await _repository.LoadAsync(path);
        Assert.Null(result);
    }

    private static TrainProjectModel CreateSampleProject()
    {
        return new TrainProjectModel
        {
            RegisteredTrains =
            [
                new HubModel
                {
                    Name = "Express",
                    DeviceId = "dev-001",
                    BluetoothAddress = 0xAABBCCDDEEFF,
                    Type = HubType.PoweredUpHub,
                    TrainMotorPort = "A",
                    RegisteredPorts =
                    [
                        new PortModel { Id = "A", Value = 0, Function = PortFunction.TrainMotor, Device = PortDevice.TrainMotor },
                        new PortModel { Id = "B", Value = 1, Function = PortFunction.Light, Device = PortDevice.LedLights }
                    ]
                }
            ],
            Programs =
            [
                new TrainProgramModel
                {
                    Name = "My Program",
                    Events =
                    [
                        new ProgramEventModel
                        {
                            Type = ProgramEventType.SensorTriggered,
                            TrainDeviceId = "dev-001",
                            TrainPort = "A",
                            Trigger = EventTriggerType.ColorChangeTo,
                            TriggerColorParam = SensorColor.Red,
                            Action = EventActionType.StopMotor,
                            TargetDeviceId = "dev-001",
                            TargetPort = "A"
                        }
                    ]
                }
            ],
            Sections = new SectionsModel
            {
                Sections =
                [
                    new SectionModel { Name = "Station", Detector = "A*dev-001" },
                    new SectionModel { Name = "Bridge" }
                ],
                Paths =
                [
                    new PathModel { Name = "Main Loop", Sections = [0, 1], LoopPath = true }
                ]
            }
        };
    }
}
