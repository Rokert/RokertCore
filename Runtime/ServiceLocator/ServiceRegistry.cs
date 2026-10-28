using System;
using System.Collections.Generic;

namespace Rokert.Core
{
    /// <summary>
    /// Instance-based service registry. Use it at composition roots and game edges
    /// (ads, audio, save, analytics). Core/domain code should receive its dependencies
    /// through constructors instead of calling the locator.
    /// </summary>
    public sealed class ServiceRegistry
    {
        private readonly Dictionary<Type, IService> _services = new Dictionary<Type, IService>();

        /// <summary>
        /// Decides whether a persistent service is still usable when scoped services are cleared.
        /// The Unity binding sets this to drop destroyed UnityEngine.Object instances.
        /// </summary>
        public Func<object, bool> IsAlive { get; set; } = static _ => true;

        /// <summary>Registers a service under T. Throws if T is already registered; use <see cref="Replace{T}"/> to swap.</summary>
        public void Register<T>(T service) where T : IService
        {
            if (service == null) throw new ArgumentNullException(nameof(service));

            Type type = typeof(T);
            if (_services.ContainsKey(type))
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

        /// <summary>Returns the service under T. A missing service is a wiring bug, so this throws.</summary>
        public T Get<T>() where T : IService
        {
            if (_services.TryGetValue(typeof(T), out IService service))
                return (T)service;

            throw new InvalidOperationException(
                $"[ServiceRegistry] {typeof(T).Name} is not registered. Register it in the composition root before use.");
        }

        public bool TryGet<T>(out T service) where T : IService
        {
            if (_services.TryGetValue(typeof(T), out IService found))
            {
                service = (T)found;
                return true;
            }

            service = default;
            return false;
        }

        public bool IsRegistered<T>() where T : IService => _services.ContainsKey(typeof(T));

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
    }
}
