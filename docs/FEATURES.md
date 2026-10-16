# Features, status and known limitations

This document describes **implemented categories**, not a claim of feature-complete EVE Online behavior. The game retains many systems accumulated across development stages A–P, and adds the Q01–Q07 systems below. Content may be authored, adapted or source-derived depending on the subsystem.

## Established game systems

- **Simulation and ship control:** 3D space presentation, ships and hull stats, selection, travel and targeting, modules, damage, fitting, capacitor, heat, drones, ship information and fitting simulation.
- **Universe and travel:** galaxy maps, solar systems, stations, imported spatial actors, stargates, bookmarks, scanning and many exploration activities.
- **Character and commerce:** characters, skills, wallet, market, inventory, hangars, contracts, jobs, agents, missions, loot and persistent game saves.
- **Population and cooperation:** AI-controlled pilots, organization systems, crew/fleet roles, activities, paid labor and local economy.
- **Advanced PvE/PvP-inspired sandbox:** complex encounters, regional activities, physical structures, territorial operations and optional event content.

## Q01–Q07 implementations

| Area | Implemented now | Important gaps |
| --- | --- | --- |
| Q01 — industry | Source blueprint activity catalogue; owned originals/copies; inputs, fees, slots, facility occupancy, time, completion and delivery | Full economic availability, source NPC orders and all activity modifiers |
| Q02 — mining | Physical timed fields, moon surveys, extraction, real cargo, capacitor-linked stabilizers and deposits | Exact source fields, moon composition, survey/head UI and extraction rules |
| Q03 — planets | Planet pins, finite routing and storage, schematics, processing, imports and physical orbital exports | Spherical surface geometry, full extractor-head mechanics and customs systems |
| Q04 — mutation / hulls | Instance-level compatible mutators; fleet and fuel holds; fitted-hull custody | Complete coverage of mutators, every ship role/fighter/equipment effect |
| Q05 — legal / structures | Safety settings and timers, local duels, ACLs, physical structures, custody and asset-safety flows | Complete legal exemptions, services, repair/structure fidelity |
| Q06 — territory | Voluntary sides, physical guarded objectives, capture operations and settlement | Full source front geometry, contested objectives, standings and reward fidelity |
| Q07 — finite economy | Opt-in local NPC industrial policy, supply/production/activity accounting | Fully validated long-horizon economic balance and per-source policy coverage |

**All seven remain *partial* with respect to the project's original exhaustive parity goals.** The UI is functional but does not claim pixel-perfect equivalence or exact EVE mechanics. NPC stats, mission/reward recipes, availability and timers can be locally authored rules. Where the original build described specific numerical counts, treat them as implementation snapshots rather than verified comprehensive guarantees.

## Operational limitations

- The imported static-data build is **3569502**. It is not updated dynamically when EVE changes. The game has a separate rethemed setting, but much of the underlying reference content remains traceable to EVE-derived data.
- The previously bundled Windows executable/runtime has been intentionally excluded. Rebuild the game before playing or making a binary distribution.
- The game was not compiled or exercised on a Windows graphics machine as part of this repackaging. The GitHub CI and local test commands are provided as follow-up checks.
- Large catalogs, model LODs and thousands of compressed spatial shards are **required runtime assets**, not removable report output. Expect a sizable repository and build directory.
- Save schema **54** is used by this version. Migration exists for some older schema states, but compatibility with every historical build is not certified.
- There is no network server, live EVE login, ESI dependency or actual multiplayer population in the game's main sandbox design.
- Full licensing and derivative-data redistribution clearance is **not complete**; do not make a public release without it. See [Legal & assets](LEGAL_AND_ASSETS.md).

## Reporting a discrepancy

Provide the game version/commit, system location, ship and equipment, reproducible steps, save schema if relevant, expected and actual results, and any exception. For suspected balance or source-game parity mismatches, please state whether you are reporting a **bug in the implemented local rule** or requesting a **new source-parity feature**.
