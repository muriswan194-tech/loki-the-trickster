# Development roadmap

This roadmap keeps one complete playable trouble as the priority. It records the observed prototype state and the next actions; it does not authorize new mechanics during this documentation pass.

## Phase 1 — Existing project audit and environment setup

- **Status:** Audited for this documentation task.
- **Tasks:** Preserve project/version/pipeline; inspect main scenes, package manifest, data, scripts, art rights and reference count.
- **Dependencies:** None.
- **Deliverables:** This folder, `WORLD_BIBLE.md`, asset license notes and the project README.
- **Done when:** Team agrees on title, visual target, single encounter scope and rights boundary.

## Phase 2 — Courtyard construction and modular environment

- **Status:** Runtime-built district and original mesh kit exist; visual consistency/quality and physical escape layout need a focused review.
- **Tasks:** Compare actual play camera against the warm courtyard target; resolve central/square map vs connected promenade map; define grid, unit dimensions, collider/pivot/material rules; improve only the most visible architecture; clarify the missing second physical exit.
- **Dependencies:** Phase 1 decisions.
- **Deliverables:** One polished district blockout, landmark, cover, readable gate and modular kit specification.
- **Done when:** Player can walk the full connected footprint, camera framing stays clear, and all routes have intentional visual/collision boundaries.

## Phase 3 — Player controller and third-person camera

- **Status:** Movement, sprint, orbit/zoom follow camera are in source; current screenshots exposed a rough character silhouette and camera needs visual review.
- **Tasks:** Validate speed/turning and collision in Unity; inspect cape/horns/occlusion; tune follow distance, orbit range, pitch limits, wall collision and input prompts; do not add packages unless a real limitation requires them.
- **Dependencies:** Phase 2 walkable floor and collision.
- **Deliverables:** Stable movement and camera with no frequent clipping.
- **Done when:** Player remains visible during normal movement and camera orbit; interactions remain aimable.

## Phase 4 — Guard character and AI state machine

- **Status:** One procedural guard and Patrol/Investigate/Suspicious/Chase/Return logic exist in source; a concept sheet shows a different 4-state summary.
- **Tasks:** Keep one canonical state diagram; tune sight, hearing, suspicion decay and lost-lead behavior; ensure guard reacts toward the actual sound/sight source and communicates state.
- **Dependencies:** Phase 3 movement and a stable map.
- **Deliverables:** One guard patrol and deterministic readable reactions.
- **Done when:** Patrol, distraction, suspicion, chase and return can be observed in one short play session without confusion.

## Phase 5 — Interactive objects and illusion mechanic

- **Status:** Current code/data include illusion, sound, crate movement, bells, disguise and additional forms/discoveries; jam brief asks to prioritize one flexible illusion and a small shared-rule prop set.
- **Tasks:** Audit each control/prop for feedback; preserve common sight/hearing/solid/movable/noisy rules; remove or hide scope-expanding powers only by explicit product decision; validate world changes persist during the encounter.
- **Dependencies:** Phases 2–4.
- **Deliverables:** One illusion plus a handful of reliable world-reactive props.
- **Done when:** Several different player ideas produce understandable guard/world reactions without accepted-combination scripts.

## Phase 6 — Stealth and escape gameplay

- **Status:** Gate escape reacts to distraction/disguise/form; current gate-check path appears to be the single physical escape point.
- **Tasks:** Decide if alternate escape is a real physical route or a distinct systemic approach to the Bifrost; ensure guard behavior does not make success a lucky radius check; add no fixed route buttons.
- **Dependencies:** Phases 4–5.
- **Deliverables:** Complete apple-lockdown encounter and at least two viable player approaches.
- **Done when:** A new player can understand the objective, experiment, be caught humorously, recover and escape.

## Phase 7 — UI, audio and feedback

- **Status:** Main menu/HUD/prompts/toasts exist in code; the reference HUD suggests a compact awareness/interaction cue. Audio coverage was not confirmed in this audit.
- **Tasks:** Keep typography and controls readable at target resolution; make interaction failure feedback explicit; add only short original cues for bell, magic, alert and success if audio is available.
- **Dependencies:** Core gameplay is stable.
- **Deliverables:** Clean menu, controls, awareness and actionable feedback.
- **Done when:** Every key action has an understandable visual response and the HUD does not block the player/world.

## Phase 8 — Comic montage and portal finale

- **Status:** Epilogue source and scene exist; comic panels transition into a live Deadpool interruption.
- **Tasks:** Confirm timing, camera, character visibility, text legibility and non-gory tone; keep it short and separate from the escape simulation; verify scene return/final end.
- **Dependencies:** Phase 6 success transition.
- **Deliverables:** Montage + live cameo + final card.
- **Done when:** The narrative beat reads once, without becoming a second mission or pausing in a broken scene.

## Phase 9 — Optimization, testing and final polish

- **Status:** Not verified for the latest character edit in the actual Editor.
- **Tasks:** Compile and run in Unity 2022.3; test the main-menu-to-playable-to-finale flow; try illusion, sound, prop movement, disguise and cover; tune lighting/materials at runtime; note remaining limits honestly.
- **Dependencies:** All previous phases.
- **Deliverables:** A tested buildable scene, controls/readme, rights notes and final capture.
- **Done when:** No compile errors, no broken transitions, reasonable performance on target machine and repeatable escape outcomes.

## Priority rule

If the jam timeline is tight, reduce the number of props and architectural variants. Do not remove readable guard reactions, player agency, camera control, clear feedback, or the one complete story flow.
