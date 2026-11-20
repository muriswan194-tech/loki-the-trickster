# Open questions and risks

## Decisions to resolve

1. **Public title:** The game title is **Loki the Trickster**; `Lokabrenna` remains the Unity project/folder name.
2. **Character/IP boundary:** The user wants Loki and a Deadpool cameo; the task text and World Bible also require original/appropriately licensed assets. Decide how directly recognizable the characters may be for the jam's distribution. No film models, screenshots, logos, costume textures or voices are present as licensed project assets.
3. **Reference count/order:** The expected 22 images are not numbered and the folder has 23 unique generated game boards. The present analysis indexes them 01–23 and keeps three cached web images separate. Confirm whether one generated board should be retired or keep all 23.
4. **Map authority:** Concept boards show an enclosed square with centered fountain and side exit; the JSON describes a wider connected district, east fountain and several promenades. The JSON/project is the current coordinate source. Decide whether to revise the JSON from a final level layout before further art placement.
5. **Escape routes:** Image maps show a side window/servant gate, but the audited runtime checks escape at the Bifrost gate. Decide between a real second exit and multiple systemic ways past one gate. Do not advertise a second physical exit until implemented and tested.
6. **Ability scope:** The small brief asks for one flexible illusion plus props; current source exposes disguise and multiple alternate forms, hidden relics, conjuring and teleport. Decide which controls remain in the jam build; test onboarding before keeping them all.
7. **AI states:** Concept sheet has four states, World Bible has noticed/suspicious distinctions, code has five states. Document one player-facing model and decide whether internal `Investigate`/`Return` stay separate.
8. **Verticality:** Art boards show bridges, stairs and balconies; current map data is planar x/z and the player collision/guard movement is simple. Confirm if any elevated walkable route is required or if distant nonwalkable skyline is enough.
9. **Character quality:** Current Loki is assembled procedurally and has no imported rigged mesh/animation clips. Decide if the jam accepts stylized original geometry or needs one approved, reusable, licensed rigged asset.
10. **Target display/performance:** Target resolution, frame rate, hardware limits and game build platform were not specified in the reference folder.
11. **Audio and accessibility:** Audio implementation and subtitle/contrast/font-size requirements were not established. The visual boards' text should not be treated as final UI copy.
12. **Reference rights:** The three cached web images come from Marvel Rivals, Asgard's Wrath 2 and a Pinterest artist page with unverified rights. They are excluded from production use. The generation terms/ownership of each AI board should also be checked before public or commercial redistribution.

## Main risks and mitigations

- **AI concept art looks production-ready but is not:** Convert only chosen geometry into measured meshes and material maps; create original assets; never use the image itself as proof of 3D readiness.
- **Scope expands through each prop board:** Start with bell, crate and a single cover object; add only if the core works.
- **Narrative systems multiply:** Keep montage/portal as authored short sequence; do not turn them into encounters.
- **Camera is obstructed by architecture/character:** Test the orbit camera against actual mesh scale, horns, cape, arches and walls.
- **Visual noise hides stealth cues:** Reserve emerald for magic, warm neutral for world, crimson/red for alert; use one awareness indicator.
- **Data/code drift:** Synchronize the two JSON copies and IDs; keep all geometry aligned to documented coordinates.
- **Overclaiming readiness:** The current documentation audit did not compile or playtest the actual editor project. The last known isolated compile predates final minor character visual edits.
