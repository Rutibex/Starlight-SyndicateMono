# Installation, build and launch

## Prerequisites

- **Windows 10/11 (64-bit recommended)** for the intended desktop target. The DesktopGL runtime may also work on Linux/macOS, but those platforms have not been verified for this package.
- **.NET 8 SDK**, for source builds. The generated program targets `net8.0`.
- Network access to NuGet during first restore, for `MonoGame.Framework.DesktopGL` version `3.8.5.1` and transitive dependencies.
- Hardware and drivers supporting MonoGame DesktopGL. An exact minimum GPU, CPU or memory requirement has not been established.

This archive is **source, not a compiled Windows release**. The original bundled self-contained runtime and all third-party executables/DLLs have been removed. No Python installation is needed to run the game after building it.

## Build and play from source

Run these commands from the repository root in a terminal or IDE developer terminal:

```powershell
dotnet --version
dotnet restore StarlightSyndicate.sln
dotnet build StarlightSyndicate.sln -c Release
dotnet run --project Source/StarlightSyndicate.csproj -c Release --no-build
```

The project copies the contents of `Assets/` into the build output. **Do not remove the data files or move the executable away from its sibling `Assets` directory.** Initial builds will copy thousands of spatial shards and ship models and can take a while.

Alternatively, open `StarlightSyndicate.sln` in Visual Studio with .NET desktop/.NET SDK support. Set the project as the startup project and run it in Release or Debug.

## Create a Windows distribution

After testing and clearing third-party distribution rights:

```powershell
dotnet publish Source/StarlightSyndicate.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -o publish/win-x64
```

Zip the **entire** `publish/win-x64` directory, not just its `.exe` file. The output includes the game, native dependencies, .NET runtime, `Assets/`, and config files. See `.github/workflows/package-windows.yml` for an opt-in build-artifact workflow. That workflow **does not publish a GitHub Release automatically**.

## Saves and settings

On Windows, save data defaults to:

```text
%LOCALAPPDATA%\StarlightSyndicateDemo\
```

The original campaign save is `save-3d.json`. The imported-universe save uses `save-universe43.json`. The client stores additional interface preferences/layout files nearby. Existing **save schema 54** continues to be used by the Q-stage code, with added properties initialized where absent; this is not a guarantee that every older save or custom file will migrate correctly.

Before trying a new build, **copy the entire save directory somewhere safe**. Do not put personal save files in a Git repository. For isolated testing you can override the save directory with the `STARLIGHT_SAVE_DIR` environment variable before launching the game.

## Troubleshooting

- **Cannot find Assets/Fleet.json or other JSON data:** ensure you are starting the copy produced by `dotnet build`/`publish`, or verify that the executable's folder has the complete `Assets` tree. Source assets are not optional.
- **MonoGame, SDL or OpenGL error:** update GPU drivers, verify required desktop graphics support, use the published output on its target OS, and include the full native dependency set. Open an issue with the exact exception.
- **Restore fails:** verify .NET 8 SDK is installed and that `api.nuget.org` is reachable. The previous hand-built stage rebuild scripts were removed; `dotnet build` is the supported route.
- **Saves missing or not loading:** check `%LOCALAPPDATA%\StarlightSyndicateDemo` or `STARLIGHT_SAVE_DIR`; try your backup, especially when changing versions.
- **First launch or build is slow:** several thousand compressed spatial records, large catalogs and model assets are included. Keep enough free disk space to accommodate copied build/publish outputs.

For a problem report, include Windows version, SDK version, graphics card, launch command, full error text and reproduction steps, but do not share personal saves without reviewing their content.
