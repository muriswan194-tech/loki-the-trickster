# Loki the Trickster — Lokabrenna

A compact Unity game-jam prototype with a walkable 3D Asgard district, systemic guard behavior, and a short comic-book finale. Its palette combines original Unity meshes with locally stored CC0 stone PBR maps and a CC0 3D courtyard statue. No runtime network access is required.

## Browser prototype

A Three.js companion prototype is in `browser-prototype/`. To run it, install Node.js 20+, open a terminal in that folder, then run `npm install` and `npm run dev`. Open the local URL printed by Vite (normally `http://127.0.0.1:5173/`). It includes orbit camera controls, the courtyard escape, the Bifrost transition, and the wanted-poster end card.

## Open and play

1. Install/open Unity Hub with Unity **2022.3 LTS** (the project version is **2022.3.62f1**).
2. In Unity Hub, choose **Add project from disk** and select this `Lokabrenna` folder.
3. Open `Assets/Scenes/Lokabrenna.unity` and press **Play**. Choose New Game from the main menu and wander the connected courtyard, west gallery, east orchard and south garden walk. The single apple-search trouble begins at the Bifrost approach. Escaping leads to a rapid comic montage, a short live 3D Deadpool interruption and a final end card.

## Controls

- **WASD / arrow keys:** move; **Shift:** sprint; **hold right mouse and drag:** rotate the third-person camera; **wheel:** zoom; **mouse cursor:** aim at a world point.
- **Space:** interact with nearby props, push crates, ring the bell, or toss a conjured apple.
- **Q:** place a visible illusion at the cursor; guards who see it investigate.
- **E:** send a sound toward the cursor; **E then S:** copy the nearby guard's appearance; **F:** quick disguise fallback.
- **S then 1–5:** human, hawk, falcon, ant, wildfire. The HUD toolkit has a Change Form button if key timing conflicts with movement. Falcon is quick; ant and wildfire are hard to recognize; wildfire can interact with the hearth.
- **T then 1 / 2 / 3:** lighten, restore, or make the aimed movable prop heavy. Three toolkit buttons provide the same settings without chords.
- **C:** conjure a solid apple; approach it and use Space to toss it and make a sound.
- **G:** use Odin's sceptre after finding it. Click **TELEPORT** in the toolkit to use a Tesseract charge.
- **P / Esc:** pause; **R:** restart the district.

The guard patrols, sees within a forward cone, hears sounds, investigates distractions, builds suspicion, and chases exposed Loki. Solid movable crates can obstruct sight. The gate is guarded, but the game does not mandate a named combination. Getting caught resets quickly with a joke; there is no combat.

Odin's sceptre and Frigga's Tesseract are optional hidden discoveries without quest markers. The Tesseract starts with ten charges and recovers one every 75 seconds. The ending is a scripted, unskippable sequence: Deadpool names Aghosh and Rizwan as the creators at TinkerSpace in Kochi; the comic confrontation is non-gory slapstick; Loki escapes back into Asgard; then the cover closes with **THE RISE OF THE DEAD / LED BY AGHOSH AND RIZWAN**.

## Editable district data

Edit `Assets/Data/asgard-district.json`; Unity loads its synchronized copy at `Assets/Resources/asgard-district.json`. Coordinates use the ground plane: **x** is left/right, **z** is down/up, and `(0,0)` is the courtyard centre. Distances are Unity units.

- `bounds`: player movement limits for the expanded connected district.
- `playerStart`: Loki's starting x/z position.
- `landmarks`: stable scene anchors for the square, fountain, alley, market, and Bifrost.
- `guard.points`: patrol x/z waypoints; `speed` is units per second.
- `triggers`: stable id/type and activation x/z/radius.
- `interactions`: stable id, type, label, x/z, interaction `radius`; supported types include `bell`, `prop`, `illusion`, `crate`, `hidden`, and `flame`. Optional properties are `noisy`, `movable`, `solid`, `hearing` (sound radius), and `distraction` (extra sound duration). The shipped hidden IDs identify the sceptre and Tesseract. Add an item by appending an interaction object and keeping both JSON copies synchronized.

Add encounter records to `Assets/Data/trouble-encounter.json` and `Assets/Resources/trouble-encounter.json`. Each has an `id`, `guestCharacterId`, `guestDisplayName`, `triggerId`, `selectionWeight`, `jamActive`, dialogue fields, and epilogue scene/lines. `selectionWeight` is a future scheduler hint; this build intentionally has only one encounter.

## Project notes

The district, homes, characters, gate, and finale set are Unity 3D scene content assembled from original runtime meshes and reusable forms. `WORLD_BIBLE.md` records the visual and licensing direction. The flagstone PBR maps and the statue are local CC0 assets; source links and license notes are in `Assets/Resources/Art/THIRD_PARTY_ASSETS.md`. The post-escape montage is original comic-style UI drawing; the Deadpool interruption returns to a live 3D scene. Uploaded images are likeness/style references only and are not imported as game art. The project targets Unity 2022.3 LTS and the built-in render pipeline/UI.
