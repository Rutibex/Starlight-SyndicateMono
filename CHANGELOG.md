# Changelog

All changes to the cleaned GitHub packaging are documented here. Earlier development-stage archives existed as incremental deliveries, but this repository intentionally starts with one consolidated history.

## Unreleased — consolidated Q01–Q07 source handoff (2026-10-15)

### Repository

- Reorganized into a single solution/project suitable for .NET 8 and MonoGame DesktopGL.
- Removed bundled executables, third-party runtime copies, old stage projects, build logs, verification reports, stage readmes, scratch scripts, redundant art exports and duplicated gallery screenshots.
- Retained the actual runtime data, models, thumbnails, spatial shards and source art definitions.
- Added player, installation, feature-status, architecture, legal, contribution, security and release documentation.
- Added GitHub Actions for Windows build/checks and manual creation of a Windows distribution artifact.

### Game systems inherited from Q01–Q07

- Blueprint activity workflows, resource extraction, planetary production, module mutations, special ship holds, legal/structure custody, guarded territorial operations and the optional finite NPC industrial economy.
- Inherited systems from prior stages remain in the source.

### Verification note

The packaging process performed file-integrity/static checks and Python asset validation; it did **not** run a Windows/.NET build in the packaging environment. Imported data redistribution permissions are not established; public release is blocked pending review.
