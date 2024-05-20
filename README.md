tdlib.native
============

This is a project to pack [TDLib][tdlib] (the Telegram Database library) binaries built for several platforms:

- [MacOS 11][spec.macos-11] (x86-64) _(GitHub Actions image: `macos-11`)_
- [MacOS 14][spec.macos-14] (AArch64) _(GitHub Actions image: `macos-14`)_
- [Ubuntu 20.04][spec.ubuntu-20.04] (x86-64) _(GitHub Actions image: `ubuntu-20.04`)_
- [Ubuntu 22.04][spec.ubuntu-22.04] (x86-64) _(GitHub Actions image: `ubuntu-22.04`)_
- [Windows Server 2019][spec.windows] (x86-64) _(GitHub Actions image: `windows-2019`)_

We aim to create a transparent process with no manual intervention, where every artifact is produced in a clean CI environment, and uploaded automatically.

Getting Started
---------------

- Install the latest package (all binaries packed for .NET SDK) from NuGet:

  | Platform                    | Package                                                                              |
  |-----------------------------|--------------------------------------------------------------------------------------|
  | Linux x86-64 (Ubuntu 20.04) | [![NuGet][badge.tdlib.native.linux-x64]][nuget.tdlib.native.linux-x64]               |
  | Linux x86-64 (Ubuntu 22.04) | [![NuGet][badge.tdlib.native.ubuntu-20.04-x64]][nuget.tdlib.native.ubuntu-20.04-x64] |
  | macOS x86-64                | [![NuGet][badge.tdlib.native.osx-x64]][nuget.tdlib.native.osx-x64]                   |
  | macOS AArch64               | [![NuGet][badge.tdlib.native.osx-arm64]][nuget.tdlib.native.osx-arm64]               |
  | Windows x86-64              | [![NuGet][badge.tdlib.native.windows-x64]][nuget.tdlib.native.windows-x64]           |
  | **All**                     | [![NuGet][badge.tdlib.native]][nuget.tdlib.native]                                   |

  **tdlib.native** package depends on latest versions of each other platform (i.e. all except the obsolete **Ubuntu 20.04**), so the resulting application will work on any supported platform.

- Download the latest binaries from the [Releases][releases] section

If using .NET, then you'll probably need to also install [tdsharp][], and then use the library through the provided API.

For other technologies or if you don't want to use tdsharp in .NET, you can just download the binaries and then use them in a manner your technology allows to use dynamically loaded libraries. Consult the [TDLib][tdlib] documentation for further directions.

Library Dependencies
--------------------

On **Ubuntu 20.04**, the TDLib version provided by tdlib.native is compiled against OpenSSL 1.1.

On **Ubuntu 22.04**, the TDLib version provided by tdlib.native is compiled against OpenSSL 3.0.

On **Windows**, [Microsoft Visual C++ Redistributable][cpp.redist] of version 2019 or higher is required by TDLib.

Documentation
-------------

- [Changelog][docs.changelog]
- [License][docs.license]
- [Contributing][docs.contributing]
- [Maintainership][docs.maintainership]

[badge.tdlib.native.linux-x64]: https://img.shields.io/nuget/v/tdlib.native.linux-x64?label=tdlib.native.linux-x64
[badge.tdlib.native.ubuntu-20.04-x64]: https://img.shields.io/nuget/v/tdlib.native.ubuntu-20.04-x64?label=tdlib.native.ubuntu-20.04-x64
[badge.tdlib.native.osx-arm64]: https://img.shields.io/nuget/v/tdlib.native.osx-arm64?label=tdlib.native.osx-arm64
[badge.tdlib.native.osx-x64]: https://img.shields.io/nuget/v/tdlib.native.osx-x64?label=tdlib.native.osx-x64
[badge.tdlib.native.windows-x64]: https://img.shields.io/nuget/v/tdlib.native.windows-x64?label=tdlib.native.windows-x64
[badge.tdlib.native]: https://img.shields.io/nuget/v/tdlib.native?label=tdlib.native
[cpp.redist]: https://docs.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-160
[docs.changelog]: ./CHANGELOG.md
[docs.contributing]: ./CONTRIBUTING.md
[docs.license]: ./LICENSE_1_0.txt
[docs.maintainership]: ./MAINTAINERSHIP.md
[nuget.tdlib.native.linux-x64]: https://www.nuget.org/packages/tdlib.native.linux-x64/
[nuget.tdlib.native.ubuntu-20.04-x64]: https://www.nuget.org/packages/tdlib.native.ubuntu-20.04-x64/
[nuget.tdlib.native.osx-arm64]: https://www.nuget.org/packages/tdlib.native.osx-arm64/
[nuget.tdlib.native.osx-x64]: https://www.nuget.org/packages/tdlib.native.osx-x64/
[nuget.tdlib.native.windows-x64]: https://www.nuget.org/packages/tdlib.native.windows-x64/
[nuget.tdlib.native]: https://www.nuget.org/packages/tdlib.native/
[releases]: https://github.com/ForNeVeR/tdlib.native/releases
[spec.ubuntu-20.04]: https://github.com/actions/runner-images/blob/main/images/ubuntu/Ubuntu2004-Readme.md
[spec.ubuntu-22.04]: https://github.com/actions/runner-images/blob/main/images/ubuntu/Ubuntu2204-Readme.md
[spec.macos-11]: https://github.com/actions/runner-images/blob/main/images/macos/macos-11-Readme.md
[spec.macos-14]: https://github.com/actions/runner-images/blob/main/images/macos/macos-14-Readme.md
[spec.windows]: https://github.com/actions/runner-images/blob/main/images/win/Windows2019-Readme.md
[tdlib]: https://github.com/tdlib/td
[tdsharp]: https://github.com/egramtel/tdsharp
