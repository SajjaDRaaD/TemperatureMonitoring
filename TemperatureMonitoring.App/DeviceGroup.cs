using Akka.Actor;
using Akka.Event;

namespace TemperatureMonitoring.App
{
    public class DeviceGroup : UntypedActor
    {
        protected string GroupId { get; }
        protected ILoggingAdapter Log { get; } = Context.GetLogger();
        private Dictionary<string, IActorRef> deviceIdToActor = new();

        public DeviceGroup(string groupId)
        {
            GroupId = groupId;
        }
        protected override void PreStart() => System.Console.WriteLine("Device Group Actor Started ...");
        protected override void PostStop() => System.Console.WriteLine("Device Group Actor Stopped ...");

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case Messages.RequestTrackDevice trackMsg when trackMsg.GroupId.Equals(GroupId):
                    if (deviceIdToActor.TryGetValue(trackMsg.DeviceId, out var actorRef))
                    {
                        actorRef.Forward(trackMsg);
                    }
                    else
                    {
                        Log.Info($"Creating device actor for {trackMsg.DeviceId}");
                        var deviceActor = Context.ActorOf(Device.Props(trackMsg.GroupId, trackMsg.DeviceId),$"device-{trackMsg.DeviceId}");
                        deviceIdToActor.Add(trackMsg.DeviceId,deviceActor);
                        deviceActor.Forward(trackMsg);
                    }
                    break;

                case Messages.RequestTrackDevice trackMsg:
                    Log.Info($"Ignoring TrackDevice request for {trackMsg.GroupId}. This actor is responsible for {GroupId}.");
                    break;
            }
        }

        public static Props Props(string groupId) => Akka.Actor.Props.Create(() => new DeviceGroup(groupId));
    }
}
