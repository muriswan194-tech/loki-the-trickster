# Visual reference analysis

## Inventory method and discrepancy

Source folder: the user-supplied local `visual assets` folder. Images were opened visually; filenames alone were not used to classify them. The folder contains **33 image files and 27 unique pixel hashes**: two Gemini boards, 21 unique OpenAI Playground boards, three images cached with the saved ChatGPT page, and one 120×120 profile/avatar icon. Six files are redundant copies: one extra copy of `01.48.30`, two of `01.48.47`, one of `01.49.38`, and two of `01.56.02`.

There is no `01`–`22` filename sequence. The 23 game concept images below are indexed here in a practical order: two Gemini boards first, then the OpenAI boards by timestamp. That yields one more unique generated game image than the pasted task anticipates. Image 23 is retained as an extra reference, not silently omitted. Image 1–2 dimensions are 2752×1536; the 21 OpenAI references range from 1024×1536 to 1672×941. Cached web images are separate and are not part of the generated game-concept sequence. Fine lettering inside AI-generated boards is not authoritative; use the visible composition and palette as evidence, not tiny or garbled captions as technical requirements.

## Sequential visual analysis

### 01 — Gemini art-direction and character/environment board

File: `Gemini_Generated_Image_ges0zjges0zjges0.png` (2752×1536). A framed pitch sheet combines a green-and-gold trickster, bronze/red guard, palace courtyard, props, palette, lighting samples and emerald magic. It establishes the strongest overall identity: warm gold/cream architecture against deep forest green, with green magic as the high-contrast cue. Its logo/title and small illustrative text are graphic mock-up only. Dependency: this is the broad palette and silhouette key for images 02, 05, 06, 07, 08 and 15–18.

### 02 — Gemini palace arcade concept

File: `Gemini_Generated_Image_rc12t0rc12t0rc12.png` (2752×1536). A warm, empty stone arcade uses repeated round arches, layered colonnades, an open central axis and a distant spire. This is a useful spatial mood: the player should see a destination through connected architecture. It is a single illustration, not a level plan or modular-model specification. Use its depth, rhythm and warm/cool separation as a 3D composition reference.

### 03 — Courtyard hero environment

File: `OpenAI Playground 2026-11-21 at 01.44.16.png` (1672×941). A broad sunlit courtyard frames a fountain, elevated bridge, stairs, planted borders, banners and a palace landmark; small guards and props suggest activity. The image communicates vertical layers and comfortable sightlines. Gameplay implication: retain a clean central traversal route and put hiding/interaction props at the edges. It conflicts with a tiny enclosed courtyard if copied literally, so compress the architecture to the current district footprint.

### 04 — Courtyard layout / guard and escape sketch

File: `OpenAI Playground 2026-11-21 at 01.45.41.png` (1672×941). Top-down plan marks a central court, loop patrol, perimeter, hiding spots, interactive props, bell distraction and a side exit. This is a readable layout concept, not measured Unity coordinates. It offers a second physical escape in addition to the main gate; the current game code only checks the Bifrost gate, so that route is not yet represented in runtime logic. See open risks.

### 05 — Loki character turnaround

File: `OpenAI Playground 2026-11-21 at 01.46.40.png` (1672×941). A slim figure in long dark-green coat/cape, gold trim and a tall horned silhouette is shown from multiple angles with portrait expressions. The back silhouette and high collar are as important as front details because play is third-person. Treat proportions and color blocking as art direction. The board is illustrative, not a skeletal rig or an instruction to reproduce an MCU costume.

### 06 — Palace guard concept sheet

File: `OpenAI Playground 2026-11-21 at 01.47.37.png` (1672×941). Turnaround views and poses use bronze/gold armor, crimson cloth, a closed helmet and shield/spear shapes. Patrol, suspicion and chase are separated by posture. Implementation implication: a few clear poses/material cues can communicate state without elaborate animation or combat. The armory is visual grammar, not permission to add combat.

### 07 — Interactive prop catalog

File: `OpenAI Playground 2026-11-21 at 01.48.03.png` (1672×941). Ten prop ideas include a stone urn, gold vase, wooden crate, crimson banner, bell, planter, movable bench/object, pedestal, fountain and a small distraction object. The functional annotations propose push, carry, climb, hide and ring. These are idea prompts; choose only a few that the current systemic rules can support. Avoid treating every caption as an approved mechanic.

### 08 — Illusion magic sequence

File: `OpenAI Playground 2026-11-21 at 01.48.30.png` (1672×941; two duplicate files). Six panels show an emerald hand cue, spreading magic, a duplicate, guard attention, the illusion dissipating and a magical distortion. Strongest transferable points: localized green cue, readable duplicate, brief duration and an understandable guard reaction. The teleport-like panel is a separate idea and must not be conflated with the core illusion. The existing project currently includes several powers; the jam brief still prioritizes one flexible illusion and systemic interactions.

### 09 — Gameplay-loop storyboard

File: `OpenAI Playground 2026-11-21 at 01.48.47.png` (1672×941; three identical files). Six comic-style scenes move from entering the court, observing a guard, waiting, placing an illusion, guard investigation and slipping toward escape. The bar summarizes Observe → Create → Distract → Sneak → Escape. Use this as pacing and readability guidance, not as a locked chain; player choices should be open-ended.

### 10 — Guard AI state diagram

File: `OpenAI Playground 2026-11-21 at 01.49.07.png` (1672×941). Four states are presented: Patrol, Suspicion, Chase and Search, with trigger/transition notes. It offers a compact conceptual state machine. The current implementation uses Patrol, Investigate, Suspicious, Chase and Return; preserve existing behavior unless a focused gameplay review establishes a better state mapping. The image is a state sketch, not a tuned AI spec.

### 11 — Improvised escape comic

File: `OpenAI Playground 2026-11-21 at 01.49.38.png` (1672×941; one duplicate). A multi-panel stealth-comedy vignette shows a distraction, surprised guards, a chase-like mishap and a sly reaction. It demonstrates expressive acting, panel variety and a playful consequence. The scene contains implied physical chaos; the actual game should keep non-combat slapstick readable and harmless.

### 12 — HUD / awareness UI board

File: `OpenAI Playground 2026-11-21 at 01.49.58.png` (1672×941). Dark gold-framed HUD mock-up shows an objective, awareness indicator, interaction key, illusion icon, cooldown and a pause/menu cue. Three visual states compare unaware, suspicious and chasing. Use its hierarchy and icon consistency. Do not copy its mission text (“Oracle's Ledger”), cooldown values, logo, or all-at-once HUD density; the shipped objective is the apple/Bifrost trouble.

### 13 — Comic montage page

File: `OpenAI Playground 2026-11-21 at 01.50.18.png` (1672×941). A six-panel page with diagonally varied views, bold onomatopoeia, reaction faces and a final Loki smirk. This supports the requested fast montage after the single playable trouble. It is a static narrative sequence; do not build each depicted incident as a playable area. Generated dialogue and character rendering need editorial review.

### 14 — Portal interruption / finale image

File: `OpenAI Playground 2026-11-21 at 01.50.31.png` (1672×941). Loki reacts in a gold hall as a cool violet/blue circular portal appears; a silhouetted visitor and comic speech turn the arrival into a surprise. Its abrupt cool-color contrast is effective. It is not a portal shader spec and does not establish Marvel portal canon. Keep the live finale short, original in staging, and separate from gameplay.

### 15 — Courtyard lighting comparison

File: `OpenAI Playground 2026-11-21 at 01.55.45.png` (1536×1024). Several views compare warm daylight, golden-hour and darker dramatic lighting for the same courtyard. Warm key light and restrained cool shadow preserve gold-stone readability; the dark version is atmospheric but could hide stealth affordances. Build one gameplay lighting setup first; optional finale color variation can be a later pass.

### 16 — Modular architecture kit

File: `OpenAI Playground 2026-11-21 at 01.56.02.png` (1149×1369; two duplicate files). A vertical catalog depicts wall/arch modules, columns, stairs, raised platform, balcony rail and floor pieces. It suggests a reusable kit rather than bespoke one-off buildings. No grid size, pivot convention, collider profile or UV scale is specified; those must be set in Unity before production.

### 17 — Prop placement and breakdown

File: `OpenAI Playground 2026-11-21 at 01.56.19.png` (1536×1024). An isometric court places a central fountain, steps, side bell, vase, urn, crates, banner, planter, pedestal and bench; the side panel breaks props out. Emerald/gold outlines imply interactable vs decorative. Good for visual zoning, but its elevated, walled layout differs from the continuous walkable district in `asgard-district.json`.

### 18 — Material and texture palette

File: `OpenAI Playground 2026-11-21 at 01.56.33.png` (1122×1402). A vertical swatch board shows cream stone, carved gold/brass, dark leather/wood, green cloth, crimson cloth and surface examples. It helps define roughness/metal variation and accent restraint. The board itself is not tileable PBR data; use the already licensed local maps or author proper seamless textures.

### 19 — Top-down grid map

File: `OpenAI Playground 2026-11-21 at 01.56.49.png` (1536×1024). A more regular, tile-based square has a central fountain, four interior planters, gates, an enclosing wall and labeled routes. It differs from image 04 and from the current long 31×22-unit connected district; treat both layouts as alternative blockouts, not cumulative geometry. Convert final decisions into JSON coordinates rather than tracing pixels.

### 20 — Character and guard animation board

File: `OpenAI Playground 2026-11-21 at 01.57.07.png` (1024×1536). A tall sprite/pose sheet compares Loki and guard actions across locomotion, stealth, gestures and reactions. It communicates confident idle, lowered sneak, quick directional motion and obvious guard tells. It is not a rig definition, frame timing sheet or production sprite atlas. For 3D, obtain/build a rig and retargetable clips or use restrained procedural motion.

### 21 — Third-person camera board

File: `OpenAI Playground 2026-11-21 at 01.57.27.png` (1536×1024). Four gameplay-framed camera situations, a top-down mini-map/diagram and guidance suggest an over-the-shoulder chase camera with orbit, collision and clear sightlines. Use the concept to review comfort, framing and occlusion. Do not rely on tiny rendered settings text as actual numeric tuning; use Unity Inspector measurements and play checks.

### 22 — Courtyard design/overview board

File: `OpenAI Playground 2026-11-21 at 01.57.41.png` (1536×1024). A presentation board combines a bird's-eye courtyard with fountain and side objects, entrance views and a compact breakdown of props, gameplay and characters. It reinforces the core visual language and a main landmark. It is a pitch composition, not a second map or a source for finished meshes.

### 23 — Environment and gameplay reference board (extra)

File: `OpenAI Playground 2026-11-21 at 01.57.58.png` (1149×1369). A vertical board combines a courtyard screenshot-style hero view with environment tiles, blockout, props and a Loki look. It is a useful cross-category summary. This is the extra generated image that makes the folder's game-art set 23 rather than the anticipated 22.

## Cached web images and non-game file

These are present under `Create Game Visuals_files/` and were identified by their saved-page alt text:

- **24 — Marvel Rivals map/article thumbnail**, `lOkYu…` JPEG, 1529×860. Copyrighted game imagery. At most a mood reference for large-scale city density; do not import, trace or reproduce.
- **25 — Asgard's Wrath 2 article thumbnail**, `hK_riK…` JPEG, 1200×675. Copyrighted game imagery. Architectural/mood comparison only; not a production asset.
- **26 — Pinterest “Pin on Alekzander Zagorulko” image**, `KAeN…` JPEG, 736×736. Artist/source rights are not established by the saved page. Do not copy into the game or redistribute; retain only as an unverified reference pending source/license identification.
- `ri.png`, 120×120, is a profile/avatar icon from the saved ChatGPT page, not a game visual. Exclude it from art direction.

## Cross-image consistency and conflicts

The strongest common language is pale/gold stone, repeated arches, deep green/cape-and-horn trickster, bronze/crimson guard and emerald magic. The boards vary between sunny illustrated fantasy, dark realism, and graphic comic-book rendering; use the sunny golden-hour courtyard as the normal-game target and reserve dark/inked contrast for montage panels. Layout boards disagree about scale, symmetry and escape points. The 4-state AI board differs from the current 5-state implementation. Prop sheets imply climbing, carrying and many object-specific mechanics that exceed the intended compact systemic sandbox. Treat image labels as suggestions; `WORLD_BIBLE.md`, this project's JSON, and explicit user gameplay direction control implementation.
