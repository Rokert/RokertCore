using System.Collections.Generic;
using NUnit.Framework;

namespace Rokert.Core.Tests
{
    public class StateMachineTests
    {
        private sealed class RecordingState : IState
        {
            private readonly string _name;
            private readonly List<string> _log;

            public RecordingState(string name, List<string> log) { _name = name; _log = log; }

            public void Enter() => _log.Add(_name + ".Enter");
            public void Tick()  => _log.Add(_name + ".Tick");
            public void Exit()  => _log.Add(_name + ".Exit");
        }

        private List<string> _log;
        private RecordingState _idle;
        private RecordingState _run;
        private RecordingState _dead;
        private StateMachine<RecordingState> _fsm;

        [SetUp]
        public void SetUp()
        {
            _log = new List<string>();
            _idle = new RecordingState("Idle", _log);
            _run = new RecordingState("Run", _log);
            _dead = new RecordingState("Dead", _log);
            _fsm = new StateMachine<RecordingState>();
        }

        [Test]
        public void Start_EntersInitialState()
        {
            _fsm.Start(_idle);

            Assert.AreSame(_idle, _fsm.CurrentState);
            CollectionAssert.AreEqual(new[] { "Idle.Enter" }, _log);
        }

        [Test]
        public void Tick_TransitionsWhenConditionHolds()
        {
            bool go = false;
            _fsm.AddTransition(_idle, _run, () => go);
            _fsm.Start(_idle);
            _log.Clear();

            go = true;
            _fsm.Tick();

            CollectionAssert.AreEqual(new[] { "Idle.Exit", "Run.Enter", "Run.Tick" }, _log);
        }

        [Test]
        public void Tick_WithoutTrigger_OnlyTicksCurrentState()
        {
            _fsm.AddTransition(_idle, _run, () => false);
            _fsm.Start(_idle);
            _log.Clear();

            _fsm.Tick();

            CollectionAssert.AreEqual(new[] { "Idle.Tick" }, _log);
        }

        [Test]
        public void AnyTransition_WinsOverPerStateTransition()
        {
            _fsm.AddTransition(_idle, _run, () => true);
            _fsm.AddAnyTransition(_dead, () => true);
            _fsm.Start(_idle);

            _fsm.Tick();

            Assert.AreSame(_dead, _fsm.CurrentState);
        }

        [Test]
        public void AnyTransition_DoesNotReenterTheCurrentState()
        {
            _fsm.AddAnyTransition(_dead, () => true);
            _fsm.Start(_dead);
            _log.Clear();

            _fsm.Tick();

            CollectionAssert.AreEqual(new[] { "Dead.Tick" }, _log);
        }

        [Test]
        public void ForceState_BypassesConditions()
        {
            _fsm.Start(_idle);

            _fsm.ForceState(_dead);

            Assert.AreSame(_dead, _fsm.CurrentState);
        }

        [Test]
        public void StateChanged_ReportsPreviousAndNext()
        {
            RecordingState from = null, to = null;
            _fsm.StateChanged += (a, b) => { from = a; to = b; };
            _fsm.Start(_idle);

            _fsm.ForceState(_run);

            Assert.AreSame(_idle, from);
            Assert.AreSame(_run, to);
        }
    }
}
