using BAP.Ble.Protocol;

namespace BAP.Tests.Protocol;

public class GattConstantsTests
{
    [Fact]
    public void Lpf2UUIDs_AreWellFormed()
    {
        Assert.NotEqual(Guid.Empty, GattConstants.Lpf2Service);
        Assert.NotEqual(Guid.Empty, GattConstants.Lpf2Characteristic);
        Assert.NotEqual(GattConstants.Lpf2Service, GattConstants.Lpf2Characteristic);
    }

    [Fact]
    public void SBrickUUIDs_AreDistinct()
    {
        Assert.NotEqual(GattConstants.SBrickCharacteristic, GattConstants.SBrickCommands);
        Assert.NotEqual(GattConstants.SBrickService, GattConstants.SBrickCharacteristic);
    }

    [Fact]
    public void WedoUUIDs_MotorAndSensorAreDifferent()
    {
        Assert.NotEqual(GattConstants.WedoMotorWrite, GattConstants.WedoSensorValue);
    }

    [Fact]
    public void PFxUUIDs_RxAndTxAreDifferent()
    {
        Assert.NotEqual(GattConstants.PFxRxCharacteristic, GattConstants.PFxTxCharacteristic);
    }

    [Fact]
    public void BuWizzUUIDs_AreWellFormed()
    {
        Assert.NotEqual(Guid.Empty, GattConstants.BuWizzService);
        Assert.NotEqual(Guid.Empty, GattConstants.BuWizzCharacteristic);
    }
}
