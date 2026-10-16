# Data origin, dependencies and public-release permissions

**STOP: review before setting this repository public or attaching downloadable binaries.** Packaging and code cleanup do **not** grant rights to redistribute material supplied by third parties. This is a provenance and permission notice, not legal advice or a determination that the project is permitted under a particular license.

## EVE-derived data and inspiration

The original project explicitly describes importing CCP's **EVE Online Static Data Export (SDE), build 3569502**, and historical asteroid-belt coordinates; it also uses analogous hulls, module/item attributes, NPC/station/system content, EVE-inspired interactions and adapted missions. Large retained runtime datasets include:

- `Assets/Sde40/` (imported item/attribute catalog), `Assets/Retheme41/` (derived/retitled entity registry), `Assets/Galaxy42/` and `Assets/Spatial43/` (star-cluster and spatial data).
- `Assets/Stations44/` and `Assets/Dogma45/` (station, agent and attribute/effect data).
- `Assets/SystemsQ/` and various older catalogs (additional gameplay data with source-derived fields and independently authored local rules).

These are included in this **local handoff** because removing them prevents the game from loading. **Their presence is not proof of permission for a public GitHub repository, a fork, or a standalone game distribution.** Retitling data does not necessarily remove a third party's underlying rights.

CCP's published [Developer License Agreement](https://developers.eveonline.com/license-agreement) limits use of licensed game data to applications serving the specified purpose, generally non-commercial applications supporting enjoyment of EVE. A standalone EVE-like game is not obviously covered by that defined purpose; independent permission or substantial replacement with original data may be necessary. Consult the current agreement and, if appropriate, CCP or legal counsel before distributing.

Additional official material: [CCP Static Data documentation](https://developers.eveonline.com/docs/services/static-data/).

> Suggested acknowledgement where required by the applicable CCP license: © 2014 CCP hf. All rights reserved. “EVE”, “EVE Online”, “CCP”, and related logos and images are trademarks or registered trademarks of CCP hf. The appropriate notice should be reviewed against the current terms; an acknowledgement alone does not authorize distribution.

## Original and third-party material

- **Project code and original setting:** author permission/license decision is pending. No blanket MIT/GPL/CC license is asserted in this package.
- **MonoGame DesktopGL, .NET, native dependencies:** restored or redistributed under their own licenses; review NuGet package manifests and dependency notices in a compiled binary before release. Build output dependencies were intentionally *not* committed to this source package.
- **Procedural ship models:** shipped as runtime `.gltf` and thumbnails, derived from local recipe and faction art files. Confirm that their recipes, palettes, names, reference meshes and any imported artwork are rights-cleared before public use.
- **Screenshots:** captured from the game UI; they may display third-party-derived names/data even though the image files are part of the repository documentation.
- **Original historical documents and upstream source data:** historical stage logs, research documents, source exports and raw reference datasets were omitted if not needed for running the game. Runtime-derived snapshots remain.

## Minimum public-release decision checklist

- [ ] Select and obtain authorization for a license covering **original code and art**.
- [ ] Resolve the right to host and distribute the imported/derived EVE datasets **for a standalone game**, or replace those datasets with fully original, cleared equivalents.
- [ ] Audit ship and world names, logos, trademark references and any non-original visual assets.
- [ ] Add proper notices/attributions for every third-party package and content source.
- [ ] Compile, test, and run the intended downloadable build on Windows; check backups and saves.
- [ ] Only then make the repository public or trigger a public release.

## Relationship disclaimer

This project is an unofficial independent prototype, not affiliated with, endorsed by, or sponsored by CCP hf. or EVE Online. Names and marks belong to their respective owners. This disclaimer itself does not replace or extend any applicable license.
