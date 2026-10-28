# Changelog

## 0.1.0

- Service Locator: `ServiceRegistry` (instance) + `ServiceLocator` (static facade), `IService`, `IPersistentService`.
- `EventBus`: instance-based with a `Global` default, allocation-free publish, copy-on-write subscribers.
- `StateMachine<T>` + `IState`: any-state and per-state transitions, `StateChanged` event.
- `IUndoableCommand` + `CommandHistory` (undo / redo).
- `ObjectPool<T>` with prewarm and max size.
- `Rokert.Core.Bindings`: Unity logging and destroyed-object handling.

Differences from the originals in Rokert Fighter (`Rokert` namespace):

- Namespace is `Rokert.Core`, so both can coexist during migration.
- `Register` throws on a duplicate; the old one logged an error and overwrote. Use `Replace` for intentional swaps.
- `EventBus.Publish` no longer allocates (the old one called `GetInvocationList()` on every publish).
- `StateMachine.SetInitialState` became `Start`, which also calls `Enter()` on the first state.
- Registries and the bus are instances, so tests can use isolated ones instead of static state.
