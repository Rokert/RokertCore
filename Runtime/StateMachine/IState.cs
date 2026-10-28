namespace Rokert.Core
{
    /// <summary>A self-contained state: owns its entry, tick and exit logic.</summary>
    public interface IState
    {
        void Enter();
        void Tick();
        void Exit();
    }
}
