using NUnit.Framework;

namespace Rokert.Core.Tests
{
    public class ObjectPoolTests
    {
        private sealed class Item { public bool Active; }

        private int _created;

        private ObjectPool<Item> NewPool(int maxSize = 256) =>
            new ObjectPool<Item>(
                () => { _created++; return new Item(); },
                item => item.Active = true,
                item => item.Active = false,
                maxSize);

        [SetUp]
        public void SetUp() => _created = 0;

        [Test]
        public void Get_CreatesWhenEmptyAndActivates()
        {
            Item item = NewPool().Get();

            Assert.AreEqual(1, _created);
            Assert.IsTrue(item.Active);
        }

        [Test]
        public void Release_ThenGet_ReusesTheInstance()
        {
            ObjectPool<Item> pool = NewPool();
            Item first = pool.Get();
            pool.Release(first);

            Item second = pool.Get();

            Assert.AreSame(first, second);
            Assert.AreEqual(1, _created);
        }

        [Test]
        public void Release_ResetsTheItem()
        {
            ObjectPool<Item> pool = NewPool();
            Item item = pool.Get();

            pool.Release(item);

            Assert.IsFalse(item.Active);
        }

        [Test]
        public void Prewarm_FillsTheFreeList()
        {
            ObjectPool<Item> pool = NewPool();

            pool.Prewarm(5);

            Assert.AreEqual(5, pool.FreeCount);
            Assert.AreEqual(5, _created);
        }

        [Test]
        public void Release_BeyondMaxSize_DropsTheItem()
        {
            ObjectPool<Item> pool = NewPool(maxSize: 1);
            Item a = pool.Get();
            Item b = pool.Get();

            pool.Release(a);
            pool.Release(b);

            Assert.AreEqual(1, pool.FreeCount);
        }
    }
}
