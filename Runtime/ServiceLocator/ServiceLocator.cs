using System;
using System.Collections.Generic;

namespace Rokert.Core
{
    /// <summary>Static facade over a default <see cref="ServiceRegistry"/> for convenient access at the edges.</summary>
    public static class ServiceLocator
    {
        public static ServiceRegistry Registry { get; } = new ServiceRegistry();

        public static IReadOnlyCollection<Type> RegisteredTypes => Registry.RegisteredTypes;

        public static void Register<T>(T service) where T : IService => Registry.Register(service);
        public static void Replace<T>(T service) where T : IService => Registry.Replace(service);
        public static T Get<T>() where T : IService => Registry.Get<T>();
        public static bool TryGet<T>(out T service) where T : IService => Registry.TryGet(out service);
        public static bool IsRegistered<T>() where T : IService => Registry.IsRegistered<T>();
        public static void Unregister<T>(T service) where T : IService => Registry.Unregister(service);
        public static void ClearScoped() => Registry.ClearScoped();
        public static void Clear() => Registry.Clear();
    }
}
