namespace BAP.Core.Enums;

/// <summary>
/// What happens when a train exits a section.
/// </summary>
public enum SectionReleaseAction
{
    ResumeSpeed = 0,
    ExecuteCode = 1,
    DoNothing = 2
}
