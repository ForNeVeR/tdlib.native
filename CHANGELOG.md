<!--
SPDX-FileCopyrightText: 2022-2026 tdlib.native contributors <https://github.com/ForNeVeR/tdlib.native>

SPDX-License-Identifier: BSL-1.0
-->

Changelog
=========

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [1.8.60] - 2026-01-10
### Changed
- Update to [TDLib v1.8.60](https://github.com/ForNeVeR/tdlib-versioned/releases/tag/tdlib%2Fv1.8.60).

## [1.8.59] - 2025-12-29
### Changed
- Update to [TDLib v1.8.59](https://github.com/ForNeVeR/tdlib-versioned/releases/tag/tdlib%2Fv1.8.59).

## [1.8.58] - 2025-12-07
### Changed
- Update to [TDLib v1.8.58](https://github.com/ForNeVeR/tdlib-versioned/releases/tag/tdlib%2Fv1.8.58).

## [1.8.56.1] - 2025-11-16
### Removed
- **(Breaking change!)** The package for Ubuntu 20.04 (OpenSSL v1.1) is no longer maintained nor updated (because [the corresponding GitHub runner is no longer available](https://github.com/actions/runner-images/issues/11101)).
- [#132](https://github.com/ForNeVeR/tdlib.native/issues/132): removed a duplicated file `libtdjson.so.1.8.45` from the Linux packages. Only the normal `.so` now remains. Thanks to @Jackhammer88!

### Added
- New AArch64 packages for Windows and Linux (Ubuntu-22.04-based).

### Changed
- Update to [TDLib v1.8.56.1](https://github.com/ForNeVeR/tdlib-versioned/releases/tag/tdlib%2Fv1.8.56.1).
- x86-64 builds for macOS are now built against macOS 15, not macOS 13.

## [1.8.45] - 2025-02-14
### Changed
- We switched the builds for x86-64 macOS to macOS 13 image (from macOS 11).
- Update to [TDLib v1.8.45](https://github.com/tdlib/td/tree/721300bcb4d0f2114505712f4dc6350af1ce1a09).

## [1.8.29] - 2024-05-28
### Changed
- Update to [TDLib v1.8.29](https://github.com/tdlib/td/tree/fd3154b28727df9e66423d64168fab1202d8c849).

## [1.8.21.2] - 2024-05-27
### Fixed
- The main package **tdlib.native** now depends on the following platform packages:
  - **tdlib.native.linux-x64** (Ubuntu 22.04),
  - **tdlib.native.osx-arm64** (macOS AArch64),
  - **tdlib.native.osx-x64** (macOS x86-64),
  - **tdlib.native.win-x64** (Windows x86-64).

## [1.8.21.1] - 2024-05-26 [YANKED]
**Note** that this version has been unlisted from nuget.org because the main package **tdlib.native** was prepared incorrectly: its dependencies on the platform packages weren't set.

### Changed
- **(Breaking change!)** The main package is now built on Ubuntu 22.04 and requires OpenSSL v3. The package for Ubuntu 20.04 (the previous default) is now available separately as `tdlib.native.ubuntu-20.04-x64`.

  Consult the package documentation to see the current versions and layout.
- **(Breaking change!)** The platform-dependent artifacts have been extracted to separate packages. The main package no longer includes any executable files, and just depends on the latest versions of the platform-dependent packages. The new packages:
  - **tdlib.native.linux-x64** (Ubuntu 22.04),
  - **tdlib.native.ubuntu-20.04-x64** (Ubuntu 20.04),
  - **tdlib.native.osx-arm64** (macOS AArch64),
  - **tdlib.native.osx-x64** (macOS x86-64),
  - **tdlib.native.win-x64** (Windows x86-64).

  You may directly depend on them as needed.
- (Technically a _breaking change_, though it's not expected to actually break anything.) The Linux artifacts no longer include `.a` files.

### Added
- Support for AArch64 versions of macOS.
- Support for Ubuntu 22.04 (OpenSSL v3).

## [1.8.21] - 2023-11-26
### Changed
- Update to [TDLib v1.8.21](https://github.com/tdlib/td/tree/07c1d53a6d3cb1fad58d2822e55eef6d57363581).
- **(Breaking change!)** The package now only supports macOS 11, since macOS 10 is out of support by the OS (and CI infrastructure) vendor.
- **(Breaking change!)** The package now only supports Ubuntu 20.04 and newer, since Ubuntu 18.04 has reached EOL.

## [1.8.12] - 2023-03-18
### Changed
- Update TDLib [to a commit from v1.8.12](https://github.com/tdlib/td/tree/70bee089d492437ce931aa78446d89af3da182fc).

## [1.8.9] - 2022-12-09
### Changed
- Update TDLib [to a commit from v1.8.9](https://github.com/tdlib/td/tree/29752073cf2fe586ecefe572d3821a8cf853fab5).

## [1.8.1] - 2022-04-10
### Changed
- Update TDLib [to a commit from v1.8.1](https://github.com/tdlib/td/tree/1e1ab5d1b0e4811e6d9e1584a82da08448d0cada).

## [1.7.9.1] - 2021-12-09
### Changed
- Linux: build on Ubuntu 18.04

### Fixed
- [The issue with GLIBCXX_3.4.26 on Ubuntu 18.04](https://github.com/ForNeVeR/tdlib.native/issues/51).

## [1.7.9] - 2021-12-04
### Changed
- Update TDLib [to the latest commit of v1.7.9](https://github.com/tdlib/td/tree/8d7bda00a535d1eda684c3c8802e85d69c89a14a).

### Fixed
- This fixes [the issue](https://github.com/tdlib/td/issues/1758) with `UPDATE_APP_TO_LOGIN` which has started occuring after a server update.

## [1.7.0] - 2021-08-03
### Changed
- Update to TDLib v1.7.0.

## [1.6.0] - 2020-02-16
### Changed
- Update to TDLib v1.6.0.
- Windows: build using Visual Studio 2019.

## [1.3.0] - 2018-09-09
### Changed
- Pack for NuGet in a way compatible with SDK-based projects.
- Update to TDLib v1.3.0.

## [1.2.0] - 2018-09-04

Initial release supporting TDLib v1.2.0.

[1.2.0]: https://github.com/ForNeVeR/tdlib.native/releases/tag/v1.2.0
[1.3.0]: https://github.com/ForNeVeR/tdlib.native/compare/v1.2.0...v1.3.0
[1.6.0]: https://github.com/ForNeVeR/tdlib.native/compare/v1.3.0...v1.6.0
[1.7.0]: https://github.com/ForNeVeR/tdlib.native/compare/v1.6.0...v1.7.0
[1.7.9]: https://github.com/ForNeVeR/tdlib.native/compare/v1.7.0...v1.7.9
[1.7.9.1]: https://github.com/ForNeVeR/tdlib.native/compare/v1.7.9...v1.7.9.1
[1.8.1]: https://github.com/ForNeVeR/tdlib.native/compare/v1.7.9.1...v1.8.1
[1.8.9]: https://github.com/ForNeVeR/tdlib.native/compare/v1.8.1...v1.8.9
[1.8.12]: https://github.com/ForNeVeR/tdlib.native/compare/v1.8.9...v1.8.12
[1.8.21]: https://github.com/ForNeVeR/tdlib.native/compare/v1.8.12...v1.8.21
[1.8.21.1]: https://github.com/ForNeVeR/tdlib.native/compare/v1.8.21...v1.8.21.1
[1.8.21.2]: https://github.com/ForNeVeR/tdlib.native/compare/v1.8.21.1...v1.8.21.2
[1.8.29]: https://github.com/ForNeVeR/tdlib.native/compare/v1.8.21.2...v1.8.29
[1.8.45]: https://github.com/ForNeVeR/tdlib.native/compare/v1.8.29...v1.8.45
[1.8.56.1]: https://github.com/ForNeVeR/tdlib.native/compare/v1.8.45...v1.8.56.1
[1.8.58]: https://github.com/ForNeVeR/tdlib.native/compare/v1.8.56.1...v1.8.58
[1.8.59]: https://github.com/ForNeVeR/tdlib.native/compare/v1.8.58...v1.8.59
[1.8.60]: https://github.com/ForNeVeR/tdlib.native/compare/v1.8.59...v1.8.60
[Unreleased]: https://github.com/ForNeVeR/tdlib.native/compare/v1.8.60...HEAD
