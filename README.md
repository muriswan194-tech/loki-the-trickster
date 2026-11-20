# 'Lokabrenna' — The world doesn't revolve around Wildfire, the Trickster

A compact, original Unity game-jam prototype. It uses Unity's built-in primitives and UI; no franchise artwork, purchased assets, additional packages, or online dependencies are included. The original placeholder visuals are generated at runtime by the project code.

## Open and play

1. Install Unity Hub and a Unity **2022.3 LTS** editor.
2. In Unity Hub, choose **Add project from disk** and select this `Lokabrenna` folder.
3. Let Unity finish importing. The included editor bootstrap creates and opens `Assets/Scenes/Lokabrenna.unity` automatically on first open. If needed, open that scene manually.
4. Press **Play**. Choose **New Game** to begin. The gate crisis begins after a short exploration window.

## Controls and play

- **WASD / arrow keys:** explore the courtyard.
- **Space:** use the nearby interaction shown on screen.
- **R:** restart after getting caught or crossing the Bifrost.
- Try the illusion sigil, bell, apple distraction, or shapeshift prompt near the guard. Use a distraction to reach the Bifrost at the north end.

Getting caught is a short, funny reset prompt, not a combat or health system.

## Community-editable district layout

Edit `Assets/Data/asgard-district.json` to change the layout. The same file is copied to `Assets/Resources/asgard-district.json` so Unity can load it at runtime; keep both copies in sync. Coordinates use the ground plane: **x** is left/right, **z** is down/up, and the origin `(0,0)` is the centre of the courtyard. Distances are Unity units.

- `bounds`: player movement limits.
- `playerStart`, `gate`: x/z positions.
- `patrol.points`: guard route waypoints; `speed` is units per second.
- `interactions`: stable `id`, `type` (`bell`, `prop`, `illusion`), x/z, interaction `radius`, and on-screen `label`.
- `district`: display metadata for future community additions.

When adding an interaction, use one of the implemented types or extend `LokabrennaGame.Interact` to define its behavior. Add its new point to both JSON copies. The scene is built from the data and primitive shapes, keeping the layout human-editable and separate from gameplay rules.

## Project notes

The prototype targets Unity 2022.3 LTS with the built-in render pipeline and Unity UI. No online services or third-party runtime packages are required. The Loki-inspired character is an original, low-detail green-and-gold trickster placeholder; it is not a recreation of any film or comic design.
