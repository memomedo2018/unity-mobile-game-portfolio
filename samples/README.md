# Standalone gameplay code samples

These examples were created for this portfolio with AI assistance. They are small reviewable demonstrations, not source extracted from ROADFORGE or another commercial game. Their tests describe the intended behavior; they are not evidence of production use or years of development history.

## Run and review

From the repository root, with the .NET 8 SDK installed:

```sh
dotnet run --project tests/PortfolioSamples.Tests.csproj --configuration Release
```

The console runner executes 23 named behavior tests and exits nonzero on a failure. It uses temporary, uniquely named test directories and removes only its own fixtures. No NuGet test framework is needed. GitHub Actions runs the same command on Windows and Linux.

The core uses ordinary C# without `UnityEngine` dependencies. This allows fast verification without opening an editor. The Unity UI adapter is excluded from the .NET build because it needs Unity and uGUI assemblies.

## 1. Mobile input

[Core](Core/MobileStick.cs) · [Unity adapter](Unity/TouchStickView.cs)

The first pointer owns the joystick until release or cancellation. A second finger cannot steal control or release it. A radial dead zone is remapped to the full output range, and diagonal movement is clamped to unit magnitude. Invalid numeric input produces zero movement.

### Unity setup

1. Copy `Core/MobileStick.cs` and `Unity/TouchStickView.cs` into a scratch Unity project.
2. Create a Canvas with a GraphicRaycaster and a UI Image with Raycast Target enabled.
3. Ensure an EventSystem and an appropriate UI input module exist.
4. Attach `TouchStickView` to the Image. Radius is measured in local UI units, so the adapter converts screen coordinates through the canvas camera.
5. Read `Value` from your movement component. For a CharacterController, movement could be:

```csharp
Vector2 input = touchStick.Value;
Vector3 planarVelocity = new Vector3(input.x, 0, input.y) * moveSpeed;
controller.Move(planarVelocity * Time.deltaTime);
```

That snippet omits gravity, camera-relative movement and animation deliberately. Those belong to the game-specific movement layer. The adapter cancels input on focus loss, app pause, or disable, preventing a stuck movement command. Runtime inspector changes to radius/dead zone require reinitializing the component.

**Manual integration checks:** drag in all directions; hold one finger and tap with another; release outside the Image; background and restore the app while holding; test at different Canvas scales. The adapter is compiler-checked against Unity 6 assemblies locally; these device checks have not been performed as part of the standalone test suite.

## 2. Enemy decision logic

[Core](Core/EnemyBrain.cs)

Perception supplies target visibility and distance; the brain returns a state and a one-tick attack signal. It does not move a Transform, find a target, or apply damage. Separating those responsibilities makes transitions deterministic and tests inexpensive.

```csharp
EnemyDecision decision = brain.Tick(Time.deltaTime, canSeeTarget, targetDistance);
// Chase: request movement through your navigation layer.
// Attack: stop movement and face the target.
// ShouldAttack: trigger one attack through your combat layer.
```

The default detection range is 12 units, but an engaged enemy forgets the target beyond 16. Attack starts at 2 units and remains active until the target leaves 3 units. These separate boundaries avoid rapid state changes near a threshold. Losing visibility goes idle immediately; adding a timed search state would be a separate feature.

Cooldown keeps running while the target is lost. A long frame can emit at most one attack, preventing catch-up damage bursts. `Reset()` clears the state and timer for reuse in an object pool. Call `Tick` once per simulation update; supplying zero delta intentionally pauses cooldown progression. Choose scaled or unscaled time explicitly in the Unity adapter.

## 3. Local progress save

[Core](Core/ProgressSave.cs)

`Progress` validates its bounds, `ProgressCodec` handles the envelope, and `FileProgressStore` handles files. A save contains a schema version, level, coins, and a SHA-256 checksum. The checksum detects accidental corruption; it does not encrypt the data or prevent cheating.

```csharp
var store = new FileProgressStore(Application.persistentDataPath);
LoadResult result = store.Load();
Progress progress = result.Progress;
// Surface recovery/default status in your own UI or diagnostics when appropriate.
store.Save(new Progress(unlockedLevel: 3, coins: 120));
```

Writes use a unique temporary file in the same directory, flush it, then replace the primary. A valid previous primary is copied to the backup before replacement. A corrupted primary never overwrites a good backup. Loading reports whether primary, backup or defaults were used, so recovery is observable. Permission and unexpected I/O failures propagate to the caller instead of being silently reported as a successful save.

**Scope and tradeoffs:**

- Single writer only. Concurrent saves need serialization/locking.
- Local filesystem sample for desktop/mobile. WebGL, consoles, cloud sync and platform storage APIs require different adapters and platform validation.
- Replacement semantics depend on the target filesystem. Power-loss durability is not guaranteed by these tests; directory syncing and device-specific storage guarantees are outside this sample.
- The tiny payload is synchronous. Schedule real saves deliberately rather than writing every frame.
- Unknown versions are rejected; no migration policy is implemented. A production app should avoid overwriting saves from a newer version without a deliberate recovery decision.
- No purchased currency, account secrets or personal information should be protected using this checksum alone.

## Evidence and limits

- 23 behavior tests cover input ownership/cancellation, dead zones, bounded magnitude, state boundaries, cooldowns, malformed saves, backup recovery, file replacement, and observable storage failure.
- The Unity adapter is thin and compiler-checked locally; automatic CI covers core behavior only.
- No complete game project, proprietary source, third-party asset pack or signing key is included.

See the [CI runs](https://github.com/memomedo2018/unity-mobile-game-portfolio/actions/workflows/samples.yml) for the current commit's results.
