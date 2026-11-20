# Level design and gameplay

## District layout

Use one connected walkable district, not a sequence of levels. The current editable JSON bounds are x `-15.5..15.5` and z `-11.7..10` Unity units; player starts at `(0,0)`. The square/fountain/market/alley/palace-side residences and west/east/south promenades create the neighborhood. Bifrost approach is near `(0,8.15)`. The crisis trigger is near `(0,5.5)`. Guard patrol waypoints are `(-1.8,2.8)` and `(1.8,2.8)`.

Treat concept maps as alternatives. The 01.45 and 01.56 maps show enclosed square courts with a central fountain and a side escape/window; the current JSON describes a wider connected district with an east fountain and side promenades. Do not stack these layouts or transfer their image pixels directly into coordinates. Keep the district data as the source of truth and align geometry to it.

### Navigation / sightlines

- Keep the north gate readable from the middle of the square.
- Preserve a clear walking lane through the court; put cover and props at the edge of this lane.
- Use arcades, planters, crates and recesses to break line of sight without creating unreachable traps.
- Keep the west alley and east/south walks connected to the main square so exploration is meaningful.
- Use boundary geometry or convincing skyline dressing at the edge; avoid hard invisible walls through open-looking paths.
- Make stairs/raised bridges optional only if collision and camera can be tested; current JSON is planar x/z and does not define elevation or navmesh.

## Playable loop

1. Start from the main menu and enter the district with no mission-select screen.
2. Wander and learn the patrol before the apple-search crisis begins.
3. A short line explains that the watch sealed the Bifrost while searching for Odin's missing golden apple.
4. Try the illusion, a sound, a movable/noisy prop, cover, disguise or other supported action on the shared world rules.
5. Read guard response: patrol → investigate/suspicion → chase if Loki is exposed → return after losing the lead.
6. Reach the guarded Bifrost approach while the guard is elsewhere or Loki can pass plausibly.
7. Show a brief comic montage of other troubles; then a separate normal-speed Deadpool portal interruption and end card.

No combat, inventory progression, branching missions or fixed three-route puzzle are needed.

## Systemic rules and interaction contract

- Guards use sight, hearing, last-known position and suspicion; every response should point to a visible cause.
- Props expose simple data: `noisy`, `movable`, `solid`, `hearing`, `distraction`. A moved or thrown prop can produce a sound; a solid object can create cover.
- Illusion places a local decoy at the aim point. The guard can investigate if it sees the decoy. It should fade clearly.
- Shapeshift changes recognition, not world rules. Strange sprinting or a visibly implausible action can still raise suspicion.
- Misdirection places/redirects a sound at the aim point; it should use the same hearing rule as noisy props.
- Interactions need a toast/icon/animation response for success and for “out of range/not usable.”

Do not hard-code a menu of accepted action combinations. Any action sequence should emerge from shared prop/sight/hearing rules, with clear deterministic feedback.

## Current gameplay coverage and mismatch

The current source includes a two-point guard patrol; sight and sound investigation; suspicion/chase/return behavior; decoy; disguise and other forms; movable crates and other props; optional hidden discoveries; and an opening at the gate when the guard is distracted, Loki is disguised or a supported form is active. Catching the player resets quickly with a comic line. The source currently checks the Bifrost/gate for escape. **A second physical escape point from the map concepts is not present in the audited gate-check path.** This should be resolved before claiming “multiple exits”: either add one deliberate alternate physical boundary in a future implementation pass or describe current flexibility as multiple ways to create an opening at the same gate. Keep that choice in the open-questions doc; this documentation task does not change it.

## Guard readability

Use one small awareness cue and matching body movement. Patrol is calm; investigation turns toward the stimulus; suspicion pauses/scans; chase leans and accelerates; return visibly abandons the lead. Keep magic emerald separate from the guard-alert palette. If the player is caught, use a short comic reaction, then restart into the same courtyard without punishment.

## Map data conventions

`Assets/Data/asgard-district.json` is human-editable; `Assets/Resources/asgard-district.json` is the runtime resource mirror. Coordinates are Unity world-plane x/z floats in units, `(0,0)` near the district center. Bounds are inclusive movement limits. Interaction and landmark IDs should remain stable. Add an interaction as a JSON record, keep both copies synchronized, then add matching runtime behavior/visuals and update the format docs. `trouble-encounter.json` stores encounter text/scene metadata separately. For layout changes, update JSON, runtime landmark placement, collision, player limits, guard paths, README and any validation checks together.
