using System;
using System.Collections.Generic;

namespace Rokert.Core
{
    /// <summary>
    /// Typed publish/subscribe bus. Events are structs. Publishing allocates nothing:
    /// handlers live in a copy-on-write array, so subscribing during a publish is safe.
    /// </summary>
    public sealed class EventBus
    {
        public static EventBus Global { get; } = new EventBus();

        private sealed class Channel<T> where T : struct
        {
            public Action<T>[] Handlers = Array.Empty<Action<T>>();
        }

        private readonly Dictionary<Type, object> _channels = new Dictionary<Type, object>();

        public void Subscribe<T>(Action<T> handler) where T : struct
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            Channel<T> channel = GetOrCreate<T>();
            Action<T>[] old = channel.Handlers;
            var next = new Action<T>[old.Length + 1];
            Array.Copy(old, next, old.Length);
            next[old.Length] = handler;
            channel.Handlers = next;
        }

        public void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            if (!_channels.TryGetValue(typeof(T), out object boxed)) return;

            var channel = (Channel<T>)boxed;
            Action<T>[] old = channel.Handlers;
            int index = Array.IndexOf(old, handler);
            if (index < 0) return;

            var next = new Action<T>[old.Length - 1];
            Array.Copy(old, 0, next, 0, index);
            Array.Copy(old, index + 1, next, index, old.Length - index - 1);
            channel.Handlers = next;
        }

        /// <summary>Invokes every subscriber. One throwing handler never starves the others.</summary>
        public void Publish<T>(in T gameEvent) where T : struct
        {
            if (!_channels.TryGetValue(typeof(T), out object boxed)) return;

            Action<T>[] snapshot = ((Channel<T>)boxed).Handlers;
            for (int i = 0; i < snapshot.Length; i++)
            {
                try
                {
                    snapshot[i](gameEvent);
                }
                catch (Exception ex)
                {
                    CoreLog.Error($"[EventBus] Handler for {typeof(T).Name} threw: {ex}");
                }
            }
        }

        public void Clear() => _channels.Clear();

        private Channel<T> GetOrCreate<T>() where T : struct
        {
            if (_channels.TryGetValue(typeof(T), out object boxed))
                return (Channel<T>)boxed;

            var channel = new Channel<T>();
            _channels[typeof(T)] = channel;
            return channel;
        }
    }
}
