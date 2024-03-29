Changelog
=========

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

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
[Unreleased]: https://github.com/ForNeVeR/tdlib.native/compare/v1.8.21...HEAD
