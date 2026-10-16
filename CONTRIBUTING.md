# Contributing

Thanks for helping improve Starlight Syndicate. Start with [Development](docs/DEVELOPMENT.md) and [Features and limitations](docs/FEATURES.md), then search existing issues.

## Bugs and improvements

Use the GitHub issue templates. For a bug, explain how to reproduce it, current/expected behavior, Windows/.NET version, and whether an older save is involved. For a proposal, distinguish small fixes from new parity or content work. Do not upload private player data or nonredistributable files.

## Before a pull request

1. Keep edits focused and explain what changed.
2. Test an up-to-date build, `--stage-q-selftest`, relevant subsystem suites and the asset verifier; report what you could not run.
3. Preserve serialized state migration and resource accounting rules; add assertions for behaviors that previously failed.
4. Avoid introducing data copied from third-party games or externally licensed assets without demonstrated permissions and accurate attribution.
5. Do not commit `bin/`, `obj/`, publishing outputs, screenshots from test runs, raw output logs or personal save files.

**Licensing:** no general contribution license has yet been selected. Before accepting external code contributions or making this public, maintainers should choose and communicate a contributor and repository license arrangement. No contribution is implied to waive a contributor's copyright.
