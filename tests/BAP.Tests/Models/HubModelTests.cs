using BAP.Core.Enums;
using BAP.Core.Models;

namespace BAP.Tests.Models;

public class HubModelTests
{
    [Fact]
    public void IsTrain_ReturnsFalse_WhenNoTrainMotorPort()
    {
        var hub = new HubModel
        {
            RegisteredPorts =
            [
                new PortModel { Id = "A", Function = PortFunction.Motor },
                new PortModel { Id = "B", Function = PortFunction.Light }
            ]
        };

        Assert.False(hub.IsTrain());
    }

    [Fact]
    public void IsTrain_ReturnsTrue_WhenHasTrainMotorPort()
    {
        var hub = new HubModel
        {
            RegisteredPorts =
            [
                new PortModel { Id = "A", Function = PortFunction.TrainMotor },
                new PortModel { Id = "B", Function = PortFunction.Light }
            ]
        };

        Assert.True(hub.IsTrain());
    }

    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var hub = new HubModel();

        Assert.Equal(2000, hub.ClearingTimeInMs);
        Assert.Equal(40, hub.SpeedWhenAboutToStop);
        Assert.Equal(1.0f, hub.SpeedCoefficient);
        Assert.False(hub.LoopCurrentPath);
        Assert.Empty(hub.RegisteredPorts);
        Assert.False(hub.IsConnected);
        Assert.Equal(SensorColor.Green, hub.LedColor);
    }
}
