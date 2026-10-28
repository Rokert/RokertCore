namespace Rokert.Core
{
    /// <summary>
    /// Marker for services that outlive a scene (audio, ads, save data).
    /// <see cref="ServiceRegistry.ClearScoped"/> keeps them registered.
    /// </summary>
    public interface IPersistentService { }
}
