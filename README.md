# InspectorEvents
A lightweight event serialization toolkit for the Unity Inspector.

> Warning: Odin Inspector is required to use this project.

## Installation
Add this URL in Unity Package Manager (Add package from git URL):
`https://github.com/Hissal/InspectorEvents.git?path=/Assets/InspectorEvents`

To pin a specific release, add the tag after the URL:
`https://github.com/Hissal/InspectorEvents.git?path=/Assets/InspectorEvents#2.3.1`

## Releasing
1. Update `Assets/InspectorEvents/package.json` to the version you want to publish.
2. Run the `Release` GitHub Actions workflow manually.
3. Enter the plain semver version, for example `2.3.1`.
4. Leave `auto_fix_package_version` enabled if you want the workflow to update `Assets/InspectorEvents/package.json` on `main` automatically when it does not match.

The workflow aborts if the version is not plain `X.Y.Z` semver or if the tag already exists. If `auto_fix_package_version` is disabled, it also aborts when the input does not match `Assets/InspectorEvents/package.json`.
