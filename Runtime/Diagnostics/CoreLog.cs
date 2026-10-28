using System;

namespace Rokert.Core
{
    /// <summary>
    /// Logging hooks for the engine-free core. Defaults write to the console; the
    /// Rokert.Core.Unity assembly rebinds them to UnityEngine.Debug at startup.
    /// </summary>
    public static class CoreLog
    {
        public static Action<string> OnInfo  = static message => Console.WriteLine(message);
        public static Action<string> OnError = static message => Console.Error.WriteLine(message);

        public static void Info(string message)  => OnInfo?.Invoke(message);
        public static void Error(string message) => OnError?.Invoke(message);
    }
}
