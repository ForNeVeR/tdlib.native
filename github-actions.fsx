#r "nuget: Generaptor.Library, 1.5.0"
open System

open System.IO
open Generaptor
open Generaptor.GitHubActions
open type Generaptor.GitHubActions.Commands
open type Generaptor.Library.Actions
open type Generaptor.Library.Patterns

let mainBranch = "master"

let macOs11 = "macos-11"
let macOs14 = "macos-14"
let ubuntu20_04 = "ubuntu-20.04"
let ubuntuLatest = "ubuntu-latest"
let windows2019 = "windows-2019"

let pwsh name script =
    step(name = name, shell = "pwsh", run = script)

let pwshWithResult name script producedFiles = [
    step(name = name, shell = "pwsh", run = script)
    for file in producedFiles do
        step(
            name = $"{name}: verify produced file",
            shell = "pwsh",
            run = $"if (!(Test-Path -LiteralPath '{file}')) {{ throw 'File not found' }}"
        )
]

let prepareArtifacts os resultFileName =
    pwshWithResult "Prepare artifact" $"./{os}/prepare-artifacts.ps1" [resultFileName]

let buildJobName platform arch = $"build-{platform}-{arch}"

let checkoutWithSubmodules = step(name = "Checkout", uses = "actions/checkout@v4", options = Map.ofList [
    "submodules", "true"
])

let platformToDotNet = function
    | "linux" -> "linux"
    | "macos" -> "osx"
    | "windows" -> "win"
    | other -> failwith $"Unknown platform {other}"

let archToDotNet = function
    | "x86-64" -> "x64"
    | "aarch64" -> "arm64"
    | other -> failwith $"Unknown architecture {other}"

let platformBuildArtifactDirectory platform architecture =
    $"./build/runtimes/{platformToDotNet platform}-{archToDotNet architecture}/native"

let downloadArtifact platform architecture =
    step(
        name = $"Download artifact: {platform}.{architecture}",
        uses = "actions/download-artifact@v4",
        options = Map.ofList [
            "name", $"tdlib.{platform}.{architecture}"
            "path", platformBuildArtifactDirectory platform architecture
        ]
    )

let setUpDotNetSdk = step(name = "Set up .NET SDK", uses = "actions/setup-dotnet@v4", options = Map.ofList [
    "dotnet-version", "7.0.x"
])

let testEnv =
    [
        "DOTNET_NOLOGO", "1"
        "DOTNET_CLI_TELEMETRY_OPTOUT", "1"
        "NUGET_PACKAGES", "${{ github.workspace }}/.github/nuget-packages"
        "PACKAGE_VERSION_BASE", "1.8.21"
    ] |> Seq.map(fun (k, v) -> setEnv k v)

type Workflows =
    static member BuildArtifacts(
        platform: string,
        arch: string,
        artifactFileName: string,
        ?installScript: string,
        ?buildScriptArgs: string
    ) = [
        pwsh "Generate cache key" "./common/Test-UpToDate.ps1 -GenerateCacheKey"
        step(
             name = "Cache artifacts",
             uses = "actions/cache@v4",
             options = Map.ofList [
                "path", "artifacts"
                "key", $"{platform}.{arch}." + "${{ hashFiles('.github/cache-key.json') }}"
            ]
        )
        yield! installScript |> Option.toList |> Seq.map(pwsh "Install")
        pwsh "Build" (
            String.concat " " [
                $"./{platform}/build.ps1"
                yield! buildScriptArgs |> Option.toList
            ]
        )
        yield! prepareArtifacts platform $"artifacts/{artifactFileName}"
        step(name = "Upload build result", uses = "actions/upload-artifact@v4", options = Map.ofList [
            "name", $"tdlib.{platform}.{arch}"
            "path", "artifacts/*"
        ])
    ]

    static member BuildJob(
        image: string,
        platform: string,
        arch: string,
        artifactFileName: string,
        ?installScript: string,
        ?buildScriptArgs: string
    ) = job (buildJobName platform arch) [
        runsOn image
        checkoutWithSubmodules
        yield! Workflows.BuildArtifacts(
            platform = platform,
            arch = arch,
            artifactFileName = artifactFileName,
            ?installScript = installScript,
            ?buildScriptArgs = buildScriptArgs
        )
    ]

    static member TestJob(
        image: string,
        platform: string,
        arch: string,
        testArgs: string,
        ?installScript: string,
        ?afterDownloadSteps: JobCreationCommand seq
    ) = job $"test-{platform}-{arch}" [
        needs(buildJobName platform arch)
        runsOn image
        yield! testEnv

        checkoutWithSubmodules
        yield! installScript |> Option.toList |> Seq.map(pwsh "Install")
        downloadArtifact platform arch

        yield! afterDownloadSteps |> Option.toList |> Seq.collect id

        let rid = $"{platformToDotNet platform}-{archToDotNet arch}"

        setUpDotNetSdk
        pwsh "Pack NuGet" (
            $"dotnet pack tdlib.native.{rid}.proj" +
            " -p:Version=${{ env.PACKAGE_VERSION_BASE }}-preview --output build"
        )
        // TODO[#64]: Add ${{ github.run_id }} as a patch version
        step(name = "NuGet cache", uses = "actions/cache@v4", options = Map.ofList [
            "path", "${{ env.NUGET_PACKAGES }}"
            "key", "${{ runner.os }}.nuget.${{ hashFiles('tdsharp/**/*.csproj') }}"
        ])
        pwsh "Test" $"./common/test.ps1 -PackageName tdlib.native.{rid} {testArgs}"
    ]

module Platform =
    let Linux = "linux"
    let MacOS = "macos"
    let Windows = "windows"

module Arch =
    let AArch64 = "aarch64"
    let X86_64 = "x86-64"

let workflows = [
    workflow "main" [
        name "Main"
        onPushTo mainBranch
        onPushTags "v*"
        onPullRequestTo mainBranch
        onSchedule(day = DayOfWeek.Saturday)
        onWorkflowDispatch

        Workflows.BuildJob(
            image = ubuntu20_04,
            platform = Platform.Linux,
            arch = Arch.X86_64,
            installScript = "./linux/install.ps1 -ForBuild",
            artifactFileName = "libtdjson.so"
        )

        Workflows.BuildJob(
            image = macOs14,
            platform = Platform.MacOS,
            arch = Arch.AArch64,
            installScript = "./macos/install.ps1",
            artifactFileName = "libtdjson.dylib"
        )

        Workflows.BuildJob(
            image = macOs11,
            platform = Platform.MacOS,
            arch = Arch.X86_64,
            installScript = "./macos/install.ps1",
            artifactFileName = "libtdjson.dylib"
        )

        Workflows.BuildJob(
            image = windows2019,
            platform = Platform.Windows,
            arch = Arch.X86_64,
            buildScriptArgs = @"-VcpkgToolchain c:\vcpkg\scripts\buildsystems\vcpkg.cmake",
            artifactFileName = "tdjson.dll"
        )

        let downloadAndRepackArtifact platform arch = [
            downloadArtifact platform arch
            let dir = platformBuildArtifactDirectory platform arch
            pwsh
                $"Archive artifact for platform: {platform}.{arch}"
                $"Set-Location {dir} && zip -r $env:GITHUB_WORKSPACE/tdlib.{platform}.{arch}.zip *"
        ]

        Workflows.TestJob(
            image = ubuntu20_04,
            platform = Platform.Linux,
            arch = Arch.X86_64,
            installScript = "./linux/install.ps1 -ForTests",
            testArgs = "-NuGet $env:GITHUB_WORKSPACE/tools/nuget.exe -UseMono"
        )

        Workflows.TestJob(
            image = macOs14,
            platform = Platform.MacOS,
            arch = Arch.AArch64,
            testArgs = "-NuGet nuget"
        )

        Workflows.TestJob(
            image = macOs11,
            platform = Platform.MacOS,
            arch = Arch.X86_64,
            testArgs = "-NuGet nuget"
        )

        Workflows.TestJob(
            image = windows2019,
            platform = Platform.Windows,
            arch = Arch.X86_64,
            testArgs = "-NuGet nuget",
            afterDownloadSteps = [
                step(name = "Cache downloads for Windows", uses = "actions/cache@v4", options = Map.ofList [
                    "path", "build/downloads"
                    "key", "${{ hashFiles('windows/install.ps1') }}"
                ])
                pwsh "Install dependencies" "./windows/install.ps1"
                pwsh "Windows-specific testing" "./windows/test.ps1"
            ]
        )

        job "release" [
            runsOn ubuntuLatest
            yield! [
                buildJobName Platform.Linux Arch.X86_64
                buildJobName Platform.MacOS Arch.AArch64
                buildJobName Platform.MacOS Arch.X86_64
                buildJobName Platform.Windows Arch.X86_64
            ] |> Seq.map needs

            yield! testEnv
            checkout

            yield! downloadAndRepackArtifact Platform.Linux Arch.X86_64
            yield! downloadAndRepackArtifact Platform.MacOS Arch.AArch64
            yield! downloadAndRepackArtifact Platform.MacOS Arch.X86_64
            yield! downloadAndRepackArtifact Platform.Windows Arch.X86_64

            setUpDotNetSdk

            step(
                name = "Read version from ref",
                id = "version",
                shell = "pwsh",
                run = "\"version=$(if ($env:GITHUB_REF.StartsWith('refs/tags/v')) { $env:GITHUB_REF -replace '^refs/tags/v', '' } else { \"$env:PACKAGE_VERSION_BASE-preview\" })\" >> $env:GITHUB_OUTPUT"
            )

            step(
                name = "Prepare the release notes",
                uses = "ForNeVeR/ChangelogAutomation.action@v1",
                options = Map.ofList [
                    "input", "./CHANGELOG.md"
                    "output", "release-notes.md"
                    "format", "Markdown"
                ]
            )

            let packPackageFor platform architecture =
                pwsh
                    $"Pack NuGet package: {platform}.{architecture}"
                    (
                        $"dotnet pack tdlib.native.{platformToDotNet platform}-{archToDotNet architecture}.proj" +
                        " -p:Version=${{ steps.version.outputs.version }} --output build"
                    )

            packPackageFor Platform.Linux Arch.X86_64
            packPackageFor Platform.MacOS Arch.AArch64
            packPackageFor Platform.MacOS Arch.X86_64
            packPackageFor Platform.Windows Arch.X86_64

            pwsh "Install dependencies" "./linux/install.ps1 -ForPack"
            pwsh
                "Prepare NuGet source"
                "common/New-NuGetSource.ps1 -UseMono -NuGet $env:GITHUB_WORKSPACE/tools/nuget.exe"
            pwsh
                "Pack NuGet package: main"
                "dotnet pack tdlib.native.proj -p:Version=${{ steps.version.outputs.version }} --output build"

            step(name = "Upload NuGet packages", uses = "actions/upload-artifact@v4", options = Map.ofList [
                "name", "tdlib.nuget"
                "path", "./build/*.nupkg"
            ])

            yield! ifCalledOnTagPush [
                step(
                    name = "Create release",
                    id = "release",
                    uses = "actions/create-release@v1",
                    env = Map.ofList [
                        "GITHUB_TOKEN", "${{ secrets.GITHUB_TOKEN }}"
                    ],
                    options = Map.ofList [
                        "tag_name", "${{ github.ref }}"
                        "release_name", "v${{ steps.version.outputs.version }}"
                        "body_path", "./release-notes.md"
                    ]
                )

                let uploadArchive platform architecture =
                    step(
                        name = $"Upload archive: {platform}.{architecture}",
                        uses = "actions/upload-release-asset@v1",
                        env = Map.ofList [
                            "GITHUB_TOKEN", "${{ secrets.GITHUB_TOKEN }}"
                        ],
                        options = Map.ofList [
                            "upload_url", "${{ steps.release.outputs.upload_url }}"
                            "asset_name", $"tdlib.{platform}.{architecture}.zip"
                            "asset_path", $"./tdlib.{platform}.{architecture}.zip"
                            "asset_content_type", "application/zip"
                        ]
                    )

                uploadArchive Platform.Linux Arch.X86_64
                uploadArchive Platform.MacOS Arch.AArch64
                uploadArchive Platform.MacOS Arch.X86_64
                uploadArchive Platform.Windows Arch.X86_64

                let uploadPackage fileName =
                    step(
                        name = $"Upload NuGet package: {fileName}",
                        uses = "actions/upload-release-asset@v1",
                        env = Map.ofList [
                            "GITHUB_TOKEN", "${{ secrets.GITHUB_TOKEN }}"
                        ],
                        options = Map.ofList [
                            "upload_url", "${{ steps.release.outputs.upload_url }}"
                            "asset_name", fileName
                            "asset_path", $"./build/{fileName}"
                            "asset_content_type", "application/zip"
                        ]
                    )

                let uploadPlatformPackage platform architecture =
                    uploadPackage (
                        $"tdlib.native.{platformToDotNet platform}-{archToDotNet architecture}." +
                            "${{ steps.version.outputs.version }}.nupkg"
                    )

                uploadPlatformPackage Platform.Linux Arch.X86_64
                uploadPlatformPackage Platform.MacOS Arch.AArch64
                uploadPlatformPackage Platform.MacOS Arch.X86_64
                uploadPlatformPackage Platform.Windows Arch.X86_64
                uploadPackage "tdlib.native.${{ steps.version.outputs.version }}.nupkg"
            ]

            let pushPackage (fileName: string) =
                step(
                    name = $"Push {Path.GetFileNameWithoutExtension fileName} to nuget.org",
                    condition = "github.event_name == 'push' && contains(github.ref, 'refs/tags/')",
                    run = "dotnet nuget push --source https://api.nuget.org/v3/index.json" +
                        " --api-key ${{ secrets.NUGET_KEY }}" +
                        $" ./build/{fileName}"
                )

            let pushPlatformPackage platform architecture =
                pushPackage (
                    $"tdlib.native.{platformToDotNet platform}-{archToDotNet architecture}." +
                        "${{ steps.version.outputs.version }}.nupkg"
                )

            pushPlatformPackage Platform.Linux Arch.X86_64
            pushPlatformPackage Platform.MacOS Arch.AArch64
            pushPlatformPackage Platform.MacOS Arch.X86_64
            pushPlatformPackage Platform.Windows Arch.X86_64
            pushPackage "tdlib.native.${{ steps.version.outputs.version }}.nupkg"
        ]

        job "verify-encoding" [
            runsOn ubuntuLatest
            checkout
            pwsh "Verify encoding" "./common/verify-encoding.ps1"
        ]
    ]
]

EntryPoint.Process fsi.CommandLineArgs workflows
