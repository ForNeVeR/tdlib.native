tdlib.native
============

This is a project to pack [TDLib][tdlib] (the Telegram Database library) binaries built for several platforms:

- [MacOS 11][spec.macos] (x64) _(GitHub Actions image: `macos-11`)_
- [Ubuntu 18.04][spec.linux] (x64) _(Docker image: `ubuntu:18.04`)_
- [Windows Server 2019][spec.windows] (x64) _(GitHub Actions image: `windows-2019`)_

We aim to create a transparent process with no manual intervention, where every artifact is produced in a clean CI environment, and uploaded automatically.

Getting Started
---------------

- Install the latest package (all binaries packed for .NET SDK) from NuGet: [![NuGet](https://img.shields.io/nuget/v/tdlib.native.svg)][nuget]
- Download the latest binaries from the [Releases][releases] section

If using .NET, then you'll probably need to also install [tdsharp][], and then use the library through the provided API.

For other technologies or if you don't want to use tdsharp in .NET, you can just download the binaries and then use them in a manner your technology allows to use dynamically loaded libraries. Consult the [TDLib][tdlib] documentation for further directions.

Library Dependencies
--------------------

On Windows, [Microsoft Visual C++ Redistributable][cpp.redist] of version 2019 or higher is required by TDLib.

Documentation
-------------

- [Changelog][docs.changelog]
- [License][docs.license]
- [Maintainership][docs.maintainership]

[cpp.redist]: https://docs.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-160
[docs.changelog]: ./CHANGELOG.md
[docs.license]: ./LICENSE_1_0.txt
[docs.maintainership]: ./MAINTAINERSHIP.md
[nuget]: https://www.nuget.org/packages/tdlib.native/
[releases]: https://github.com/ForNeVeR/tdlib.native/releases
[spec.linux]: https://hub.docker.com/_/ubuntu
[spec.macos]: https://github.com/actions/runner-images/blob/main/images/macos/macos-11-Readme.md
[spec.windows]: https://github.com/actions/runner-images/blob/main/images/win/Windows2019-Readme.md
[tdlib]: https://github.com/tdlib/td
[tdsharp]: https://github.com/egramtel/tdsharp
