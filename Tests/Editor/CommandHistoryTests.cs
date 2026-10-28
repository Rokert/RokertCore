using NUnit.Framework;

namespace Rokert.Core.Tests
{
    public class CommandHistoryTests
    {
        private sealed class AddCommand : IUndoableCommand
        {
            private readonly int[] _total;
            private readonly int _amount;

            public AddCommand(int[] total, int amount) { _total = total; _amount = amount; }

            public void Execute() => _total[0] += _amount;
            public void Undo()    => _total[0] -= _amount;
        }

        private CommandHistory _history;
        private int[] _total;

        [SetUp]
        public void SetUp()
        {
            _history = new CommandHistory();
            _total = new[] { 0 };
        }

        [Test]
        public void Execute_RunsTheCommand()
        {
            _history.Execute(new AddCommand(_total, 3));

            Assert.AreEqual(3, _total[0]);
            Assert.IsTrue(_history.CanUndo);
        }

        [Test]
        public void Undo_RevertsTheLastCommand()
        {
            _history.Execute(new AddCommand(_total, 3));
            _history.Execute(new AddCommand(_total, 4));

            Assert.IsTrue(_history.Undo());

            Assert.AreEqual(3, _total[0]);
        }

        [Test]
        public void Redo_ReappliesAnUndoneCommand()
        {
            _history.Execute(new AddCommand(_total, 3));
            _history.Undo();

            Assert.IsTrue(_history.Redo());

            Assert.AreEqual(3, _total[0]);
        }

        [Test]
        public void Execute_DiscardsTheRedoStack()
        {
            _history.Execute(new AddCommand(_total, 3));
            _history.Undo();

            _history.Execute(new AddCommand(_total, 7));

            Assert.IsFalse(_history.CanRedo);
            Assert.AreEqual(7, _total[0]);
        }

        [Test]
        public void UndoAndRedo_OnEmptyHistory_ReturnFalse()
        {
            Assert.IsFalse(_history.Undo());
            Assert.IsFalse(_history.Redo());
        }
    }
}
