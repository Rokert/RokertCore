using UnityEngine;

namespace Rokert.Core.Bindings
{
    /// <summary>
    /// Connects the engine-free core to Unity: routes CoreLog to the console and lets the
    /// service locator drop destroyed UnityEngine.Object services on ClearScoped().
    /// </summary>
    internal static class UnityBindings
    {
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Bind()
        {
            CoreLog.OnInfo  = Info;
            CoreLog.OnError = Debug.LogError;
            ServiceLocator.Registry.IsAlive = static service => service is not Object unityObject || unityObject != null;
        }

        // Info output only in the editor or with ROKERT_DEBUG, like the stripped logging in RokertFighter.
        [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("ROKERT_DEBUG")]
        private static void LogInfo(string message) => Debug.Log(message);

        private static void Info(string message) => LogInfo(message);
    }
}
