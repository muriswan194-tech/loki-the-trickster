# World-building bible

## North star

Make one believable, wanderable Asgard neighborhood that implies a much larger golden city. It should read as a connected place in real-time 3D, not as an illustration assembled from unrelated props. At player scale, routes and cover must be clear. At skyline scale, a palace/Bifrost landmark gives orientation. The visual boards are concept direction; the Unity world must be rebuilt as navigable 3D geometry.

## World identity

- **Primary forms:** tall verticals, broad archways, layered galleries, controlled symmetry, carved bands and tapered rooflines.
- **Secondary forms:** inhabited doors, market/recreation details, planters, banners, fountains, benches and service passages.
- **Composition:** keep a readable main square, visible landmark, two side promenades and a connected southern walk. Use open sightlines across the central area and pockets of cover at its edges.
- **Boundaries:** imply an extended city beyond the game with palace silhouettes, distant roofs and atmospheric depth; close the playable perimeter with architecture/terrain rather than an abrupt empty void.

## Palette and lighting

Use warm cream limestone and restrained aged gold/bronze for the majority of the environment. Deep green belongs to Loki; crimson is a small guard/rank accent; emerald is reserved for magic and interactable feedback. Dark recesses and cool teal/blue shadows provide depth. Keep gold distinct from flat yellow and avoid making every surface reflective.

The current Built-in pipeline supports a modest directional sun with soft shadows, ambient fill, fog/sky and Standard materials. Aim for warm low-angle daylight/golden hour with cooler shadow fill. Keep the playable courtyard brighter than montage/finale shots. Any color-grading or comic outline effect should be restrained and compatible with the project's current pipeline; do not introduce a custom render pipeline just to match a board.

## Materials and surface treatment

Use a coherent small material set: pale stone, dark stone/recesses, worn brass/bronze, painted or woven cloth, dark leather/wood and selective emissive emerald. Vary roughness and bevel/edge shape by material. Preserve readable values in shadow. The concept texture board is a swatch reference, not a seamless texture. Existing CC0 marble maps can support paving; existing CC0 statue can serve as a decorative landmark after recoloring. The Tiles131 texture is present but listed as unused.

## Landmark and navigation

The north Bifrost/gate approach is the immediate story destination. A palace spire, arch or layered skyline should remain visible from multiple district routes. The central fountain/square can be a secondary orienting landmark. Routes should connect without forcing the player through one scripted sequence: south start → square → west gallery/east orchard → north gate, with side alleys and cover connecting those paths.

The current data model uses Unity world-plane coordinates: x left/right, z down/up, y height. `(0,0)` is the district center. Existing bounds are x `-15.5..15.5`, z `-11.7..10`. Keep new visual assets aligned to this coordinate system unless a deliberate map revision is documented in JSON.

## Environmental storytelling

Use props to show that the district is inhabited: recently moved crates, a bell used by the watch, a missing-apple search notice, a market stand, clipped banners, gardens and distant guard routes. These details should strengthen the apple-lockdown premise without adding lore screens or quests. Interactive objects need silhouette/material cues that differ from background decoration; decorative duplicates should not falsely imply interactivity.

## 3D production translation

Build a small modular kit with a single chosen grid/pivot convention: wall/arch span, column/capital, stair/ramp, balcony/bridge segment, railing, gate, floor tile and trim. Set unit scale, snap points, LOD/collider rules and UV density before duplicating. Repeat modules with controlled variation in height, trim, banners and damage; avoid one-off shape drift. Create real colliders only for gameplay surfaces and cover. Distant palace detail can be a low-cost silhouette.

## Visual hierarchy in play

1. Loki and guard remain the most readable moving silhouettes.
2. The gate and escape landmarks remain visually distinct.
3. Interactive props use a consistent outline, light or icon cue.
4. Emerald magic gets the highest saturation and emissive contrast.
5. Decorative filigree remains subordinate to navigation and character contrast.

## Boundaries

Do not copy exact MCU set geometry, screen captures, logos, costumes or existing game art. The user supplied images are visual references only. Preserve original geometry and patterns, and keep third-party use within the documented CC0 assets unless an asset's license is separately reviewed. Avoid new packages, online runtime dependencies and a full open-world extension.
