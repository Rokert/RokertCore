using System;
using System.Collections.Generic;

namespace Rokert.Core
{
    /// <summary>
    /// Generic FSM. Each tick it evaluates transitions in registration order (first match wins),
    /// any-state transitions before per-state ones, then ticks the current state.
    /// </summary>
    public sealed class StateMachine<T> where T : class, IState
    {
        private sealed class Transition
        {
            public readonly T From;   // null = any state
            public readonly T To;
            public readonly Func<bool> Condition;

            public Transition(T from, T to, Func<bool> condition)
            {
                From = from;
                To = to;
                Condition = condition;
            }
        }

        private readonly List<Transition> _anyTransitions = new List<Transition>();
        private readonly List<Transition> _fromTransitions = new List<Transition>();

        public T CurrentState { get; private set; }

        /// <summary>Raised after a state change, with (previous, next). Previous is null on the first entry.</summary>
        public event Action<T, T> StateChanged;

        /// <summary>Sets the starting state and calls its Enter().</summary>
        public void Start(T initial)
        {
            if (initial == null) throw new ArgumentNullException(nameof(initial));
            ChangeState(initial);
        }

        public void AddTransition(T from, T to, Func<bool> condition)
        {
            if (from == null) throw new ArgumentNullException(nameof(from));
            _fromTransitions.Add(new Transition(from, to, condition));
        }

        /// <summary>Adds a transition that can fire from any state (global interrupts such as death).</summary>
        public void AddAnyTransition(T to, Func<bool> condition) =>
            _anyTransitions.Add(new Transition(null, to, condition));

        public void Tick()
        {
            Transition triggered = FindTriggered();
            if (triggered != null) ChangeState(triggered.To);

            CurrentState?.Tick();
        }

        /// <summary>Changes state immediately, bypassing transition conditions.</summary>
        public void ForceState(T state) => ChangeState(state);

        private Transition FindTriggered()
        {
            for (int i = 0; i < _anyTransitions.Count; i++)
            {
                Transition t = _anyTransitions[i];
                if (!ReferenceEquals(t.To, CurrentState) && t.Condition()) return t;
            }

            for (int i = 0; i < _fromTransitions.Count; i++)
            {
                Transition t = _fromTransitions[i];
                if (ReferenceEquals(t.From, CurrentState) && t.Condition()) return t;
            }

            return null;
        }

        private void ChangeState(T next)
        {
            if (ReferenceEquals(next, CurrentState)) return;

            T previous = CurrentState;
            previous?.Exit();
            CurrentState = next;
            next?.Enter();
            StateChanged?.Invoke(previous, next);
        }
    }
}
