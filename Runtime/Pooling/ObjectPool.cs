using System;
using System.Collections.Generic;

namespace Rokert.Core
{
    /// <summary>
    /// Engine-agnostic pool. The factory creates instances; onGet/onRelease let the caller
    /// activate and reset them (for example SetActive on a GameObject).
    /// </summary>
    public sealed class ObjectPool<T> where T : class
    {
        private readonly Stack<T> _free = new Stack<T>();
        private readonly Func<T> _factory;
        private readonly Action<T> _onGet;
        private readonly Action<T> _onRelease;
        private readonly int _maxSize;

        public int FreeCount => _free.Count;

        public ObjectPool(Func<T> factory, Action<T> onGet = null, Action<T> onRelease = null, int maxSize = 256)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _onGet = onGet;
            _onRelease = onRelease;
            _maxSize = maxSize;
        }

        public void Prewarm(int count)
        {
            for (int i = 0; i < count && _free.Count < _maxSize; i++)
            {
                T item = _factory();
                _onRelease?.Invoke(item);
                _free.Push(item);
            }
        }

        public T Get()
        {
            T item = _free.Count > 0 ? _free.Pop() : _factory();
            _onGet?.Invoke(item);
            return item;
        }

        /// <summary>Returns an item. If the pool is full the item is dropped (not kept).</summary>
        public void Release(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            _onRelease?.Invoke(item);
            if (_free.Count < _maxSize) _free.Push(item);
        }

        public void Clear() => _free.Clear();
    }
}
