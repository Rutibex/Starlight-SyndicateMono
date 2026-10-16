# GitHub repository setup and release-page copy

**Public-release gate:** see [Legal & assets](LEGAL_AND_ASSETS.md) and [Release checklist](RELEASE_CHECKLIST.md). Prepare the repository as **private** until rights to distribute the source-derived EVE datasets and all original assets are cleared. The packaged files are complete for a local source handoff, not an authorization to publish.

## Suggested GitHub "About" field

**Description:** `Offline single-player space sandbox with ships, exploration, fitting, trading, industry, factions and a persistent galaxy. Built in C# / MonoGame.`

**Topics:** `csharp`, `monogame`, `dotnet`, `space-game`, `sandbox-game`, `single-player`, `simulation`, `procedural-generation`, `game-development`, `science-fiction`

Add screenshots and links through the root README; GitHub automatically renders it on the repository front page. Avoid enabling the GitHub Discussions tab until you're ready to support public discussion.

## Upload in GitHub Desktop (no terminal required)

1. Extract this package to a new local folder, then open **GitHub Desktop → File → Add local repository**. If the folder is not a Git repository, use **Create a Repository Here**.
2. Select the project folder containing `README.md` and `StarlightSyndicate.sln`, not its parent ZIP-extraction folder.
3. Review the Changes tab: it should list `Source/`, `Assets/`, `Art/`, `docs/`, `tools/` and the GitHub configuration. It should **not** contain DLLs, EXEs, personal saves, validation logs or `bin/obj` output.
4. Commit, then **Publish repository** with **Keep this code private** selected. Large assets can take time to upload; each tracked file is under GitHub's usual 100 MB individual-file cap.
5. Verify the README screenshot previews, navigation links and Actions checks. Check code and data rights before changing visibility.

## Release procedure

When permission and Windows regression checks are complete, run **Actions → Package Windows → Run workflow**. This creates a downloadable Actions artifact for private testing. It does **not** create a public release or upload binaries to the repo.

After testing the artifact on a clean machine, choose **Releases → Draft a new release**, create a version tag, attach the verified ZIP and paste notes adapted from [Release notes template](RELEASE_NOTES_TEMPLATE.md). Make sure the package includes its `Assets` folder; an isolated EXE is not a playable distribution.

## Size and asset policy

This is an intentionally large game-data repository. `.gitignore` excludes build products and redundant exports, **not** required runtime catalogs. Avoid turning on Git LFS indiscriminately: its quotas and release/clone behavior would need to be configured and documented separately. If repository size becomes a problem, plan a versioned, rights-cleared asset download pipeline and validate it before removing anything that the program loads.
