# Player guide

Starlight Syndicate is a single-player sandbox. Your ship, finances, reputation, possessions and location persist. Time passes in the simulation, and many actions use actual inventory, fuel, money, capacitor or deadlines.

## Getting started

1. Start a new character. Select an available origin, review your starting ship and current location, then enter the desktop.
2. Use the **SS** button at the top of the left application rail to open the application list. Rearrange, resize or stack windows as needed.
3. At a station, open **Ship hangar**, **Item hangar** and **Ship fitting**. Use the fitting interface to understand modules, slots and the isolated fitting simulator.
4. Open **Regional market** to inspect orders, **Agents** for missions and **Skills** to examine qualification and training.
5. **Undock**, use the overview and selected-item controls to approach, orbit, target, warp, or dock. The space view also provides a HUD, movement and module controls.
6. Open the **Galaxy map** to look up destinations; use system navigation, stargates and bookmarks to explore.

### Core shortcuts and input

| Action | Input |
| --- | --- |
| Save game | `F8` |
| Load game | `F9` |
| Toggle fullscreen | `F11` |
| Close a menu, dialog or focused search | `Esc` |
| Move / resize application windows | Drag their header or resize handles |
| Context-sensitive actions | Right-click on supported items, ship objects and windows |
| Scroll lists and zoom certain views | Mouse wheel while hovering that view |

Some input is contextual. Use each window's on-screen buttons and tooltips rather than relying on undocumented shortcuts.

## Ships, equipment and combat

**Fitting** supports equipment slots, character restrictions, CPU/power and capacitor budgets, plus a simulated configuration so you can experiment without spending your actual resources. Some larger ships have specialist holds or modes. **Information** windows explain item attributes, requirements and fitting-related data. **Drones** and the probe scanner have their own interfaces where appropriate.

Space encounters use target selection, equipped weapons, defense, capacitor, movement and range. Abilities and mechanics vary by hull. NPC ships and structures exist as simulated actors; not every real-world EVE role or combat effect has been implemented.

## Economy, industry and exploration

Trading, owned inventory, station services, resource mining, research and manufacturing are connected to the same simulated economy. The gameplay path is typically **find resources → haul them → reprocess/manufacture → fit or sell**. Blueprints, some recipes and input requirements are represented directly in source-derived catalog data, while a number of prices, industrial rules and rewards are locally authored.

Exploration activities include signatures, scanners, mission pockets, regional sites and optional events. Some activities require eligible hulls or equipment, character skills, docking or local security conditions. Mission steps and inventory changes are persistent; avoid treating valuable cargo or supplies as disposable.

## The seven newer application windows

From **SS**, open any of these Q-stage windows:

| Window | What you can do |
| --- | --- |
| Industry / Blueprint Activities | Acquire finite blueprint originals, install manufacturing/research/copy/invention jobs, manage input materials and collect output |
| Resource Survey / Extraction | Find timed resource fields, survey moons, mine finite deposits and operate connected industrial systems |
| Planetary Colonies | Deploy colonies, place processor/extractor pins, link and route materials, produce commodities and export through physical launches |
| Module Mutation / Capital Holds | Store fleet/fuel cargo, retain fitted ships and modify compatible equipment instances |
| Structures / Custody / Legal Timers | Anchor/refuel structures, manage stored property, choose safety settings and track legal consequences |
| Territorial Operations | Choose whether to enlist, deploy local objectives and complete guarded capture workflows |
| Regional Industrial Economy | Observe or enable finite industrial NPC policy and inspect its local economic activity |

Several actions require physical proximity, actual fuel/materials or legal authorization. Some of these systems are approximations rather than full source-game parity; see [Features and limitations](FEATURES.md).

## Fleets, organizations and story activities

You can hire and manage named crew, organize wings and squads, outfit eligible subordinate ships, run group-oriented sites and interact with corporations, standings and agents. Depending on the activity, you can choose Support, Oppose, Independent or Abstain; participation is not always mandatory.

## Backing up your progress

Saves live outside the repository, in `%LOCALAPPDATA%\StarlightSyndicateDemo` on Windows unless `STARLIGHT_SAVE_DIR` is set. Copy that directory before updating. Inventory, jobs, structures and campaign state are linked, so use an in-game save before exiting.
