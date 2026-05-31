using BAP.Core.Enums;

namespace BAP.Core.Models;

/// <summary>
/// A single automation rule: When trigger X on hub/port Y → Do action Z.
/// </summary>
public class ProgramEventModel
{
    public string? Name { get; set; }
    public ProgramEventType Type { get; set; }

    // ── Trigger configuration ──

    /// <summary>
    /// Device ID of the hub whose sensor triggers this event.
    /// </summary>
    public string? TrainDeviceId { get; set; }

    /// <summary>
    /// Port on the triggering hub.
    /// </summary>
    public string? TrainPort { get; set; }

    /// <summary>
    /// What kind of sensor event triggers this.
    /// </summary>
    public EventTriggerType Trigger { get; set; }

    /// <summary>
    /// Distance threshold for distance-based triggers.
    /// </summary>
    public int TriggerDistanceParam { get; set; }

    /// <summary>
    /// Color for color-based triggers.
    /// </summary>
    public SensorColor TriggerColorParam { get; set; }

    // ── Action configuration ──

    /// <summary>
    /// What action to perform when triggered.
    /// </summary>
    public EventActionType Action { get; set; }

    /// <summary>
    /// Device ID of the target hub for the action.
    /// </summary>
    public string? TargetDeviceId { get; set; }

    /// <summary>
    /// Port on the target hub for the action.
    /// </summary>
    public string? TargetPort { get; set; } = "A";

    /// <summary>
    /// Numeric parameters for the action (speed, time, etc.).
    /// </summary>
    public int[] Param { get; set; } = new int[2];

    /// <summary>
    /// C# code to execute (for ExecuteCode actions).
    /// </summary>
    public string? CodeToRun { get; set; }

    /// <summary>
    /// Path to a sound file (for PlaySound actions).
    /// </summary>
    public string? SoundPath { get; set; }

    /// <summary>
    /// PFx light effect selection.
    /// </summary>
    public string? PfxLightParam { get; set; }

    /// <summary>
    /// PFx light output mask.
    /// </summary>
    public string? Lights { get; set; }

    public ProgramEventModel() { }

    public ProgramEventModel(ProgramEventType type)
    {
        Type = type;
        TargetPort = "A";
        Param = new int[2];
        CodeToRun = type != ProgramEventType.GlobalCode
            ? DefaultEventCode
            : DefaultGlobalCode;
    }

    private const string DefaultEventCode = """
        // Start a motor attached to port A to run at 75% indefinitely
        Hub[0].SetMotorSpeed("A", 75);

        // Wait 1000ms (1 second)
        Wait(1000);

        // Stop the motor attached to port A
        Hub[0].Stop("A");
        """;

    private const string DefaultGlobalCode = """
        // The code in this section will be available in all other programs!
        ///////////////////////////////////////////////////////////////////////

        // Create your own enums
        public enum MyEnum
        {
           MyYellowTrain = 0,
           MyRedTrain = 1
        }

        // Create your own helper functions
        public void MyGlobalFunction()
        {
           // Execute your code here
        }

        // Create your own constants!
        public const int RED_TRAIN = 1;
        public const int YELLOW_TRAIN = 2;
        """;
}
