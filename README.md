<!--
SPDX-FileCopyrightText: 2018-2025 Friedrich von Never <friedrich@fornever.me>

SPDX-License-Identifier: BSL-1.0
-->

tdlib.native
============

This is a project to pack [TDLib][tdlib] (the Telegram Database library) binaries built for several platforms:

- [Ubuntu 22.04][spec.ubuntu-22.04-arm] (AArch64) _(GitHub Actions image: `ubuntu-22.04-arm`)_
- [Ubuntu 22.04][spec.ubuntu-22.04] (x86-64) _(GitHub Actions image: `ubuntu-22.04`)_
- [Windows 11][spec.windows-arm] (AArch64) _(GitHub Actions image: `windows-11-arm`)_
- [Windows Server 2022][spec.windows] (x86-64) _(GitHub Actions image: `windows-2022`)_
- [macOS 14][spec.macos-14] (AArch64) _(GitHub Actions image: `macos-14`)_
- [macOS 15][spec.macos-15] (x86-64) _(GitHub Actions image: `macos-15`)_

We aim to create a transparent process with no manual intervention, where every artifact is produced in a clean CI environment and uploaded automatically.

Getting Started
---------------

- Install the latest package (all binaries packed for .NET SDK) from NuGet:

  | Platform                     | Package                                                                    |
  |------------------------------|----------------------------------------------------------------------------|
  | Linux AArch64 (Ubuntu 22.04) | [![NuGet][badge.tdlib.native.linux-arm64]][nuget.tdlib.native.linux-arm64] |
  | Linux x86-64 (Ubuntu 22.04)  | [![NuGet][badge.tdlib.native.linux-x64]][nuget.tdlib.native.linux-x64]     |
  | Windows AArch64              | [![NuGet][badge.tdlib.native.win-arm64]][nuget.tdlib.native.win-arm64]     |
  | Windows x86-64               | [![NuGet][badge.tdlib.native.win-x64]][nuget.tdlib.native.win-x64]         |
  | macOS AArch64                | [![NuGet][badge.tdlib.native.osx-arm64]][nuget.tdlib.native.osx-arm64]     |
  | macOS x86-64                 | [![NuGet][badge.tdlib.native.osx-x64]][nuget.tdlib.native.osx-x64]         |
  | **All**                      | [![NuGet][badge.tdlib.native]][nuget.tdlib.native]                         |

  The **tdlib.native** package depends on the latest versions of each platform, so the resulting application will work on any supported platform.

- Download the latest binaries from the [Releases][releases] section

If using .NET, then you'll probably need to also install [tdsharp][], and then use the library through the provided API.

For other technologies or if you don't want to use tdsharp in .NET, you can just download the binaries and then use them in a manner your technology allows to use dynamically loaded libraries. Consult the [TDLib][tdlib] documentation for further directions.

Library Dependencies
--------------------
This package doesn't bundle certain dependencies, and they are expected to be provided by the user's environment.

On **Ubuntu 22.04**, the TDLib version provided by tdlib.native is compiled against OpenSSL 3.0.

On **macOS**, the TDLib version provided by tdlib.native is compiled against OpenSSL 3.0.

On **Windows**, [Microsoft Visual C++ Redistributable][cpp.redist] of version 2019 or higher is required by TDLib.

Documentation
-------------

- [Changelog][docs.changelog]
- [Contributing][docs.contributing]
- [Maintainership][docs.maintainership]

License
-------
The project is distributed under the terms of [the BSL-1.0 license][docs.license], the same as TDLib itself.

The license indication in the project's sources is compliant with the [REUSE specification v3.3][reuse.spec]. Note this statement doesn't affect TDLib itself: TDLib and related projects don't have to follow this specification.

[badge.tdlib.native.linux-arm64]: https://img.shields.io/nuget/v/tdlib.native.linux-arm64?label=tdlib.native.linux-arm64
[badge.tdlib.native.linux-x64]: https://img.shields.io/nuget/v/tdlib.native.linux-x64?label=tdlib.native.linux-x64
[badge.tdlib.native.osx-arm64]: https://img.shields.io/nuget/v/tdlib.native.osx-arm64?label=tdlib.native.osx-arm64
[badge.tdlib.native.osx-x64]: https://img.shields.io/nuget/v/tdlib.native.osx-x64?label=tdlib.native.osx-x64
[badge.tdlib.native.win-arm64]: https://img.shields.io/nuget/v/tdlib.native.win-arm64?label=tdlib.native.win-arm64
[badge.tdlib.native.win-x64]: https://img.shields.io/nuget/v/tdlib.native.win-x64?label=tdlib.native.win-x64
[badge.tdlib.native]: https://img.shields.io/nuget/v/tdlib.native?label=tdlib.native
[cpp.redist]: https://docs.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-160
[docs.changelog]: ./CHANGELOG.md
[docs.contributing]: ./CONTRIBUTING.md
[docs.license]: ./LICENSE.txt
[docs.maintainership]: ./MAINTAINERSHIP.md
[nuget.tdlib.native.linux-arm64]: https://www.nuget.org/packages/tdlib.native.linux-arm64/
[nuget.tdlib.native.linux-x64]: https://www.nuget.org/packages/tdlib.native.linux-x64/
[nuget.tdlib.native.osx-arm64]: https://www.nuget.org/packages/tdlib.native.osx-arm64/
[nuget.tdlib.native.osx-x64]: https://www.nuget.org/packages/tdlib.native.osx-x64/
[nuget.tdlib.native.win-arm64]: https://www.nuget.org/packages/tdlib.native.win-arm64/
[nuget.tdlib.native.win-x64]: https://www.nuget.org/packages/tdlib.native.win-x64/
[nuget.tdlib.native]: https://www.nuget.org/packages/tdlib.native/
[releases]: https://github.com/ForNeVeR/tdlib.native/releases
[reuse.spec]: https://reuse.software/spec-3.3/
[reuse]: https://reuse.software/
[spec.macos-14]: https://github.com/actions/runner-images/blob/main/images/macos/macos-14-Readme.md
[spec.macos-15]: https://github.com/actions/runner-images/blob/main/images/macos/macos-15-Readme.md
[spec.ubuntu-22.04-arm]: https://github.com/actions/partner-runner-images/blob/main/images/arm-ubuntu-22-image.md
[spec.ubuntu-22.04]: https://github.com/actions/runner-images/blob/main/images/ubuntu/Ubuntu2204-Readme.md
[spec.windows-arm]: https://github.com/actions/partner-runner-images/blob/main/images/arm-windows-11-image.md
[spec.windows]: https://github.com/actions/runner-images/blob/main/images/windows/Windows2022-Readme.md
[tdlib]: https://github.com/tdlib/td
[tdsharp]: https://github.com/egramtel/tdsharp
