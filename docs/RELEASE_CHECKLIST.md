# Release checklist

The repository is technically organized for version control, not yet cleared or certified for distribution.

## Rights and identity

- [ ] Resolve the EVE data and derived-content distribution concern described in [Legal & assets](LEGAL_AND_ASSETS.md).
- [ ] Choose an explicit license for original code/art and replace the provisional [LICENSE](../LICENSE), if appropriate.
- [ ] Review trademark/attribution notices, dependencies and third-party credits.
- [ ] Confirm project name, maintainers, support contact and version tag.

## Tests

- [ ] `python tools/verify_runtime_assets.py`
- [ ] `python tools/test_ship_model_pipeline.py`
- [ ] `dotnet restore StarlightSyndicate.sln`
- [ ] `dotnet build StarlightSyndicate.sln -c Release`
- [ ] `dotnet run --project Source/StarlightSyndicate.csproj -c Release -- --stage-q-selftest`
- [ ] `dotnet run --project Source/StarlightSyndicate.csproj -c Release -- --selftest`
- [ ] Start a new game on Windows and check 3D flight, docking, fitting, inventory, market, map, major Q windows, save/reload, and native graphics/audio.
- [ ] Migrate a **copy** of an existing schema-54 campaign, then validate assets and money/cargo persist.

## GitHub repository

- [ ] Upload all source and required `Assets/` paths (do not upload `bin`, `obj`, DLLs, test dumps, historical reports or saves).
- [ ] Choose GitHub topics, a description, visibility, default branch and project URL.
- [ ] Confirm external rights clearance *before* making the repo public.
- [ ] Run CI and inspect failed checks or package warnings.
- [ ] Tag a release such as `v0.1.0-alpha` **only after** acceptance.

## Windows artifact

- [ ] Run the manual `Package Windows` workflow in Actions, download its artifact and test it.
- [ ] Check the full Windows folder is included (`Assets`, runtime DLLs and native libraries).
- [ ] Scan the archive, check filesize, and test on a clean Windows machine.
- [ ] Draft GitHub Release notes with known limitations and any save-compatibility changes.
- [ ] Publish manually once permission and verification conditions are met.
