using System;
using System.Collections.Generic;
using System.Text;

namespace Rokert.Core
{
    /// <summary>
    /// Instance-based service registry. Use it at composition roots and game edges
    /// (ads, audio, save, analytics). Core/domain code should receive its dependencies
    /// through constructors instead of calling the locator.
    /// Main thread only: there is no locking. The registry never disposes the services it holds; their owner does.
    /// </summary>
    public sealed class ServiceRegistry
    {
        private readonly Dictionary<Type, IService> _services = new Dictionary<Type, IService>();

        /// <summary>
        /// Decides whether a registered service is still usable. A service that is not alive behaves as if it was never
        /// registered: <see cref="Get{T}"/> throws, <see cref="TryGet{T}"/> and <see cref="IsRegistered{T}"/> say no, and
        /// <see cref="ClearScoped"/> drops it even if it is persistent. The Unity binding sets this on the global registry
        /// to treat destroyed UnityEngine.Object instances as dead; a registry you create yourself keeps the default.
        /// </summary>
        public Func<object, bool> IsAlive { get; set; } = static _ => true;

        /// <summary>The types services are registered under. A live view: copy it before changing the registry while iterating.</summary>
        public IReadOnlyCollection<Type> RegisteredTypes => _services.Keys;

        /// <summary>Registers a service under T. Throws if T is already registered; use <see cref="Replace{T}"/> to swap.</summary>
        public void Register<T>(T service) where T : IService
        {
            if (service == null) throw new ArgumentNullException(nameof(service));

            Type type = typeof(T);
            if (TryFind(type, out _, out _)) // a dead entry does not count as a duplicate
                throw new InvalidOperationException(
                    $"[ServiceRegistry] {type.Name} is already registered. Use Replace<T>() to swap it on purpose.");

            _services[type] = service;
            CoreLog.Info($"[ServiceRegistry] Registered: {type.Name}");
        }

        /// <summary>Registers or overwrites the service under T (tests, runtime swaps).</summary>
        public void Replace<T>(T service) where T : IService
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            _services[typeof(T)] = service;
        }

        /// <summary>
        /// Returns the service under T. A missing or destroyed service is a wiring bug, so this throws, and the message
        /// says what is registered and whether the service was registered under a concrete type instead of T.
        /// </summary>
        public T Get<T>() where T : IService
        {
            if (TryFind(typeof(T), out IService service, out bool wasDestroyed))
                return (T)service;

            throw new InvalidOperationException(NotRegisteredMessage(typeof(T), wasDestroyed));
        }

        public bool TryGet<T>(out T service) where T : IService
        {
            if (TryFind(typeof(T), out IService found, out _))
            {
                service = (T)found;
                return true;
            }

            service = default;
            return false;
        }

        public bool IsRegistered<T>() where T : IService => TryFind(typeof(T), out _, out _);

        /// <summary>Removes T only if it still points at <paramref name="service"/> (safe for OnDestroy).</summary>
        public void Unregister<T>(T service) where T : IService
        {
            if (_services.TryGetValue(typeof(T), out IService current) && ReferenceEquals(current, service))
                _services.Remove(typeof(T));
        }

        /// <summary>Drops scene-scoped services; keeps <see cref="IPersistentService"/> ones that are still alive.</summary>
        public void ClearScoped()
        {
            List<KeyValuePair<Type, IService>> keep = null;
            foreach (KeyValuePair<Type, IService> entry in _services)
            {
                if (entry.Value is not IPersistentService) continue;
                if (!IsAlive(entry.Value)) continue;

                keep ??= new List<KeyValuePair<Type, IService>>();
                keep.Add(entry);
            }

            _services.Clear();
            if (keep == null) return;
            foreach (KeyValuePair<Type, IService> entry in keep)
                _services[entry.Key] = entry.Value;
        }

        /// <summary>Drops everything, persistent services included.</summary>
        public void Clear() => _services.Clear();

        // A dead service (a destroyed Unity object, for example) is removed on the spot, so it is never handed out
        // and a new one can be registered under the same type.
        private bool TryFind(Type type, out IService service, out bool wasDestroyed)
        {
            wasDestroyed = false;
            if (!_services.TryGetValue(type, out service)) return false;
            if (IsAlive(service)) return true;

            _services.Remove(type);
            service = null;
            wasDestroyed = true;
            return false;
        }

        private string NotRegisteredMessage(Type type, bool wasDestroyed)
        {
            var message = new StringBuilder("[ServiceRegistry] ").Append(type.Name);
            message.Append(wasDestroyed
                ? " was registered but it has been destroyed. Register it again, or unregister it in OnDestroy."
                : " is not registered. Register it in the composition root before use.");

            foreach (KeyValuePair<Type, IService> entry in _services)
            {
                if (entry.Key == type || !type.IsInstanceOfType(entry.Value)) continue;
                message.Append(" A service that implements it is registered under ").Append(entry.Key.Name)
                       .Append(": register it with Register<").Append(type.Name).Append(">(...).");
                break;
            }

            message.Append(" Registered: ");
            if (_services.Count == 0) message.Append("nothing.");
            else message.Append(string.Join(", ", NamesOf(_services.Keys))).Append('.');
            return message.ToString();
        }

        private static IEnumerable<string> NamesOf(IEnumerable<Type> types)
        {
            foreach (Type type in types) yield return type.Name;
        }
    }
}
