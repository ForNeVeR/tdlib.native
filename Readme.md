tdlib.native [![NuGet](https://img.shields.io/nuget/v/tdlib.native.svg)](https://www.nuget.org/packages/tdlib.native/)
============

This is the project to pack [tdlib][] binaries for multiple common platforms.

How to build
------------

Push the tag to this repository. CI servers will do their job and upload the
artifacts to the [Releases][releases] page.

How to pack to NuGet
--------------------

Pack script requires NuGet 5 to be installed on a machine.

```console
$ pwsh ./pack.nuget.ps1
```

Then upload the `build/tdlib.native.<VERSION>.nupkg` to the NuGet server.

[releases]: releases

[tdlib]: https://github.com/tdlib/td
