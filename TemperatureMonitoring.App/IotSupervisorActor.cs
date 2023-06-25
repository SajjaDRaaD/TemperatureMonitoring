using Akka.Actor;

namespace TemperatureMonitoring.App;

public class IotSupervisorActor : UntypedActor
{
    protected override void PreStart() => Console.WriteLine("IotSupervisor Started ...");
    protected override void PostStop() => Console.WriteLine("IotSupervisor Stopped ...");

    protected override void OnReceive(object message)
    {
    }

    public static Props Props() => Akka.Actor.Props.Create(() => new IotSupervisorActor());
}