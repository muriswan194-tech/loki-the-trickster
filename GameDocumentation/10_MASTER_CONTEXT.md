# Master context for future Codex sessions

## One-paragraph brief

Work only inside the existing Unity project `Lokabrenna` (Unity 2022.3.62f1, Built-in Render Pipeline). Build one original, stylized 3D Asgard courtyard stealth-comedy trouble for Loki: the Bifrost is guarded while the royal watch searches for Odin's missing golden apple. The player explores, observes and improvises with one flexible illusion and a small number of props that share clear sight/hearing/movement rules. No combat, no mission chain and no extra playable areas. Escape leads to a short comic montage, a separate live portal cameo/end beat and the final card. The art target is warm cream stone and aged gold, deep green/gold Loki, bronze/crimson guard and selective emerald magic, with clear real-time 3D silhouettes and restrained comic presentation.

## Authority order

1. Explicit user gameplay and scope directions in the active Codex chat.
2. Current project source/data/readme and `WORLD_BIBLE.md` for actual implementation and licensing.
3. These ten documents for consolidated context and asset interpretation.
4. The 23 generated concept boards as visual guidance only.
5. The three cached web thumbnails only as unlicensed discovery/mood references; never as production art.

If a board conflicts with current map coordinates, the one-encounter requirement, systemic play, or license rules, record the conflict and prefer the higher-authority source. The board labels “The Golden Realm: Trickster” do not settle the public title.

## Project map and entry points

- Main scene: `Assets/Scenes/Lokabrenna.unity`.
- Finale scene: `Assets/Scenes/LokabrennaEpilogue.unity`.
- Main runtime builder/controller: `Assets/Scripts/LokabrennaGame.cs`.
- District loader/data classes: `Assets/Scripts/DistrictData.cs`.
- Original mesh library: `Assets/Scripts/LokabrennaMeshLibrary.cs`.
- Montage/finale: `Assets/Scripts/LokabrennaEpilogue.cs`.
- Editor generation/bootstrap: `Assets/Editor/LokabrennaArtAssets.cs`, `Assets/Editor/LokabrennaBootstrap.cs`.
- Editable district layout: `Assets/Data/asgard-district.json`; keep `Assets/Resources/asgard-district.json` synchronized.
- Encounter lines/metadata: `Assets/Data/trouble-encounter.json` and Resources mirror.
- License records: `Assets/Resources/Art/THIRD_PARTY_ASSETS.md`.
- Visual interpretation: `02_VISUAL_REFERENCE_ANALYSIS.md`; use `03_WORLD_BUILDING_BIBLE.md` and `04_CHARACTER_DESIGN_BIBLE.md` for art consistency.

## Known implementation gaps to check before claiming completion

- District art is generated/assembled in runtime code, not yet a finished prefab-based asset set.
- Loki and guard lack a production skinned/rigged model and polished clip library.
- The current gate escape check is one physical exit; map concept boards depict a side exit.
- The folder contains 23 unique generated project references, not 22, and filenames are timestamped rather than numbered.
- The final minor cape/hair edits have not been recompiled or inspected in the actual Unity Editor.

## Next work sequence

1. Confirm title, playable exit arrangement, ability scope and visual/IP boundary.
2. Capture one representative in-game camera view in Unity; compare it to the sunny courtyard target and character silhouette board.
3. Pick one small architecture/prop kit and record units, pivots, colliders and material assignments.
4. Fix camera, character/guard silhouettes and the systemic escape path before adding optional decorative variants.
5. Test multiple player approaches against the same guard/prop rules, then validate the menu → trouble → montage → finale flow.
6. Keep a clear change list, preserve existing working scenes/assets, and check the Unity Console after code edits.

## Reference folder

The original visual files remain in the user-supplied local `visual assets` folder and were not copied into the project by this documentation task. For portable collaboration, copy only approved/generated references into a project-owned reference folder after checking rights, and preserve source/license notes. Do not import the saved webpage CSS, HTML, avatar icon or copyrighted cached thumbnails as game assets.
