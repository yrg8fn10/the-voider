# Installing The Voider (playable build)

The Voider needs four files in your game's mod loader. Two come with this
release (VehicleFramework and Nautilus), so you only fetch the loader yourself.

1. Install [Tobey's BepInEx Pack for Subnautica](https://www.nexusmods.com/subnautica/mods/1108)
   into your Subnautica folder (works on Windows, macOS, Linux and Steam Deck).
2. Run the game once, then quit at the main menu.
3. Drop the `plugins` folder from this release into `Subnautica/BepInEx/`
   (merge it with the existing one). It contains:
   - `VoiderMod.dll` - The Voider
   - `VehicleFramework.dll` - the vehicle framework (GPL v3, source: github.com/NeisesMike/VehicleFramework)
   - `Nautilus.dll` - the modding API (GPL v3, source: github.com/SubnauticaModding/Nautilus)
4. Start the game, open any save, open the console (F3, then F1, or the tilde key),
   and type: `spawn Voider`

The sub spawns in front of you. Board via the cyan hatch on top of the hull,
sit in the cockpit, and drive. This is an early blockout build: the hull and
cockpit are real and drivable, the five-deck interior comes next.
