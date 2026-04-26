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
3. Enter the same plain semver version, for example `2.3.1`.

The workflow aborts if the input does not match `Assets/InspectorEvents/package.json`, if the version is not plain `X.Y.Z` semver, or if the tag already exists.
