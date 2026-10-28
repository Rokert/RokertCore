using System.Collections.Generic;

namespace Rokert.Core
{
    /// <summary>
    /// Executes commands and keeps them so they can be undone (and redone).
    /// Executing a new command discards the redo stack.
    /// </summary>
    public sealed class CommandHistory
    {
        private readonly Stack<IUndoableCommand> _undo = new Stack<IUndoableCommand>();
        private readonly Stack<IUndoableCommand> _redo = new Stack<IUndoableCommand>();

        public int UndoCount => _undo.Count;
        public int RedoCount => _redo.Count;
        public bool CanUndo => _undo.Count > 0;
        public bool CanRedo => _redo.Count > 0;

        public void Execute(IUndoableCommand command)
        {
            command.Execute();
            _undo.Push(command);
            _redo.Clear();
        }

        public bool Undo()
        {
            if (_undo.Count == 0) return false;

            IUndoableCommand command = _undo.Pop();
            command.Undo();
            _redo.Push(command);
            return true;
        }

        public bool Redo()
        {
            if (_redo.Count == 0) return false;

            IUndoableCommand command = _redo.Pop();
            command.Execute();
            _undo.Push(command);
            return true;
        }

        public void Clear()
        {
            _undo.Clear();
            _redo.Clear();
        }
    }
}
