# Loki the Trickster — Lokabrenna

A local, playable Three.js version of the Lokabrenna courtyard concept. The visual direction is an animated storybook: warm gold and cream, deep green, chunky silhouettes, cel shading, comic captions and deliberately playful character proportions. It is original 3D geometry inspired by the team's reference boards; the boards themselves are not runtime art.

## Run it

1. Install Node.js 20 or later.
2. Open a terminal in this folder.
3. Run `npm install` once, then `npm run dev`.
4. Open `http://127.0.0.1:5173` in a modern desktop browser.

The Three.js package is bundled locally by Vite. The running game does not fetch models, textures, fonts or scripts from the internet.

## Controls

- **WASD / arrows:** move relative to the camera; **Shift:** sprint.
- **Right mouse drag:** orbit; **mouse wheel:** zoom.
- **Q:** place an emerald illusion where the cursor points; **E:** misdirect a sound.
- **T:** talk to a nearby character; your bluff, compliment or joke changes what they do next. **F:** borrow the guard's appearance; **Space:** ring a nearby bell or shove a crate.
- **M:** expand or collapse the district map; **P / Esc:** pause; **R:** restart.

Watch the patrol, then get through the Bifrost or slip out via the west gallery passage. An illusion, sound, moving cover, or a calm disguise can change the guard's behavior. There is no combat.

## District map data

Edit `public/data/asgard-district.json`. `bounds`, player start, gate, crisis trigger, guard patrol, named landmarks, route polylines, interaction props and escape markers are stored in world-plane `x`/`z` coordinates (metres). The scene lays route paths and the on-screen map from this file. Keep the west passage aligned to its `escape` coordinate if moving the map.

## Scope and asset rights

The playable slice is one connected courtyard trouble with an opening menu, a live map, one systemic guard, a small prop set, an illusion, misdirection, disguise, and a short comic montage/portal ending. Character and environment forms are procedural, original placeholders. No MCU models, screenshots, logos, voices or music are included. Existing reference boards and the original Unity resources remain in the parent project for art guidance; this browser version does not modify the Unity scenes or assets.

Three.js is MIT licensed. The exact package license is included under `node_modules/three/LICENSE` after installation; retain it with redistributed builds.
