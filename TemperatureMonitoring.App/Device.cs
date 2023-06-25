using Akka.Actor;
using Akka.Event;
using static TemperatureMonitoring.App.Messages;

namespace TemperatureMonitoring.App;

public class Device : UntypedActor
{
    private double? _lastTemperatureReading = null;
    protected override void PreStart() => Console.WriteLine("Device Actor Started ...");
    protected override void PostStop() => Console.WriteLine("Device Actor Stopped ...");

    protected string? DeviceId { get; }
    protected string? GroupId { get; }
    protected ILoggingAdapter Log { get; } = Context.GetLogger();

    public Device(string? deviceId, string? groupId)
    {
        DeviceId = deviceId;
        GroupId = groupId;
    }
    protected override void OnReceive(object message)
    {
        switch (message)
        {
            case RequestTrackDevice req when req.GroupId.Equals(GroupId) && req.DeviceId.Equals(DeviceId):
                Sender.Tell(DeviceRegistered.Instance);
                break;
            case RequestTrackDevice req:
                Log.Warning($"Ignoring TrackDevice request for {req.GroupId}-{req.DeviceId}.This actor is responsible for {GroupId}-{DeviceId}.");
                break;
            case ReadTemperature read:
                Sender.Tell(new Messages.RespondTemperature(_lastTemperatureReading, read.RequestId));
                break;
            case RecordTemperature rec:
                Log.Info($"Recorded temperature reading {rec.Value} with {rec.RequestId}");
                _lastTemperatureReading = rec.Value;
                Sender.Tell(new Messages.TemperatureRecorded(rec.RequestId));
                break;
        }
    }

    public static Props Props(string groupId, string deviceId)
    {
        return Akka.Actor.Props.Create(() => new Device(deviceId, groupId));
    }
}