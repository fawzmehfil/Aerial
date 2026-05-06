# Aerial

Aerial is a Unity 3D FPV drone racing vertical slice. The drone moves forward automatically at fixed speed while the player controls horizontal and vertical positioning to pass through neon checkpoint rings in order.

## Unity Version

Built for Unity `6000.0.74f1`.

## Open the Project

1. Open Unity Hub.
2. Add this folder as a project: `/Users/fawzmehfil/Desktop/Aerial`.
3. Open with Unity `6000.0.74f1`.
4. Open `Assets/Scenes/Main.unity`.

The project includes committed starter prefabs under `Assets/Prefabs`. The editor bootstrap script can also rebuild generated scene/prefab assets from the `Drift/Rebuild Generated Scene And Prefabs` menu if needed.

You can also press Play from an empty Unity scene; the runtime bootstrap creates the game manager automatically before the scene loads.

## Verification

Run `Tools/verify_project_structure.sh` to check that the source project contains the expected Unity scene, packages, scripts, prefabs, and level data.

The project also includes a package-free Unity editor verifier that checks required gameplay types, level data, project assets, build settings, progression, and runtime level bootstrap. When Unity licensing is available, run it with:

```bash
/Applications/Unity/Hub/Editor/6000.0.74f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -projectPath . \
  -executeMethod DriftEditor.DriftProjectVerifier.RunBatch \
  -logFile /tmp/drift-verifier.log \
  -quit
```

The same verifier is available from `Drift/Run Project Verifier` inside the Unity editor.

## GitHub Notes

This repository is set up to commit source files, project settings, packages, scenes, prefabs, and their `.meta` files. Unity-generated folders such as `Library/`, `Temp/`, `Logs/`, and `UserSettings/` are intentionally ignored.

Before pushing, confirm the working tree is clean:

```bash
git status --short
```

## How to Play

- Use the main menu to start the first unlocked level or open level select.
- Fly through the highlighted current ring.
- Rings must be passed in order.
- Missing the current ring, hitting an obstacle, or leaving the playable boundary quickly resets the level.
- Completing a level unlocks the next level.

## Controls

- `W` / Up Arrow: move up
- `S` / Down Arrow: move down
- `A` / Left Arrow: move left
- `D` / Right Arrow: move right
- `R`: restart current level
- `Esc`: pause, resume, or return from level select

There is no throttle, braking, or manual forward movement. Forward speed is fixed per level.

## Levels

The project includes five handcrafted routes:

1. Basic Flow: large rings, gentle movement, no obstacles.
2. Signal Wave: alternating horizontal and vertical ring patterns.
3. Rhythm Arc: smoother rhythm-game arcs and center-to-edge transitions.
4. Gate Run: obstacle walls and pillars around readable ring gates.
5. Precision Neon: tighter rings, faster fixed speed, and more demanding control.

## Adding Levels

Add another `LevelDefinition` entry in `Assets/Scripts/LevelCatalog.cs`.

Each level defines:

- Level number and display text
- Fixed forward speed
- Playable horizontal and vertical boundary
- Ordered `RingSpec` positions and radii
- Optional `ObstacleSpec` positions and sizes

The runtime managers generate the ring meshes, triggers, obstacle geometry, guide rails, particles, drone, camera target, and UI from that data.

## Architecture

- `DroneController`: fixed forward movement, lateral input, smoothing, damping, boundary checks, and visual tilt.
- `CameraFollow`: smooth chase camera and subtle FOV response to lateral motion.
- `RingCheckpoint`: individual ring trigger and visual state.
- `RingManager`: ordered checkpoint progression, miss detection, and ring progress.
- `LevelManager`: builds and resets levels from `LevelDefinition` data.
- `GameManager`: game state, pause/restart/failure/completion, audio feedback, and progression.
- `UIManager`: package-free runtime main menu, level select, HUD, pause, failure, and completion panels.
- `LevelSelectManager`: locked/completed level button state.
- `BoundaryReset`: playable volume failure detection.
- `ObstacleReset`: obstacle collision/failure detection.
- `RuntimeVisualFactory`: primitive drone, neon ring, obstacle, guide rail, particles, materials, and procedural audio helpers.
- `DriftProjectBootstrapper`: editor-only scene, build settings, and prefab generation.

Progress is saved locally with `PlayerPrefs` keys `Drift.HighestUnlockedLevel` and `Drift.CompletedLevels`.
