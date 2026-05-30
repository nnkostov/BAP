namespace BAP.Core.Enums;

/// <summary>
/// What sensor condition triggers a program event.
/// </summary>
public enum EventTriggerType
{
    NoEvent = 0,
    ColorChangeTo = 1,
    DistanceIsBelow = 2,
    DistanceIsAbove = 3,
    ButtonPlusPressed = 4,
    ButtonMinusPressed = 5,
    ButtonStopPressed = 6,
    ButtonPowerPressed = 7,
    ButtonsReleased = 8,
    RawValueIsBelow = 9,
    RawValueIsAbove = 10
}
