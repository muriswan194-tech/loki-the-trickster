# Project overview

## Working identity

The project folder and playable prototype are named **Lokabrenna**. The reference boards use the presentation label **The Golden Realm: Trickster**. Treat that phrase as the mood-board/visual-document title until the team chooses one final public title. The intended game is a short third-person 3D stealth-comedy experience in a compact, explorable Asgard district.

The player is Loki, an elegant trickster in dark green and gold. A guard is searching for Odin's missing golden apple and has closed the Bifrost approach. The player explores, reads the patrol, uses magic and ordinary objects, and finds a way through. There is no combat. Escape leads to a brisk comic montage and a brief live portal interruption with Deadpool, followed by the ending card. The montage and cameo are story beats, not extra missions.

## Player experience

The desired loop is: arrive in a place worth looking around; notice the guard and the space; try an idea; see the world and guard react; recover from a mistake with a joke; and escape. The game should reward experimentation without announcing a fixed puzzle recipe. The player should understand the camera, interaction range, guard awareness, and escape objective at a glance.

## Jam scope

- One connected courtyard district and one complete playable trouble.
- One Loki player character and one primary guard patrol.
- One flexible illusion/decoy, a few systemic props and environmental distractions.
- Multiple viable ways to create an opening; no combat and no chain of playable missions.
- A short comic montage and a separate, short 3D portal finale.
- A small asset set with consistent scale, palette, collision and clear reuse rights.

## Current Unity project snapshot

- Editor version: Unity **2022.3.62f1**.
- Render setup: Built-in Render Pipeline; Standard shader and built-in UI are used.
- Main scene: `Assets/Scenes/Lokabrenna.unity`.
- Finale scene: `Assets/Scenes/LokabrennaEpilogue.unity`.
- The main scene builds most of its environment and characters at runtime from `LokabrennaGame.cs`; the district and interaction data are JSON. Reusable custom meshes are generated/saved by the editor art builder. This is a working prototype architecture, not a finalized prefab/rig pipeline.
- Existing local third-party art includes Poly Haven Marble 01 and Gothic Statue (CC0), and ambientCG Tiles 131 (CC0 1.0); see `Assets/Resources/Art/THIRD_PARTY_ASSETS.md`.

## Content and rights boundary

The reference material evokes Loki and Asgard, but concept images are not production models, textures, animations, or proof of rights. Build original game geometry and costume details. Do not reproduce film stills, Marvel logos, exact MCU costume panels, or third-party Pinterest/Marvel Rivals/Asgard's Wrath imagery. The Deadpool cameo is a requested story beat; its implementation should remain minimal and be reviewed against the jam's IP rules. Keep generated concept boards as guidance unless the generation tool's terms and source files explicitly support their intended use.

## Success criteria

The first-time player reaches the courtyard from the main menu, can explore and read the guard, gets clear feedback when an ability or prop affects the world, can escape through more than one approach, and sees the montage and portal finale without a broken transition. Artistic consistency and a complete loop matter more than expanding the district or adding another power.
