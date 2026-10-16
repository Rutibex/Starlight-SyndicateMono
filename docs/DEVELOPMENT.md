# Development guide

## Stack and repository layout

- **Language and runtime:** C#, .NET 8, MonoGame DesktopGL (`3.8.5.1` in the maintained project definition).
- **Project:** `StarlightSyndicate.sln` → `Source/StarlightSyndicate.csproj`.
- **Entry point:** `Source/Program.cs` dispatches tests and then `DemoGame.Run()`.
- **Simulation and save model:** `Source/World.cs` plus feature-specific partial `DemoWorld` implementations.
- **Renderer, input and windows:** `Source/DemoGame.cs`, `SpaceRenderer.cs`, `FoundationJ.cs`, `ClientUiK.cs`, `SystemsUiQ.cs` and related UI partials.
- **Data loading:** runtime JSON/PNG/glTF files under `Assets/`. Many loaders use `AppContext.BaseDirectory/Assets/...`; never assume the current working directory alone is sufficient.
- **Procedural art source:** `Art/ShipRecipes.json` and `Art/FactionArtDirection.json`.
- **Art generation & tests:** `tools/ship_model_pipeline.py`, `tools/test_ship_model_pipeline.py`. Runtime glTF LOD assets and thumbnails are committed because the game consumes them directly.
- **Documentation:** `docs/` and root community/release files.

The original archive contained many separate pass/stage `.csproj`, provenance and regression logs, and self-contained runtime files. These are not needed to compile the consolidated C# program. Original C# gameplay and test sources are retained, including their historic suffixes, rather than being renamed and risking broken type dependencies.

## Build

```powershell
dotnet restore StarlightSyndicate.sln
dotnet build StarlightSyndicate.sln -c Release
dotnet run --project Source/StarlightSyndicate.csproj -c Release
```

The project uses SDK-style wildcard compilation of C# files in `Source/` and links the `Assets/` tree into output, preserving its relative paths. This can copy thousands of files per build.

## Tests

From the repository root:

```powershell
python tools/verify_runtime_assets.py
python tools/test_ship_model_pipeline.py
dotnet run --project Source/StarlightSyndicate.csproj -c Release -- --stage-q-selftest
dotnet run --project Source/StarlightSyndicate.csproj -c Release -- --selftest
```

`--stage-q-selftest` invokes the latest Q functional regression suite. `--selftest` runs the long-standing broad backend smoke/group tests. `Source/Program.cs` defines many additional selective `--*-selftest` flags for individual subsystems. **Some tests may be expensive or depend on specific platform/runtime state**; all of them need actual Windows/.NET verification before claiming a certified release. Save-changing tests should isolate `STARLIGHT_SAVE_DIR` or use their fixture directories; never run tests against a personal live campaign without a backup.

CI workflows in `.github/workflows/` install .NET 8, validate required assets, restore/build the game, and optionally package a Windows artifact. A separate opt-in package workflow does not create or publish a public GitHub Release.

## Working with assets

Treat **data or art that is generated but used at runtime as source-controlled game content**. In particular, keep the `Assets/FleetModels`, `Assets/ShipThumbnails`, `Assets/Spatial43`, `Assets/Sde40`, `Assets/Retheme41`, `Assets/Stations44` and `Assets/SystemsQ` trees. Human-editable procedural ship definitions are in `Art`; reviewed mesh export pairs from historic delivery builds were removed.

The model pipeline accepts the `build`, `validate` and `import-obj` workflows described in the script help; running `build` can overwrite generated runtime models, so review the resulting diffs carefully. Source import history from the development archive is not automatically reproducible without the original input datasets, data import versions, and rights clearance. If you update imported static data, pin the upstream build and update provenance and coverage documentation.

## Coding changes

- Keep simulation actions deterministic where practical, and avoid mutating authoritative inventory or saved state solely from rendering/UI code.
- Extend existing partial classes and state migration carefully; keep save serialization backwards compatible when possible.
- Distinguish independently authored content from licensed upstream reference data; document the source and rights for everything newly imported.
- Run focused tests and `--stage-q-selftest`, then verify the Windows UI. Do not claim exact source-game fidelity for an authored approximation.
- Avoid committing binary builds, test logs, personal saves, generated input exports, `.vs/`, `bin/`, `obj/` or `publish/`.
