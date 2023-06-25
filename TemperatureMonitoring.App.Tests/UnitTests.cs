using Akka.TestKit;
using Akka.TestKit.Xunit2;
using FluentAssertions;

namespace TemperatureMonitoring.App.Tests
{
    public class UnitTests : TestKit
    {
        [Fact]
        public void Device_actor_must_reply_with_empty_reading_if_no_temperature_is_known()
        {
            var probe = CreateTestProbe();
            var deviceActor = Sys.ActorOf(Device.Props("group", "device"));

            deviceActor.Tell(new Messages.ReadTemperature(requestId: 42), probe.Ref);
            var response = probe.ExpectMsg<Messages.RespondTemperature>();
            response.RequestId.Should().Be(42);
            response.Value.Should().Be(null);
        }

        [Fact]
        public void Device_actor_must_reply_with_latest_temperature_reading()
        {
            var probe = CreateTestProbe();
            var deviceActor = Sys.ActorOf(Device.Props("group", "device"));

            deviceActor.Tell(new Messages.RecordTemperature(requestId: 1, value: 24.0), probe.Ref);
            probe.ExpectMsg<Messages.TemperatureRecorded>(s => s.RequestId == 1);

            deviceActor.Tell(new Messages.ReadTemperature(requestId: 2), probe.Ref);
            var response1 = probe.ExpectMsg<Messages.RespondTemperature>();
            response1.RequestId.Should().Be(2);
            response1.Value.Should().Be(24.0);

            deviceActor.Tell(new Messages.RecordTemperature(requestId: 3, value: 55.0), probe.Ref);
            probe.ExpectMsg<Messages.TemperatureRecorded>(s => s.RequestId == 3);

            deviceActor.Tell(new Messages.ReadTemperature(requestId: 4), probe.Ref);
            var response2 = probe.ExpectMsg<Messages.RespondTemperature>();
            response2.RequestId.Should().Be(4);
            response2.Value.Should().Be(55.0);
        }

        [Fact]
        public void Device_actor_must_reply_to_registration_requests()
        {
            var probe = CreateTestProbe();
            var deviceActor = Sys.ActorOf(Device.Props("group", "device"));

            deviceActor.Tell(new Messages.RequestTrackDevice("device", "group"), probe.Ref);
            probe.ExpectMsg<Messages.DeviceRegistered>();
            probe.LastSender.Should().Be(deviceActor);
        }

        [Fact]
        public void Device_actor_must_ignore_wrong_registration_requests()
        {
            var probe = CreateTestProbe();
            var deviceActor = Sys.ActorOf(Device.Props("group", "device"));

            deviceActor.Tell(new Messages.RequestTrackDevice("device", "WrongGroup"), probe.Ref);
            probe.ExpectNoMsg(TimeSpan.FromMilliseconds(500));

            deviceActor.Tell(new Messages.RequestTrackDevice("WrongDevice", "group"), probe.Ref);
            probe.ExpectNoMsg(TimeSpan.FromMilliseconds(500));
        }

        [Fact]
        public void DeviceGroup_actor_must_be_able_to_register_a_device_actor()
        {
            var probe = CreateTestProbe();
            var groupActor = Sys.ActorOf(DeviceGroup.Props("group"));

            groupActor.Tell(new Messages.RequestTrackDevice("device1","group"),probe.Ref);
            probe.ExpectMsg<Messages.DeviceRegistered>();
            var deviceActor1 = probe.LastSender;

            groupActor.Tell(new Messages.RequestTrackDevice("device2","group"),probe.Ref);
            probe.ExpectMsg<Messages.DeviceRegistered>();
            var deviceActor2 = probe.LastSender;

            deviceActor1.Should().NotBe(deviceActor2);

            deviceActor1.Tell(new Messages.RecordTemperature(25.0,0),probe.Ref);
            probe.ExpectMsg<Messages.TemperatureRecorded>(s=> s.RequestId == 0);

            deviceActor2.Tell(new Messages.RecordTemperature(15.0,1),probe.Ref);
            probe.ExpectMsg<Messages.TemperatureRecorded>(s => s.RequestId == 1);
        }

        [Fact]
        public void DeviceGroup_actor_must_ignore_requests_for_wrong_groupId()
        {
            var probe = CreateTestProbe();

            var groupActor = Sys.ActorOf(DeviceGroup.Props("Group"));
            groupActor.Tell(new Messages.RequestTrackDevice("device","wrongGroup"),probe.Ref);
            probe.ExpectNoMsg(TimeSpan.FromMilliseconds(500));
        }

        [Fact]
        public void DeviceGroup_actor_must_return_same_actor_for_same_deviceId()
        {
            var probe = CreateTestProbe();

            var groupActor = Sys.ActorOf(DeviceGroup.Props("group"));

            groupActor.Tell(new Messages.RequestTrackDevice("device1", "group"), probe.Ref);
            probe.ExpectMsg<Messages.DeviceRegistered>();
            var deviceActor1 = probe.LastSender;

            groupActor.Tell(new Messages.RequestTrackDevice("device1", "group"), probe.Ref);
            probe.ExpectMsg<Messages.DeviceRegistered>();
            var deviceActor2 = probe.LastSender;

            deviceActor1.Should().BeEquivalentTo(deviceActor2);


        }
    }
}