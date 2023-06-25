namespace TemperatureMonitoring.App;

public static class Messages
{
    public sealed class ReadTemperature
    {
        public long? RequestId { get; }

        public ReadTemperature(long? requestId)
        {
            RequestId = requestId;
        }
    }

    public sealed class RespondTemperature
    {
        public double? Value { get; }
        public long? RequestId { get; }

        public RespondTemperature(double? value, long? requestId)
        {
            Value = value;
            RequestId = requestId;
        }
    }

    public sealed class RecordTemperature
    {
        public double Value { get; }
        public long RequestId { get; }

        public RecordTemperature(double value, long requestId)
        {
            RequestId = requestId;
            Value = value;
        }
    }

    public sealed class TemperatureRecorded
    {
        public long RequestId { get; }

        public TemperatureRecorded(long requestId)
        {
            RequestId = requestId;
        }
    }

    public sealed class RequestTrackDevice
    {
        public RequestTrackDevice(string deviceId, string groupId)
        {
            DeviceId = deviceId;
            GroupId = groupId;
        }

        public string DeviceId { get; }
        public string GroupId { get; }
    }

    public sealed class DeviceRegistered
    {
        public static DeviceRegistered Instance { get; } = new();

        private DeviceRegistered(){}
    }
}