#r "nuget: Generaptor.Library, 1.2.0"
open System

open Generaptor
open Generaptor.GitHubActions
open type Generaptor.GitHubActions.Commands
open type Generaptor.Library.Actions
open type Generaptor.Library.Patterns

let mainBranch = "master"

let linuxImage = "ubuntu-20.04"

let workflows = [
    workflow "main" [
        name "Main"
        onPushTo mainBranch
        onPushTags "v*"
        onPullRequestTo mainBranch
        onSchedule(day = DayOfWeek.Saturday)

        let pwsh name script =
            step(name = name, shell = "pwsh", run = script)

        let pwshWithResult name script producedFiles = [
            step(name = name, shell = "pwsh", run = script)
            for file in producedFiles do
                step(
                    name = $"{name}: verify produced file",
                    shell = "pwsh",
                    run = $"Test-Path -LiteralPath \"{file}\""
                )
        ]

        job "build-linux" [
            runsOn linuxImage

            step(name = "Checkout", uses = "actions/checkout@v4", options = Map.ofList [
                "submodules", "true"
            ])
            pwsh "Verify encoding" "./common/verify-encoding.ps1" []
            pwsh "Install" "./linux/install.ps1 -ForBuild" []
            pwsh "Build" "./linux/build.ps1"
            yield! pwshWithResult "Prepare artifact" "./linux/prepare-artifact.ps1" ["artifacts/libtdjson.so"]
            step(name = "Upload build result", uses = "actions/upload-artifact@v2", options = Map.ofList [
                "name", "tdlib.linux"
                "path", "artifacts/*"
            ])
        ]

        job "build-macos" [
            runsOn "macos-11"

            step(name = "Checkout", uses = "actions/checkout@v4", options = Map.ofList [
                "submodules", "true"
            ])
            pwsh "Verify encoding" "./common/verify-encoding.ps1"
            pwsh "Install" "./macos/install.ps1"
            pwsh "Build" "./macos/build.ps1"
            yield! pwshWithResult "Prepare artifact" "./macos/prepare-artifact.ps1" ["artifacts/libtdjson.dylib"]
            step(name = "Upload build result", uses = "actions/upload-artifact@v2", options = Map.ofList [
                "name", "tdlib.osx"
                "path", "artifacts/*"
            ])
        ]

        job "build-windows" [
            runsOn "windows-2019"

            step(name = "Checkout", uses = "actions/checkout@v4", options = Map.ofList [
                "submodules", "true"
            ])
            pwsh "Verify encoding" "./common/verify-encoding.ps1"
            pwsh "Install" "./windows/install.ps1"
            pwsh "Build" @"./windows/build.ps1 -VcpkgToolchain c:\vcpkg\scripts\buildsystems\vcpkg.cmake"
            yield! pwshWithResult "Prepare artifact" "./windows/prepare-artifact.ps1" ["artifacts/tdjson.dll"]
            step(name = "Upload build result", uses = "actions/upload-artifact@v2", options = Map.ofList [
                "name", "tdlib.windows"
                "path", "artifacts/*"
            ])
        ]

        job "test-linux" [
            needs "build-linux"
            runsOn linuxImage

            step(name = "Checkout", uses = "actions/checkout@v4", options = Map.ofList [
                "submodules", "true"
            ])
            pwsh "Verify encoding" "./common/verify-encoding.ps1" []
            pwsh "Install" "./linux/install.ps1 -ForBuild" []
            pwsh "Build" "./linux/build.ps1"
            yield! pwshWithResult "Prepare artifact" "./linux/prepare-artifact.ps1" ["artifacts/libtdjson.so"]
            step(name = "Upload build result", uses = "actions/upload-artifact@v2", options = Map.ofList [
                "name", "tdlib.linux"
                "path", "artifacts/*"
            ])
        ]
    ]
]

EntryPoint.Process fsi.CommandLineArgs workflows
