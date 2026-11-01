<!--
SPDX-FileCopyrightText: 2021-2026 Friedrich von Never <friedrich@fornever.me>

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
8. If you want to publish a new package (not a new version, but a new project/package) change the NuGet Trusted Publishing Policy: switch from **push only new packages versions** to **push new packages and package versions** (see below for more details).
9. Push a version tag (`v1.x.x`) to this repository. CI servers will do their job and upload the artifacts to the [Releases][releases] page.
10. If you were publishing a new package, revert back the change you did at step 8.
11. If the release is not synchronized with a corresponding release of [tdsharp][], then it's recommended to [unlist][docs.unlist] it until the corresponding release of tdsharp is available. This will help the users to do a coordinated update and not update only a part of the libraries.

Update TDLib
------------
Note that there's a script `update-tdlib.fsx` that follows these steps automatically.

1. Update the Git submodule containing the TDLib sources.
2. Prepare a corresponding entry in the `CHANGELOG.md`.
3. Update the `VersionPrefix` element to the new library version in the `Directory.Build.props` file.
4. Regenerate the GitHub Actions workflow by running `dotnet fsi github-actions.fsx`.

NuGet Publishing Policy
-----------------------

This repository relies on [NuGet Trusted Publishing][docs.nuget-trusted-publishing] policy. In case you need to create it again, follow these steps:

1. Sign in to nuget.org.
2. Go to the [Trusted Publishing][nuget.trusted-publishing] section.
3. Create a new policy:
   - **Policy Name:** `github.tdlib.native`.
   - **Repository Owner:** `ForNeVeR`.
   - **Repository**: `tdlib.native`.
   - **Workflow File**: `main.yml`.
   - **Environment:** _leave empty_.
   - **Scope:** **push only new packages versions**.
   - **Glob Patterns and Packages**: `tdlib.native*`.
4. Put your nuget.org username into the `NUGET_USER` secret on the [action secrets][github.secrets] section of GitHub settings.

When introducing a new package, you can adjust the policy temporarily to allow it to **push new packages and package versions**.

[docs.nuget-trusted-publishing]: https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing
[docs.unlist]: https://docs.microsoft.com/en-us/nuget/nuget-org/policies/deleting-packages#unlisting-a-package
[github.secrets]: https://github.com/ForNeVeR/tdlib.native/settings/secrets/actions
[nuget.trusted-publishing]: https://www.nuget.org/account/trustedpublishing
[releases]: https://github.com/ForNeVeR/tdlib.native/releases
[tdsharp]: https://github.com/egramtel/tdsharp
