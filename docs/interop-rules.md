# HarmonyOS Interop Rules

This document records the invariants that later commits must preserve when touching `HarmonyOS.Interop` and generated API bindings.

## NAPI Lifetime

- `NapiReference` must track the creating environment and release its native reference exactly once.
- Cross-thread release must be deferred through `NapiFinalizationQueue`; direct deletion is only allowed when the current environment matches the reference's owner.
- Callback and promise bridges must never throw through native callbacks; completion is atomic and each handle is released exactly once.

## Async Cancellation

- Promise and callback void bridges must expose `CancellationToken` overloads and complete with `TaskStatus.Canceled` when cancellation fires.
- `CancellationTokenRegistration` must be released on cancellation, normal completion, and exception paths.
- Once a callback bridge completes, later native callbacks must be ignored and must not re-complete the task.

## Event Registration

- Managed event registration must roll back if native registration fails.
- If native unregistration fails, the managed listener must remain registered and must not be silently dropped.

## Generated Bindings

- Record property writes must surface `NapiException` through `.ThrowIfFailed()`.
- Generated files must derive required usings from the emitted body and include provenance headers with generator and SDK versions.
- Optional values must become C# `null` before NAPI value conversion; explicit ArrayBuffer and `byte[]` channel rules must remain in place.

## MAUI Contract Alignment (Essentials)

- `IPreferences.Set(key, null)`: removes the key (matches dotnet/maui Android and Windows implementations; do not throw).
- `IGeolocation`: throws `FeatureNotEnabledException` when location is disabled and `PermissionException` when permission is denied, in `GetLastKnownLocationAsync`, `GetLocationAsync`, and `StartListeningForegroundAsync` (matches dotnet/maui iOS behavior; StartListening matches Android too).

## Node Events (ArkUINodeBase)

- Event subscription is multicast since c94bc79: `SubscribeEvent` appends; a node-event is registered natively on the first managed subscriber and unregistered when the hub empties.
- Handlers must subscribe at most once per node instance (typically where the node is created) and pair it with the exact-handler `UnsubscribeEvent(eventType, handler)`. The parameterless `UnsubscribeEvent(eventType)` clears the whole hub and must not be used by handlers that share the node.
