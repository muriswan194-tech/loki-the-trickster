# Character design bible

## Player: Loki / the trickster

The project's narrative calls the player Loki; the generated art boards label him “the Trickster.” Keep the approved reference traits recognizable in broad silhouette while making the Unity model original: slim agile build, long dark hair, tall swept horned head silhouette, high collar, dark underlayer, long deep-green coat/cape and restrained gold trim. The green/gold identity must survive the third-person camera and read from behind. The expression/body language is poised, observant and theatrically sly rather than aggressive.

Use layered shapes and controlled asymmetry; do not translate the illustration into a stack of obvious boxes or an untextured capsule. The cape should have visible volume/edge, avoid clipping into the body, and cast/receive shadows appropriately. Face details need to read at close/menu views; silhouette and large color blocks matter most at play distance. Magical green belongs to effect accents, not large areas that make the costume blend into illusions.

### Required motion

- Idle: relaxed but alert, slight weight shift/head look.
- Walk/run: stable stride, feet grounded, cloak follow-through.
- Crouch/sneak if retained: lower center and reduced speed, distinct from normal walk.
- Illusion: short hand/aim cue, emerald flash or arc, return to locomotion quickly.
- Disguise: brief readable shimmer, then the borrowed guard silhouette; do not obscure control.
- Caught: harmless comic reaction, quick recovery/restart.

### Current implementation note

The current player visuals are created by `BuildLokiAppearance()` in `LokabrennaGame.cs` using custom procedural mesh assets and Unity primitives; motion is code-driven. There is no imported, skinned Loki rig or polished animation set in the audited assets. Recent source edits add costume/face details and cape handling, but this documentation pass did not modify or visually validate that work. A real modeled/rigged character remains a separate art task; do not describe these runtime parts as a production character asset.

## Guard

Create an original royal-watch silhouette: stable stance, broader shoulders than Loki, closed/clear helmet, bronze/gold armor and a small crimson sash/plume/cloth accent. A shield and ceremonial staff/spear may support silhouette, but no combat logic is wanted. Use a shared material/shape family so one guard reads as trained palace personnel.

### Readable AI states

- Patrol: upright relaxed posture, steady two-point route.
- Investigate: turns and travels toward the last sound/sight.
- Suspicious: pauses, scans and holds attention at a location.
- Chase: forward lean and clear pursuit; red/amber awareness feedback.
- Return: gives up the lead and returns to patrol.

The concept sheet uses four states while the current code has five. The runtime behavior is authoritative until a focused review intentionally consolidates state labels. Communicate state through body turn, speed and one HUD cue rather than multiple competing indicators.

## Animation and interactions

Keep movements short and interpretable at a fixed, orbiting third-person distance. Guard reactions should show what caused the response: gaze toward decoy/sound, stop/turn, walk to investigate, then resume. Loki's disguise should be judged by plausible behavior as well as appearance; unnecessary sprinting can raise suspicion. No need for facial capture, dialogue trees, ragdolls or complex physics for the jam slice.

## Guest and finale

Deadpool is a single brief cameo in the finale only, never a companion and never part of the playable guard logic. The red/black masked silhouette can be abstracted with original simple forms if an approved model is absent. The user requested recognizability; the World Bible advises original placeholder art and no ripped film models. Keep the reference to palette/silhouette and verify permitted use before public distribution. Aghosh and Rizwan are finale characters only if the team approves the use of their likeness references; they should not be made background NPCs.

## Consistency checks

- Player reads as dark green/gold against pale stone; guard reads bronze/crimson against both.
- Horns do not intersect arches/camera frame in ordinary play.
- Cape is visible from the follow camera and does not occlude the head or prompt.
- Hand/face/boot proportions remain human-like; avoid oversized primitive joints.
- Disguise can be distinguished from Loki by silhouette without relying on HUD text.
- Magic green stays visually separate from the environment and guard alerts.
