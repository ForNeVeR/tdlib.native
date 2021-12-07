tdlib.native Maintainership
===========================

Publish a New Version
---------------------

1. Update the Git submodule containing the sources, make sure everything builds correctly.
2. Update the license, if required.
3. Update the copyright year in the `copyright` section of the `tdlib.native.nuspec` file.
4. Update the version in the following places:
    - `.github/workflows/main.yml`: `PACKAGE_VERSION_BASE` environment variable (2 occurrences)
    - `common/download-release.ps1`: `$Version` variable default value
    - `common/nuget-pack.ps1`: `$Version` variable default value
    - `.appveyor.yml`: `test_script` section (2 occurrences)
5. Update the `releaseNotes` element in the `tdlib.native.nuspec` file.
6. Create a pull request, verify that the tests are okay.
7. Push a version tag (`v1.x.x`) to this repository. CI servers will do their job and upload the artifacts to the [Releases][releases] page.
8. Copy the release notes from the `.nuspec` to the GitHub release.
9. Mark the GitHub release as published.
10. Pack and upload the NuGet, as described below.
11. Attach the `.nupkg` to the GitHub release.

How to pack to NuGet
--------------------

Pack script requires NuGet 5 to be installed on a machine.

```console
$ pwsh ./common/download-release.ps1
$ pwsh ./common/nuget-pack.ps1
```

Then upload the `build/tdlib.native.<VERSION>.nupkg` to the NuGet server.

[releases]: https://github.com/ForNeVeR/tdlib.native/releases
