namespace BAP.Core.Enums;

/// <summary>
/// What action to perform when a program event is triggered.
/// </summary>
public enum EventActionType
{
    DoNothing = 0,
    StopMotor = 1,
    SetSpeed = 2,
    AccelerateOverTime = 3,
    DecelerateOverTime = 4,
    InvertSpeed = 5,
    WaitAndSetSpeed = 6,
    SetSpeedForDurationAndStop = 7,
    ActivateSwitchRight = 8,
    ActivateSwitchLeft = 9,
    ExecuteCode = 10,
    PfxPlaySound = 11,
    PfxLightFx = 12,
    PlaySound = 13
}
