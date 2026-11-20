# WORLD_BIBLE.md
## LOKI — ASGARD'S MOST WANTED

**Purpose:** Codex-facing visual and implementation brief for a compact Unity game-jam experience.  
**Project scope:** One explorable Asgard courtyard/district; one player character; guards with suspicion and chase behaviour; a small set of environmental interactions and Loki-style mischief powers; a short Deadpool portal cameo if time permits.  
**Team:** Two people. Prioritize clarity, reuse, and fast iteration. This is not a full open-world game.

---

## 1. Creative north star

Create an original, stylized fantasy playground that communicates **Asgardian grandeur, Loki's mischievous elegance, and readable stealth/chase comedy** in a small playable space.

The world should feel:
- Monumental at the skyline level, but human-scale and navigable at player level.
- Rich in gold, pale stone, warm sunlight, and bold architectural silhouettes.
- Designed with a consistent visual grammar: materials, shapes, lighting, character silhouettes, VFX and UI all feel like they belong to the same place.
- Playable first. The player should quickly understand where they can go, what they can interact with, and when guards have noticed them.

**Do not** build a huge world, a combat game, a lore encyclopedia, or a cinematic recreation. The goal is a compact sandbox with emergent mischief.

## 2. Continuity and source hierarchy

Use **MCU continuity as the main visual reference track** because the game brief draws on MCU Loki and Asgard. Keep Earth-616 comics as a separate inspiration track.

Source hierarchy:
1. Official Marvel/Disney material and published art books.
2. Production designers, costume designers, prop makers and VFX studios.
3. Reputable industry interviews and VFX publications.
4. Comics indexes and fan wikis for discovery only; verify important facts elsewhere.
5. Unverified social posts and Pinterest boards are mood-board leads, not factual evidence.

Never combine these as if they are the same system:
- Loki's illusions and other Loki magic.
- Loki's time-slipping/time doors in the Disney+ series.
- Asgard's Bifrost.
- Doctor Strange's Sling Ring portals.
- The portal visual used in Deadpool & Wolverine.
- The original portal effect designed for this game.

If a source describes TVA or another MCU setting, use it for production craft only unless the source explicitly addresses Asgard.

## 3. Visual identity

### 3.1 Provisional palette

These are **game art-direction starting values**, not canonical sampled MCU colours. Tune them against the actual Unity scene and maintain contrast/readability.

| Role | Starting colour | Use |
|---|---|---|
| Sunlight | `#FFD27D` | Warm key light, sunlit edges |
| Asgard gold | `#E5B84B` | Architectural trim, metal accents |
| Warm stone | `#EDE3D2` | Main walls, paving, columns |
| Loki green | `#0E4D2D` | Costume base and identity |
| Guard bronze | `#B8860B` | Armor and metal |
| Guard crimson | `#6E1414` | Small rank/alert accents |
| Illusion green | `#00FF66` | Gameplay magic cue; reduce saturation if it blooms too hard |
| Deadpool red | `#9E1316` | Cameo costume block |
| Portal orange | `#FF8C00` | Proposed game portal accent, not a claim about MCU portal canon |

**Colour discipline:** gold is not the same as yellow; reserve the brightest green for interactive magic; reserve crimson for guards; keep the environment warm and comparatively low-saturation so the player and VFX remain readable.

### 3.2 Shape language

- Asgard: tall verticals, broad stairs, arches, monumental columns, layered rooflines, repeating geometric ornament and controlled symmetry.
- Loki: tapered, elegant, slightly angular shapes; long vertical silhouette; cape as a readable moving shape; asymmetry can add mischief.
- Guards: heavier, stable, broad shoulder/helmet shapes; consistent armor language; simple silhouettes readable at distance.
- Magic: clean arcs, rings, afterimages and directional streaks; avoid particle noise that obscures the action.
- Deadpool cameo: strong red/black contrast and a deliberately abrupt visual interruption.

Use broad Norse/Art-Deco-inspired geometry as a starting point, but invent original patterns. Do not trace or reproduce specific Marvel concept-art shapes.

## 4. Environment — Asgard courtyard

### 4.1 Composition

Build a small district that implies a much larger city:
- One dominant landmark visible from most of the courtyard.
- A primary courtyard floor with clear movement space.
- Two or three secondary routes: stairs, raised walkways, side passages or platforms.
- A few human-scale structures/doorways to make the place feel inhabited.
- A readable perimeter and a small number of intentional escape routes.
- Distant skyline/mountain silhouettes to imply scale without building a full city.

Production research supports this approach: the *Thor: Ragnarok* team combined monumental Asgardian architecture with ordinary one-, two- and three-storey buildings to make the world feel more lived-in. Framestore's case study describes a vast CG Asgard and Norwegian Lofoten mountain references. For this jam, compress the idea into a handful of strong silhouettes.

### 4.2 Materials

Use a small material set:
- Warm pale stone for large surfaces.
- Aged gold/bronze for trim, railings, statues and architectural details.
- Darker stone for recesses and grounding.
- A restrained fabric/cloth material for banners.
- Selective emissive material for magic and interactive props.

Avoid making every surface metallic or glossy. Use roughness variation, bevels, edge wear and shadowed recesses to create depth.

### 4.3 Environmental storytelling and interaction

The courtyard should be a toybox:
- Movable or triggerable props that can distract guards.
- Objects that can be knocked over, activated or used as temporary cover.
- At least one elevated route and one ground-level route.
- Clear hiding/escape opportunities, but no single mandatory solution.
- Props must be visually distinct from background decoration.

Use colour, silhouette and simple animation to communicate interactability. Avoid adding systems that cannot be tested within the jam.

## 5. Loki — character design

### 5.1 Design priorities

- Elegant, agile silhouette with a dark cape or cape-like shape.
- Green and gold as the main identity colours; black/dark neutral for contrast.
- A strong head/shoulder silhouette that reads from the gameplay camera.
- Costume complexity reduced to a few bold layers and accents.
- Animation should sell confidence, theatricality, quick reactions and mischievous movement.

Costume designer Christine Wada's interview describes Loki's early layered armor, horns and cape as reflecting his power-seeking persona, with later simplified clothing reflecting character development. The game needs the recognizable visual idea, not a film-accurate costume.

### 5.2 Gameplay readability

- Idle: calm, poised silhouette.
- Sneak/mischief: lower center of gravity and quick head/hand gestures.
- Magic: brief anticipation pose, visible hand cue, one clear effect, fast recovery.
- Detected: keep Loki's silhouette visible; do not hide the character in a full-screen effect.

## 6. Asgardian guards

### 6.1 Design

- Unified bronze/gold armor family.
- Crimson as a small rank, sash, plume or alert accent.
- Distinct helmet and shoulder silhouette.
- Simple shield/spear or staff shapes if needed; avoid adding combat complexity.
- Use restrained metal wear and layered armor plates.

Wētā Workshop's *Thor: Ragnarok* case study documents stylized armor, molded components, bold comic-book colour and prop manufacture. It discusses Grandmaster Guard costumes and Skurge's armor; do not mislabel those as the Asgardian Einherjar. Use the page for manufacturing/material principles, then create original guard designs.

### 6.2 Suspicion and chase language

The player must understand guard state without reading text:
1. **Patrol:** relaxed stance, steady path, neutral indicator.
2. **Noticed:** guard turns/pauses; short alert cue.
3. **Suspicious:** looks toward last known position; indicator fills or changes.
4. **Chase:** clear pursuit pose and stronger alert indicator.
5. **Search/lost:** guard checks nearby space, then returns to patrol.

Keep one consistent icon/colour language. Avoid confusing magic-green with guard-alert-red.

## 7. Loki magic and illusions

The game should use a deliberately small VFX vocabulary.

### 7.1 Illusion

Purpose: misdirect guards, create a decoy, or briefly distract.
- Emerald accent.
- Quick hand/gesture cue.
- A clear silhouette or afterimage.
- Short lifetime and readable disappearance.
- The decoy must be visually distinct from the real player.

### 7.2 Teleport/dash (only if already supported by gameplay)

Do not assume MCU time-slipping is the same as ordinary Loki teleportation. If the game needs a fast escape move, invent a simple original movement effect:
- Short green streak or afterimage.
- A brief start/end cue.
- No long portal tunnel or elaborate timeline effect.

### 7.3 VFX principles

VFX Supervisor Christopher Townsend's *Loki* Season 2 discussion emphasizes that effects should work within the established visual language of set, camera and lighting. It also describes separate development for Loki magic and time-door effects. Apply that principle: effects should feel embedded in the world, not pasted on top.

Implementation budget:
- One reusable shader/material family.
- One particle prefab per power at most.
- A small number of mesh/ring/arc elements.
- Short durations and controlled bloom.
- Test effects in daylight and shadow, at actual gameplay camera distance.

## 8. Portals and the Deadpool cameo

Treat the cameo as an optional end beat, not a core dependency.

### 8.1 Keep the systems separate

- **Bifrost:** Asgardian realm-travel system; do not treat it as a generic circular portal.
- **Loki illusion:** deception/appearance manipulation; not automatically a travel gate.
- **Deadpool portal:** reference the specific film moment only after confirming the shot/source. Do not infer its colour or mechanics from unrelated MCU portals.
- **Game portal:** original effect for this project. A warm orange-gold ring is a proposed design choice, not a factual MCU specification.

### 8.2 Cameo staging

- Keep the beat short and readable.
- Portal appears first; player/guards react; Deadpool enters with a strong red/black silhouette.
- Use a comic-panel or hard-cut transition only if it is feasible and does not delay the core playable build.
- The fourth-wall/TinkerSpace ending remains optional and should be decided after the courtyard loop works.

Framestore's *Deadpool & Wolverine* case study is useful for previsualisation, staging, timing and the coordination of complex sequences. It does not establish that Framestore created every portal effect in the film.

## 9. Camera, lighting and presentation

- Use a camera angle that keeps Loki, nearby guards and escape routes visible.
- Avoid excessive depth-of-field or cinematic shake that harms play.
- Warm directional sunlight; strong but soft-edged shadows.
- Use architectural shadow to create depth and small hiding opportunities.
- Reserve high-contrast emissive effects for gameplay cues.
- Keep the main courtyard bright enough to read, with darker recesses for visual variation.
- Use atmospheric perspective for distant buildings/mountains, not a huge detailed city.

## 10. Unity implementation priorities

Build in this order; do not start with polish.

### P0 — playable visual foundation
1. Confirm existing scene and project structure; do not rebuild the project.
2. Establish camera, player scale and movement.
3. Block out courtyard, landmark, perimeter and two escape routes.
4. Set a simple lighting/palette pass.
5. Add one guard with patrol, suspicion and chase states.
6. Add one illusion/decoy interaction and one environmental distraction.
7. Verify that the loop is fun and readable.

### P1 — identity and polish
1. Replace blockout shapes with original stylized architecture.
2. Refine Loki and guard silhouettes/materials.
3. Add clear suspicion UI/audio cues.
4. Add controlled magic VFX and small environmental reactions.
5. Add a few prop interactions and ambient motion.

### P2 — optional ending
1. Add the original portal effect.
2. Stage the Deadpool cameo using an original or appropriately licensed placeholder.
3. Add comic-panel transition and fourth-wall ending only if core scope is stable.

**Do not** introduce combat, inventories, dialogue trees, procedural cities, complex AI navigation, or a full portal-travel system unless the current build already contains them and they are essential.

## 11. Asset and copyright rules

- Marvel/Disney characters, costume designs, film stills, concept art, logos, models, textures and dialogue are protected material. A publicly accessible image is not a game asset licence.
- Research references are **reference only** unless an explicit licence/permission says otherwise.
- Do not use ripped MCU models, extracted game assets, unauthorized art-book scans or film screenshots as shipped game content.
- Create original equivalents: new armor shapes, original motifs, original textures, original portal/VFX design and original dialogue.
- For third-party assets, record the exact asset page, creator, licence, attribution requirement, download source and intended use.
- CC0 or marketplace assets must still be checked at the individual asset/package level. Do not assume every model on a marketplace has the same licence.
- Keep an asset register with these statuses: `Reference only`, `CC0 verified`, `Licensed - verify terms`, `Unknown - do not use`, `Original asset to create`.

## 12. Provisional palette tokens for Unity

Create centralized material/color tokens rather than hard-coding values in many prefabs:
- `Asgard_Sunlight` = `#FFD27D`
- `Asgard_Gold` = `#E5B84B`
- `Asgard_Stone` = `#EDE3D2`
- `Loki_Green` = `#0E4D2D`
- `Guard_Bronze` = `#B8860B`
- `Guard_Crimson` = `#6E1414`
- `Magic_Emerald` = `#00FF66`
- `Deadpool_Red` = `#9E1316`
- `Portal_Orange` = `#FF8C00`

These are initial values for iteration. Adjust for lighting, material response, accessibility and contrast.

## 13. Reference index

### Core Asgard and production design
- [Marvel — Jack Kirby's Design Influence in Thor: Ragnarok](https://www.marvel.com/articles/movies/jack-kirby-s-design-influence-is-a-force-in-thor-ragnarok)
- [Marvel — Thor: The Art of the Movie (official preview)](https://www.marvel.com/articles/culture-lifestyle/marvel-studios-infinity-saga-thor-art-of-the-movie-exclusive-preview)
- [Framestore — Thor: Ragnarok VFX case study](https://www.framestore.com/work/thor-ragnarok)
- [QAGOMA — Behind the scenes of Thor: Ragnarok set design](https://www.qagoma.qld.gov.au/stories/go-behind-scenes-of-film-thor-ragnarok-by-marvel)
- [Wētā Workshop — Thor: Ragnarok armor and props](https://www.wetaworkshop.com/projects/thor-ragnarok)

### Loki costume and magic
- [Costume Designers Guild — Loki's Fashion Trajectory](https://costumedesignersguild.com/press_news/lokis-fashion-trajectory/)
- [Marvel — Loki VFX with Industrial Light & Magic](https://www.marvel.com/articles/tv-shows/loki-industrial-light-magic-visual-effects)
- [VFX Voice — How Time Keeps Slipping Away in Loki Season 2](https://vfxvoice.com/how-time-keeps-slipping-away-in-loki-season-2/)
- [ILM — Behind the Magic: Visual Effects of Loki](https://www.youtube.com/watch?v=9kXi5P8krGA)
- [The Third Floor — Loki 'Race to the Ark' visualisation](https://www.youtube.com/watch?v=pLJkVGaL_BU)

### Deadpool and cameo staging
- [Framestore — Deadpool & Wolverine VFX and previsualisation](https://www.framestore.com/work/deadpool-wolverine)
- [Framestore — Deadpool & Wolverine pre-production](https://www.framestore.com/work/deadpool-wolverine-pre-production)

### Comics and asset sources
- [Marvel — Loki character/comics index](https://www.marvel.com/characters/loki) — keep Earth-616 material separate from MCU decisions.
- [Poly Haven](https://polyhaven.com/) — generic CC0 material/environment source; verify individual asset record.
- [Kenney](https://kenney.nl/assets) — generic game assets; check pack licence.
- [Sketchfab](https://sketchfab.com/) — per-model licence varies; inspect before use.

## 14. Research gaps to resolve before final art lock

- Identify exact MCU Asgardian guard/Einh erjar references separately from Grandmaster Guard and Skurge.
- Collect direct reference frames for Loki's specific illusion tricks and note the scene/episode/film for each.
- Verify the exact Deadpool & Wolverine portal shot and its design; do not assume orange is canonical.
- Decide whether the final cameo is essential or only a stretch goal.
- Review the current Unity screenshots and choose a small set of visual corrections that fit the remaining jam time.

## 15. Codex operating instructions

When implementing from this document:
1. Inspect the existing Unity project, current scene, prefabs, scripts, packages and screenshots first.
2. Report the current state and the smallest proposed changes before any broad refactor.
3. Preserve existing working gameplay and architecture. Do not rebuild the project.
4. Implement in small, testable increments. Prefer one scene, reusable materials, simple prefabs and lightweight state logic.
5. Use original or explicitly licensed assets only. If no approved asset exists, create a simple original placeholder.
6. Compare each iteration against the design goals above at actual gameplay camera distance.
7. Do not invent additional mechanics or expand scope without approval.

**Definition of done:** the courtyard is navigable; Loki reads clearly; at least one guard visibly transitions from patrol to suspicion to chase; one mischief interaction creates a readable reaction; the environment has a coherent Asgard-inspired visual identity; and the build remains stable.