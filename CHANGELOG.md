# Changelog

## 0.1.1

Service locator hardening (no API removed):

- A registered service that is no longer alive (`ServiceRegistry.IsAlive`, wired to destroyed Unity objects on the global registry) now behaves as unregistered: `Get` throws a "destroyed" error, `TryGet` / `IsRegistered` say no, the dead entry is dropped, and the type can be registered again. Before, `Get` returned the dead object and `Register` threw a duplicate error.
- `Get` errors say what is registered and hint at `Register<TInterface>(...)` when the service was registered under its concrete type.
- `ServiceRegistry.RegisteredTypes` and `ServiceLocator.RegisteredTypes`.
- `Rokert.Core.Bindings` clears `ServiceLocator` and `EventBus.Global` at the start of a play session (domain reload off no longer leaks the last run).
- Docs: main thread only, the registry never disposes services, register under the interface.

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
