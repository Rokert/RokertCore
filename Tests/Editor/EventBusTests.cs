using System.Collections.Generic;
using NUnit.Framework;

namespace Rokert.Core.Tests
{
    public class EventBusTests
    {
        private readonly struct Scored { public readonly int Points; public Scored(int points) { Points = points; } }
        private readonly struct Other { }

        private EventBus _bus;
        private readonly List<string> _errors = new List<string>();

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
            _errors.Clear();
            CoreLog.OnError = _errors.Add;
        }

        [Test]
        public void Publish_DeliversToSubscribers()
        {
            int received = 0;
            _bus.Subscribe<Scored>(e => received += e.Points);

            _bus.Publish(new Scored(5));

            Assert.AreEqual(5, received);
        }

        [Test]
        public void Publish_OnlyReachesMatchingType()
        {
            int calls = 0;
            _bus.Subscribe<Scored>(_ => calls++);

            _bus.Publish(new Other());

            Assert.AreEqual(0, calls);
        }

        [Test]
        public void Unsubscribe_StopsDelivery()
        {
            int calls = 0;
            void Handler(Scored _) => calls++;
            _bus.Subscribe<Scored>(Handler);
            _bus.Unsubscribe<Scored>(Handler);

            _bus.Publish(new Scored(1));

            Assert.AreEqual(0, calls);
        }

        [Test]
        public void Publish_ThrowingHandlerDoesNotStarveOthers()
        {
            int calls = 0;
            _bus.Subscribe<Scored>(_ => throw new System.InvalidOperationException("boom"));
            _bus.Subscribe<Scored>(_ => calls++);

            _bus.Publish(new Scored(1));

            Assert.AreEqual(1, calls);
            Assert.AreEqual(1, _errors.Count);
        }

        [Test]
        public void Subscribe_DuringPublish_TakesEffectNextPublish()
        {
            int late = 0;
            bool subscribed = false;
            _bus.Subscribe<Scored>(_ =>
            {
                if (subscribed) return;
                subscribed = true;
                _bus.Subscribe<Scored>(__ => late++);
            });

            _bus.Publish(new Scored(1));
            Assert.AreEqual(0, late);

            _bus.Publish(new Scored(1));
            Assert.AreEqual(1, late);
        }

        [Test]
        public void Clear_RemovesAllSubscriptions()
        {
            int calls = 0;
            _bus.Subscribe<Scored>(_ => calls++);

            _bus.Clear();
            _bus.Publish(new Scored(1));

            Assert.AreEqual(0, calls);
        }
    }
}
