tdlib.native
============

This is a project to pack [TDLib][tdlib] (the Telegram Database library) binaries built for several platforms:

- [MacOS 10.15][spec.macos] (x64) _(GitHub Actions image: `macos-10.15`)_
- [Ubuntu 18.04][spec.linux] (x64) _(Docker image: `ubuntu:18.04`)_
- [Windows Server 2019][spec.windows] (x64) _(Appveyor image: `Visual Studio 2019`)_

Getting Started
---------------

- Install the latest package (all binaries packed for .NET SDK) from NuGet: [![NuGet](https://img.shields.io/nuget/v/tdlib.native.svg)][nuget]
- Download the latest binaries from the [Releases][releases] section

If using .NET, then you'll probably need to also install [tdsharp][], and then use the library through the provided API.

For other technologies or if you don't want to use tdsharp in .NET, you can just download the binaries and then use them in a manner your technology allows to use dynamically loaded libraries. Consult the [TDLib][tdlib] documentation for further directions.

Documentation
-------------

- [License][docs.license]
- [Maintainership][docs.maintainership]

[docs.license]: ./LICENSE_1_0.txt
[docs.maintainership]: ./MAINTAINERSHIP.md
[nuget]: https://www.nuget.org/packages/tdlib.native/
[releases]: https://github.com/ForNeVeR/tdlib.native/releases
[spec.linux]: https://hub.docker.com/_/ubuntu
[spec.macos]: https://github.com/actions/virtual-environments/blob/main/images/macos/macos-10.15-Readme.md
[spec.windows]: https://www.appveyor.com/docs/windows-images-software/
[tdlib]: https://github.com/tdlib/td
[tdsharp]: https://github.com/egramtel/tdsharp
