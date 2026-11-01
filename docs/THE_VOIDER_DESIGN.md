# THE VOIDER - Design Document (v0.1)

A gargantuan Precursor razor-class combat submarine for Subnautica 1.

## 1. What the Voider is

The Voider is a leviathan-sized submarine built by the Precursors as a razor-type combat design. Where the Cyclops is a transport and the SeaTruck is a tug, the Voider is a warship: a walkable, five-deck hull wrapped in blade arrays that cut leviathan-class predators in a single pass.

Design pillars:

- **Gargantuan but livable** - five real decks you walk through, not one cramped cab
- **A one-tap weapon** - the razor blades along the hull
- **No custom 3D modeling** - the hull is generated in code, the interior reuses in-game Precursor assets
- **Fits the original Subnautica** - Precursor tech aesthetic, ion power, unlockable by scanning

## 2. Identity and lore

| | |
|---|---|
| Name | The Voider |
| Class | Razor-class Precursor deep submersible |
| Built by | The Precursors |
| Unlock | Scan the Voider fragments inside the Precursor bases (Sanctuary, Disease Research Facility, Primary Containment Facility) |

Mod lore: when the Kharaa outbreak forced the Precursors into their last work, they built the Voider as a warship to defend their research sites against leviathan-class fauna. It outlived its builders, and its blueprint still sleeps in their bases, waiting for someone who can read it.

## 3. Specifications

| Stat | Value |
|---|---|
| Length | 62 m |
| Beam | 14 m |
| Height | 18 m |
| Class | Gargantuan submersible |
| Crush depth | 1,700 m (2,500 m with hull upgrade MK2, 3,000 m with MK3) |
| Cruise / flank speed | 8 / 14 knots |
| Crew | 1 pilot, 1 copilot, up to 8 passengers |
| Power | 6 Precursor ion power cells |

## 4. Deck plan

| Deck | Name | What is inside |
|---|---|---|
| 1 (keel) | Docking bay | Built-in dock for the Seamoth and the Prawn, exit hatch, ladders up |
| 2 | Engine room | Ion power cells, generators, razor-blade hydraulics |
| 3 | Crew quarters | 6 bunks, lockers, fabricator, medkit fabricator, radio |
| 4 | Captain's deck | Captain's quarters, and the bridge at the very bow of the hull |
| 5 | Greenhouse | Glass dome with grow beds and 360-degree views |

The bridge sits at the very front of the submarine, at the bow of deck 4, with the pilot and copilot stations side by side, per the design brief.

## 5. The razor blades

- **Where:** blade arrays run along both sides of the hull, plus one prow blade at the very front.
- **Damage:** contact damage to leviathan-class creatures is lethal in one pass (the one-tap). Exact values are set in one config file, so they stay tunable.
- **Balance note:** the one-tap is the design, not a bug. A config toggle can soften it to heavy damage if a player wants the danger back.

## 6. SeaTruck comparison (from Below Zero)

The SeaTruck is Below Zero's modular tug. The Voider is Subnautica 1's warship and mobile base.

| | SeaTruck (Below Zero) | The Voider (Subnautica 1) |
|---|---|---|
| Role | Modular tug | Warship + mobile base |
| Interior | Driver's cab only | Five walkable decks |
| Growth | Attachable trailers | Fixed hull, built-in dock instead |
| Weapons | None | Razor blades, one-tap on leviathan-class |
| Dock | None, needs a Moonpool at base | Built into the keel |
| Depth | 300 to 1,000 m | 1,700 to 3,000 m |
| Art | Official models | None needed: procedural hull + in-game assets |

## 7. How it gets built (no modeling)

| Phase | Work |
|---|---|
| 0 (done) | This repo: design doc + code scaffold |
| 1 | VehicleFramework skeleton that spawns the sub |
| 2 | Procedural hull: the mesh is generated in code (Unity Mesh API) - a blade-wrapped spine built from primitives and swept shapes |
| 3 | Interior: five walkable decks, ladders, the greenhouse dome glass |
| 4 | Weapons: the collision damage system for the blades |
| 5 | Sounds, cockpit UI, balancing, in-game testing |

Honest note: procedural geometry in code is real, but heavy. If it proves too slow in game, the fallback is a community CC0 sub hull mesh, with the blades and interior unchanged.

## 8. Install (target)

Requires Subnautica 1 with a mod loader (QModManager or BepInEx, decided in Phase 1) plus VehicleFramework, then drop `VoiderMod` into the mods folder. These notes are finalized in Phase 1 alongside the loader choice.
