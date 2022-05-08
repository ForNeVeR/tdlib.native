tdlib.native Maintainership
===========================

Publish a New Version
---------------------

1. Update the Git submodule containing the sources, make sure everything builds correctly.
2. Update the license, if required.
3. Update the copyright year in the `copyright` section of the `tdlib.native.nuspec` file.
4. Update the version in the following places:
    - `.github/workflows/main.yml`: `PACKAGE_VERSION_BASE` environment variable (4 occurrences)
5. Update the `releaseNotes` element in the `tdlib.native.nuspec` file.
6. Create a pull request, verify that the tests are okay. Merge it afterwards.
7. Push a version tag (`v1.x.x`) to this repository. CI servers will do their job and upload the artifacts to the [Releases][releases] page.
8. Copy the release notes from the `.nuspec` to the GitHub release.
9. Download the `.nupkg` file from the release and upload the NuGet repository.
10. Mark the GitHub release as published.

[releases]: https://github.com/ForNeVeR/tdlib.native/releases
