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
