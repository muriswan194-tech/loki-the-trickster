# Unity technical implementation

## Project facts

- Editor: Unity 2022.3.62f1.
- Rendering: Built-in Render Pipeline. Existing code uses Standard materials, skybox, directional light and Unity's built-in IMGUI/UI patterns. Avoid HDRP/URP conversion and unnecessary package installs.
- Packages in `Packages/manifest.json`: built-in `com.unity.modules.imgui` and `com.unity.modules.physics`; no third-party Unity package dependency is required.
- Build scenes: `Assets/Scenes/Lokabrenna.unity` then `Assets/Scenes/LokabrennaEpilogue.unity` in `ProjectSettings/EditorBuildSettings.asset`.

## Existing code/data architecture

| Path | Current responsibility |
|---|---|
| `Assets/Scripts/LokabrennaGame.cs` | Builds the playable district, Loki, guard, camera, HUD/menu, props, movement, abilities, guard perception/state and gate transition at runtime. It is a large central controller and should not gain more unrelated systems. |
| `Assets/Scripts/DistrictData.cs` | District/encounter data classes and simple JSON parsing/loading. |
| `Assets/Scripts/LokabrennaMeshLibrary.cs` | Small authored reusable meshes such as robe/cape/horns/arch/props. |
| `Assets/Scripts/LokabrennaEpilogue.cs` | Comic montage and short live Deadpool interruption/final card. |
| `Assets/Editor/LokabrennaBootstrap.cs` | Ensures the two main Unity scenes exist and have the relevant bootstrap component. |
| `Assets/Editor/LokabrennaArtAssets.cs` | Generates/saves custom mesh assets and texture import settings. |
| `Assets/Editor/LokabrennaRuleChecks.cs`, `LokabrennaStoryChecks.cs` | Editor-side rule/story checks present in project; run only when implementation/verification is requested. |
| `Assets/Data/asgard-district.json` | Bounds, spawn, guard waypoint list, triggers, interactions and landmarks. |
| `Assets/Resources/asgard-district.json` | Synchronized runtime-readable district copy. |
| `Assets/Data/trouble-encounter.json`, `Assets/Resources/trouble-encounter.json` | Encounter records and opening/epilogue lines. |
| `Assets/Resources/Models/` | Generated mesh assets and local statue import. |

Most world content is assembled during `LokabrennaGame.Start()` by code rather than saved as a rich authored hierarchy of prefabs. Scene edits should preserve that bootstrap flow. Consider extracting prefabs/components only in a deliberate refactor after a working scene backup.

## Recommended hierarchy / prefab boundaries

When this is next structured, use a single `LokabrennaRoot` with children for `Environment`, `Landmarks`, `InteractiveProps`, `Characters`, `Camera`, `UI`, and `VFX`. Prefabs should be introduced in small groups: reusable architecture module; one interactive prop prefab with a simple property component; guard; Loki; one magic cue. Keep scene-authored static geometry separate from encounter logic. Avoid moving the runtime builder and scene bootstrap at the same time.

## Component and physics guidance

- Ground and walls: simple static colliders; ramps/stairs only if player path and camera are verified.
- Loki: CharacterController or current movement/collision approach; prevent walking through solid covers and enforce the data bounds.
- Props: Rigidbody only for the few movable/noisy props. Use discrete, deterministic movement and capped impulse; avoid a full destruction/rigidbody pile.
- Guard: current waypoint movement/vision logic is small and does not require NavMesh for this flat district. Add NavMesh only if the layout becomes truly layered and current movement fails.
- Trigger volumes: encounter activation and escape; visual and code positions must share the JSON coordinate source.
- Camera: orbit with mouse, follow smoothing, scroll zoom, and collision check. Verify no clipping into Loki, walls, arches or horns.

## Data workflow

Use stable IDs and document fields. Existing district interactions have `id`, `type`, `label`, `x`, `z`, `radius`, optional `hearing`/`distraction`, and boolean `noisy`/`movable`/`solid`. Encounter data stays separate from map geometry. Every data change should update the Data and Resources mirrors. Add a custom validator only when requested; do not leave temporary verification code in runtime scripts.

## Scene flow

Main scene → menu → one playable district/trouble → escape → epilogue montage → live portal cameo → ending. Restart and caught states should return to the main scene reliably. Do not add playable scenes for montage snapshots. The epilogue should return to the established 3D palette only if required by the final approved story beat.

## Verification boundaries

The project has existing editor checks and a recent temporary preview build in prior work. The last known isolated Unity compile succeeded before the final minor cape/hair visual edits; the current asset-analysis task has not recompiled or playtested the project. Before the next code release, use the actual Unity 2022.3 editor to check compile console, launch the main scene, run the menu-to-escape-to-finale path, and inspect the player/camera at game distance. Do not infer production-ready performance from a static concept board.
