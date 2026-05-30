using BAP.Core.Enums;
using BAP.Core.Models;

namespace BAP.Tests.Models;

public class ConnectionLimitSettingsTests
{
    [Fact]
    public void None_AllowsAll()
    {
        var settings = new ConnectionLimitSettings { Mode = ConnectionLimitMode.None };
        var project = new TrainProjectModel();

        Assert.True(settings.IsMacAddressAllowed(0xAABBCCDDEEFF, project));
    }

    [Fact]
    public void OnlyProject_AllowsRegistered()
    {
        var project = new TrainProjectModel();
        project.RegisteredTrains.Add(new HubModel { BluetoothAddress = 0xAABBCCDDEEFF });

        var settings = new ConnectionLimitSettings { Mode = ConnectionLimitMode.OnlyProject };

        Assert.True(settings.IsMacAddressAllowed(0xAABBCCDDEEFF, project));
        Assert.False(settings.IsMacAddressAllowed(0x112233445566, project));
    }

    [Fact]
    public void OnlySetList_ChecksList()
    {
        var mac = (ulong)0xAABBCCDDEEFF;
        var macHex = string.Format("{0:X}", mac);

        var settings = new ConnectionLimitSettings
        {
            Mode = ConnectionLimitMode.OnlySetList,
            AllowedDevices = macHex
        };

        Assert.True(settings.IsMacAddressAllowed(mac, new TrainProjectModel()));
        Assert.False(settings.IsMacAddressAllowed(0x112233445566, new TrainProjectModel()));
    }
}
