# Patterns

## Service Locator (`ServiceRegistry`, `ServiceLocator`)

**Use** at the composition root and at game edges: ads, audio, save data, analytics, scene loading.
**Do not use** inside domain/core code. Core classes receive dependencies through their constructor, so they stay testable and their dependencies stay visible.

```csharp
// Composition root
ServiceLocator.Register<IAdService>(new AdMobService());

// Edge (a presenter or view that needs the service)
var ads = ServiceLocator.Get<IAdService>();
```

- Services implement `IService`. Services that outlive a scene also implement `IPersistentService`.
- `ClearScoped()` on scene teardown keeps persistent services.
- `Register` throws on duplicates. Use `Replace` to swap on purpose (tests, mocks).
- In tests, create a `new ServiceRegistry()` instead of touching the static one.
- Register under the interface you will ask for: `Register<IAudio>(audio)`. `Register(audio)` infers the concrete type and `Get<IAudio>()` then fails (the error message points this out).
- A service that is not alive counts as not registered. On the global `ServiceLocator` the Unity binding treats a destroyed `UnityEngine.Object` as dead, so `Get` throws a clear "destroyed" error instead of returning a fake-null object, and the type can be registered again. A `new ServiceRegistry()` keeps the default (`IsAlive` always true) unless you set it.
- `Get` errors list what is registered; `RegisteredTypes` gives the same list for tooling and tests.
- Main thread only (no locking). The registry never disposes services: the code that creates a service disposes it.
- The Unity binding clears the global `ServiceLocator` and `EventBus.Global` at the start of every play session, so Enter Play Mode with domain reload off does not carry services or subscribers over from the last run.

## Event bus (`EventBus`)

**Use** for broadcast notifications between systems that should not know each other (score changed, level completed).
**Do not use** for request/response or when you need ordering guarantees between handlers.

- Events are `readonly struct`s.
- Always `Unsubscribe` (typically in `OnDisable`).
- A handler that throws is logged and does not stop other handlers.
- Subscribing during a publish takes effect on the next publish.

## State machine (`StateMachine<T>`)

**Use** for game flow (menu, playing, won, lost) and for characters or enemies with clear states.
**Do not use** for two or three booleans; a plain `enum` is simpler.

- Transitions are checked in registration order, first match wins. Any-state transitions go first.
- `Start(initial)` calls `Enter()`; `ForceState` bypasses conditions.

## Command + undo (`IUndoableCommand`, `CommandHistory`)

**Use** when an action must be reversible (undo a move, rewarded-ad undo).
**Do not use** for actions that cannot be exactly inverted; store a snapshot instead.

- `Execute` and `Undo` must be exact inverses.
- A new `Execute` clears the redo stack.

## Object pool (`ObjectPool<T>`)

**Use** for objects created and destroyed constantly (effects, projectiles, UI items).
**Do not use** for rarely spawned objects.

- `onGet` activates, `onRelease` resets (for GameObjects: `SetActive`).
- Items released beyond `maxSize` are dropped, not kept.
