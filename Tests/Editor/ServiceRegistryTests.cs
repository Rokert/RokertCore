using System;
using System.Linq;
using NUnit.Framework;

namespace Rokert.Core.Tests
{
    public class ServiceRegistryTests
    {
        private interface IAudio : IService { }
        private interface IAds : IService { }

        private sealed class Audio : IAudio { }
        private sealed class PersistentAds : IAds, IPersistentService { }

        private ServiceRegistry _registry;

        [SetUp]
        public void SetUp()
        {
            CoreLog.OnInfo = _ => { };
            _registry = new ServiceRegistry();
        }

        [Test]
        public void Get_ReturnsRegisteredInstance()
        {
            var audio = new Audio();
            _registry.Register<IAudio>(audio);

            Assert.AreSame(audio, _registry.Get<IAudio>());
        }

        [Test]
        public void Get_Throws_WhenNotRegistered()
        {
            Assert.Throws<InvalidOperationException>(() => _registry.Get<IAudio>());
        }

        [Test]
        public void Register_Throws_OnDuplicate()
        {
            _registry.Register<IAudio>(new Audio());

            Assert.Throws<InvalidOperationException>(() => _registry.Register<IAudio>(new Audio()));
        }

        [Test]
        public void Replace_SwapsTheInstance()
        {
            _registry.Register<IAudio>(new Audio());
            var other = new Audio();

            _registry.Replace<IAudio>(other);

            Assert.AreSame(other, _registry.Get<IAudio>());
        }

        [Test]
        public void TryGet_ReportsPresence()
        {
            Assert.IsFalse(_registry.TryGet<IAudio>(out _));

            _registry.Register<IAudio>(new Audio());

            Assert.IsTrue(_registry.TryGet<IAudio>(out IAudio audio));
            Assert.IsNotNull(audio);
        }

        [Test]
        public void Unregister_IgnoresAStaleInstance()
        {
            var first = new Audio();
            var second = new Audio();
            _registry.Register<IAudio>(first);
            _registry.Replace<IAudio>(second);

            _registry.Unregister<IAudio>(first);

            Assert.AreSame(second, _registry.Get<IAudio>());
        }

        [Test]
        public void ClearScoped_RemovesScopedAndKeepsPersistent()
        {
            var ads = new PersistentAds();
            _registry.Register<IAudio>(new Audio());
            _registry.Register<IAds>(ads);

            _registry.ClearScoped();

            Assert.IsFalse(_registry.IsRegistered<IAudio>());
            Assert.AreSame(ads, _registry.Get<IAds>());
        }

        [Test]
        public void ClearScoped_DropsPersistentServiceThatIsNoLongerAlive()
        {
            _registry.Register<IAds>(new PersistentAds());
            _registry.IsAlive = _ => false;

            _registry.ClearScoped();

            Assert.IsFalse(_registry.IsRegistered<IAds>());
        }

        [Test]
        public void Get_Throws_ForADeadService_AndSaysSo()
        {
            var audio = new Audio();
            _registry.Register<IAudio>(audio);
            _registry.IsAlive = service => !ReferenceEquals(service, audio);

            var ex = Assert.Throws<InvalidOperationException>(() => _registry.Get<IAudio>());

            StringAssert.Contains("destroyed", ex.Message);
        }

        [Test]
        public void DeadService_BehavesAsIfItWasNeverRegistered()
        {
            var audio = new Audio();
            _registry.Register<IAudio>(audio);
            _registry.IsAlive = service => !ReferenceEquals(service, audio);

            Assert.IsFalse(_registry.IsRegistered<IAudio>());
            Assert.IsFalse(_registry.TryGet<IAudio>(out _));
            Assert.IsFalse(_registry.RegisteredTypes.Contains(typeof(IAudio)), "the dead entry is dropped");
        }

        [Test]
        public void DeadService_CanBeRegisteredAgain()
        {
            var first = new Audio();
            var second = new Audio();
            _registry.Register<IAudio>(first);
            _registry.IsAlive = service => !ReferenceEquals(service, first);

            _registry.Register<IAudio>(second); // would throw if the dead entry were still there

            Assert.AreSame(second, _registry.Get<IAudio>());
        }

        [Test]
        public void Get_Message_ListsWhatIsRegistered()
        {
            _registry.Register<IAds>(new PersistentAds());

            var ex = Assert.Throws<InvalidOperationException>(() => _registry.Get<IAudio>());

            StringAssert.Contains("IAudio", ex.Message);
            StringAssert.Contains("IAds", ex.Message);
        }

        [Test]
        public void Get_Message_SaysNothingIsRegistered_OnAnEmptyRegistry()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => _registry.Get<IAudio>());

            StringAssert.Contains("nothing", ex.Message);
        }

        [Test]
        public void Get_Message_HintsAtRegisteringUnderTheInterface()
        {
            _registry.Register(new Audio()); // T is inferred as the concrete Audio, not IAudio

            var ex = Assert.Throws<InvalidOperationException>(() => _registry.Get<IAudio>());

            StringAssert.Contains("Register<IAudio>", ex.Message);
        }

        [Test]
        public void RegisteredTypes_ListsTheKeys()
        {
            _registry.Register<IAudio>(new Audio());
            _registry.Register<IAds>(new PersistentAds());

            CollectionAssert.AreEquivalent(new[] { typeof(IAudio), typeof(IAds) }, _registry.RegisteredTypes);
        }

        [Test]
        public void Clear_RemovesEverything()
        {
            _registry.Register<IAds>(new PersistentAds());

            _registry.Clear();

            Assert.IsFalse(_registry.IsRegistered<IAds>());
        }
    }
}
