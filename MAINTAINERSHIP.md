tdlib.native Maintainership
===========================

Publish a New Version
---------------------

1. Choose a new version. To avoid confusion, all the packages should follow the versioning of the upstream Telegram library, and should have the same version among them. Increment the fourth version number is a re-packaging of the same TDLib is performed.
2. Update the Git submodule containing the TDLib sources.
3. Optionally, also update the submodule containing the tdsharp sources.
4. Prepare a corresponding entry in the `CHANGELOG.md`.
5. Update the license, if required.
6. Update the copyright year in the `copyright` section of the `tdlib.native.nuspec` file.
7. Update the version in the following places:
    - `.github/workflows/main.yml`: `PACKAGE_VERSION_BASE` environment variable (4 occurrences)
8. Create a pull request, verify that the tests are okay. Merge it afterward.
9. Push a version tag (`v1.x.x`) to this repository. CI servers will do their job and upload the artifacts to the [Releases][releases] page.
10. Mark the release as published after it has been created and everything's alright.
11. If the release is not synchronized with a corresponding release of [tdsharp][], then it's recommended to [unlist][docs.unlist] it until the corresponding release of tdsharp is available. This will help the users to do a coordinated update and not update only a part of the libraries.

Rotate Keys
-----------

CI relies on NuGet API key being added to the secrets. From time to time, this key requires maintenance: it will become obsolete and will have to be updated.

To update the key:

1. Sign in onto nuget.org.
2. Go to the [API keys][nuget.api-keys] section.
3. Create a new key with permission to **Push new packages and package versions** and only allowed to publish **tdlib.native** package.

   Alternately, if you have such key already and want to regenerate it, press the **Regenerate** button in its page section.
4. Paste the generated API keys to the [action secrets][github.secrets] section on GitHub settings (update the `NUGET_KEY` secret).

[docs.unlist]: https://docs.microsoft.com/en-us/nuget/nuget-org/policies/deleting-packages#unlisting-a-package
[github.secrets]: https://github.com/ForNeVeR/tdlib.native/settings/secrets/actions
[nuget.api-keys]: https://www.nuget.org/account/apikeys
[releases]: https://github.com/ForNeVeR/tdlib.native/releases
[tdsharp]: https://github.com/egramtel/tdsharp
