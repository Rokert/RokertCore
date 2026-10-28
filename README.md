# Rokert Core (`com.rokert.core`)

Small, engine-agnostic patterns shared across Rokert games (EchoCave, Rokert Fighter, ...).

| Pattern | Type | Use it for |
|---|---|---|
| Service Locator | `ServiceRegistry` / `ServiceLocator` | Composition root and game edges (ads, audio, save, analytics) |
| Event bus | `EventBus` | Decoupled, allocation-free messages between systems (struct events) |
| State machine | `StateMachine<T>` + `IState` | Game flow, character/enemy states |
| Command + undo | `IUndoableCommand` + `CommandHistory` | Moves that can be undone / redone |
| Object pool | `ObjectPool<T>` | Reusing frequently spawned objects |

See `Documentation~/PATTERNS.md` for when to use (and not use) each one.

## Assemblies

- `Rokert.Core` has no engine references. It is plain C# and can be used from pure-domain assemblies and tested without the editor.
- `Rokert.Core.Bindings` connects the core to Unity: it routes `CoreLog` to the Unity console and lets the service locator drop destroyed `UnityEngine.Object` services.
- `Rokert.Core.Tests` (EditMode) lives in `Tests/Editor`.

## Install

While developing, reference it from a game's `Packages/manifest.json` as a local package:

```json
"com.rokert.core": "file:../../RokertCore"
```

(path relative to the game's `Packages/` folder; for `UnityProjects/<Game>` that is `file:../../RokertCore`).

To run the package tests from a game, also add it to `testables`:

```json
"testables": ["com.rokert.core"]
```

### Releasing

Unity ignores assets that have no `.meta` file inside an immutable (git) package. So, before tagging a release:

1. Open a game that references the package via `file:` so Unity generates the `.meta` files.
2. Commit the package together with its `.meta` files.
3. Tag a version (`v0.1.0`) and reference it from other projects:
   `"com.rokert.core": "https://github.com/<user>/RokertCore.git#v0.1.0"`.

## Rules for adding a pattern

- Only extract a pattern after it is used in two projects. Generalizing earlier produces APIs that do not fit.
- Nothing game-specific in this package. If something needs game knowledge, it goes behind an interface.
- Every pattern ships with EditMode tests and a short entry in `Documentation~/PATTERNS.md`.
- Keep `Rokert.Core` free of `UnityEngine`; put engine glue in `Rokert.Core.Bindings`.
