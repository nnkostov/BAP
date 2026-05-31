namespace BAP.Core.Enums;

/// <summary>
/// The type of device attached to a port.
/// </summary>
public enum PortDevice
{
    Unknown = 0,
    BasicMotor = 1,
    TrainMotor = 2,
    LedLights = 8,
    BoostLed = 22,
    WeDo2Tilt = 34,
    WeDo2Distance = 35,
    BoostDistance = 37,
    BoostExtMotor = 38,
    BoostMotor = 39,
    BoostTilt = 40,
    DuploTrainBaseMotor = 41,
    DuploTrainBaseSpeaker = 42,
    DuploTrainBaseColor = 43,
    DuploTrainBaseSpeedometer = 44,
    ControlPlusLMotor = 46,
    ControlPlusXlMotor = 47,
    PoweredUpRemoteButton = 55,
    EV3Sensor = 400,
    EV3Motor = 401,
    NxtSensor = 402,
    NxtMotor = 403,
    EV3ColorSensor = 404
}
