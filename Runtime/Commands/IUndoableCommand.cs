namespace Rokert.Core
{
    /// <summary>A reversible action. Execute and Undo must be exact inverses.</summary>
    public interface IUndoableCommand
    {
        void Execute();
        void Undo();
    }
}
