<!--
SPDX-FileCopyrightText: 2021-2025 Friedrich von Never <friedrich@fornever.me>

SPDX-License-Identifier: BSL-1.0
-->

tdlib.native Maintainership
===========================

Publish a New Version
---------------------

1. Choose a new version. To avoid confusion, all the packages should follow the versioning of the upstream Telegram library and should have the same version among them. Increment the fourth version number if re-packaging of the same TDLib is performed.
2. Optionally, also update the submodule containing the tdsharp sources.
3. Prepare a corresponding entry in the `CHANGELOG.md`.
4. Update the license, if required.

   Note that the Windows package bundles its dependencies, so their licenses have to be checked as well.
5. Update the copyright information in the `Copyright` element of the `Directory.Build.props` file.
6. Update the `<VersionPrefix>` in the `Directory.Build.props` file.
7. Create a pull request, verify that the tests are okay. Merge it afterward.
8. Make sure the NuGet key you use for publishing is still active. If not, then rotate the key as explained in the corresponding section of this document.
9. Push a version tag (`v1.x.x`) to this repository. CI servers will do their job and upload the artifacts to the [Releases][releases] page.
10. If the release is not synchronized with a corresponding release of [tdsharp][], then it's recommended to [unlist][docs.unlist] it until the corresponding release of tdsharp is available. This will help the users to do a coordinated update and not update only a part of the libraries.

Update TDLib
------------
Note that there's a script `update-tdlib.fsx` that follows these steps automatically.

1. Update the Git submodule containing the TDLib sources.
2. Prepare a corresponding entry in the `CHANGELOG.md`.
3. Update the `VersionPrefix` element to the new library version in the `Directory.Build.props` file.
4. Regenerate the GitHub Actions workflow by running `dotnet fsi github-actions.fsx`.

Rotate Keys
-----------

CI relies on NuGet API key being added to the secrets. From time to time, this key requires maintenance: it will become obsolete and will have to be updated.

To update the key:

1. Sign in onto nuget.org.
2. Go to the [API keys][nuget.api-keys] section.
3. Create a new key with permission to **Push only new package versions** named `github-actions.tdlib.native` with a glob pattern of `tdlib.native*`.

   Alternately, if you have such key already and want to regenerate it, press the **Regenerate** button in its page section.

   If you are adding a new package, then use the **Push new packages and package versions** permission, but limit the key lifetime to the minimum possible date range — you will refresh it again using the limited set of permissions next time.
4. Paste the generated API keys to the [action secrets][github.secrets] section on GitHub settings (update the `NUGET_KEY` secret).

[docs.unlist]: https://docs.microsoft.com/en-us/nuget/nuget-org/policies/deleting-packages#unlisting-a-package
[github.secrets]: https://github.com/ForNeVeR/tdlib.native/settings/secrets/actions
[nuget.api-keys]: https://www.nuget.org/account/apikeys
[releases]: https://github.com/ForNeVeR/tdlib.native/releases
[tdsharp]: https://github.com/egramtel/tdsharp
