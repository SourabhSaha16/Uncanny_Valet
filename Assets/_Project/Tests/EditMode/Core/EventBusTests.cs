using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Tests.Core
{
    public class EventBusTests
    {
        [Test]
        public void Publish_DeliversEventToSubscriber()
        {
            var bus = new EventBus();
            CarId received = CarId.None;
            bus.Subscribe<CarEntered>(e => received = e.Car);

            bus.Publish(new CarEntered(new CarId(7)));

            Assert.AreEqual(new CarId(7), received);
        }

        [Test]
        public void Unsubscribe_StopsDelivery()
        {
            var bus = new EventBus();
            var count = 0;
            void Handler(CarExited e) => count++;
            bus.Subscribe<CarExited>(Handler);
            bus.Unsubscribe<CarExited>(Handler);

            bus.Publish(new CarExited(new CarId(1)));

            Assert.AreEqual(0, count);
        }

        [Test]
        public void Publish_WithNoSubscribers_DoesNothing()
        {
            var bus = new EventBus();
            Assert.DoesNotThrow(() => bus.Publish(new ShiftStarted()));
        }
    }
}
