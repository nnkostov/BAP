using System;

namespace LegoTrainProject.Exceptions
{
    /// <summary>
    /// Base exception for all BAP-specific exceptions.
    /// </summary>
    public class BapException : Exception
    {
        public BapException() { }
        public BapException(string message) : base(message) { }
        public BapException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when a hub connection fails.
    /// </summary>
    public class HubConnectionException : BapException
    {
        public string HubName { get; }
        public string DeviceId { get; }

        public HubConnectionException(string hubName, string message)
            : base(message)
        {
            HubName = hubName;
        }

        public HubConnectionException(string hubName, string deviceId, string message)
            : base(message)
        {
            HubName = hubName;
            DeviceId = deviceId;
        }

        public HubConnectionException(string hubName, string message, Exception innerException)
            : base(message, innerException)
        {
            HubName = hubName;
        }
    }

    /// <summary>
    /// Exception thrown when a Bluetooth operation fails.
    /// </summary>
    public class BluetoothException : BapException
    {
        public ulong BluetoothAddress { get; }

        public BluetoothException(string message) : base(message) { }

        public BluetoothException(string message, ulong bluetoothAddress)
            : base(message)
        {
            BluetoothAddress = bluetoothAddress;
        }

        public BluetoothException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when a port operation fails.
    /// </summary>
    public class PortException : BapException
    {
        public string PortId { get; }
        public string HubName { get; }

        public PortException(string portId, string message)
            : base(message)
        {
            PortId = portId;
        }

        public PortException(string hubName, string portId, string message)
            : base(message)
        {
            HubName = hubName;
            PortId = portId;
        }
    }

    /// <summary>
    /// Exception thrown when a section reservation fails.
    /// </summary>
    public class SectionReservationException : BapException
    {
        public string SectionName { get; }
        public string TrainName { get; }

        public SectionReservationException(string sectionName, string message)
            : base(message)
        {
            SectionName = sectionName;
        }

        public SectionReservationException(string sectionName, string trainName, string message)
            : base(message)
        {
            SectionName = sectionName;
            TrainName = trainName;
        }
    }

    /// <summary>
    /// Exception thrown when program execution fails.
    /// </summary>
    public class ProgramExecutionException : BapException
    {
        public string ProgramName { get; }

        public ProgramExecutionException(string message) : base(message) { }

        public ProgramExecutionException(string programName, string message)
            : base(message)
        {
            ProgramName = programName;
        }

        public ProgramExecutionException(string programName, string message, Exception innerException)
            : base(message, innerException)
        {
            ProgramName = programName;
        }
    }

    /// <summary>
    /// Exception thrown for invalid parameter values.
    /// </summary>
    public class InvalidParameterException : BapException
    {
        public string ParameterName { get; }
        public object ActualValue { get; }

        public InvalidParameterException(string parameterName, string message)
            : base(message)
        {
            ParameterName = parameterName;
        }

        public InvalidParameterException(string parameterName, object actualValue, string message)
            : base(message)
        {
            ParameterName = parameterName;
            ActualValue = actualValue;
        }
    }
}
