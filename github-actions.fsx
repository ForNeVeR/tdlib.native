#r "nuget: Generaptor.Library, 1.5.0"

open System
open System.IO
open Generaptor
open Generaptor.GitHubActions
open type Generaptor.GitHubActions.Commands
open type Generaptor.Library.Actions
open type Generaptor.Library.Patterns

let mainBranch = "master"

let macOs13 = "macos-13"
let macOs14 = "macos-14"
let ubuntu20_04 = "ubuntu-20.04"
let ubuntu22_04 = "ubuntu-22.04"
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

let checkoutWithSubmodules = step(name = "Checkout", uses = "actions/checkout@v4", options = Map.ofList [
    "submodules", "true"
])

module Platform =
    let [<Literal>] Ubuntu20_04 = "ubuntu-20.04"
    let [<Literal>] Ubuntu22_04 = "ubuntu-22.04"
    let [<Literal>] MacOS = "macos"
    let [<Literal>] Windows = "windows"

module Arch =
    let [<Literal>] AArch64 = "aarch64"
    let [<Literal>] X86_64 = "x86-64"

module Names =
    let private platformToDotNet = function
    | Platform.Ubuntu20_04 -> "linux"
    | Platform.Ubuntu22_04 -> "linux"
    | Platform.MacOS -> "osx"
    | Platform.Windows -> "win"
    | other -> failwith $"Unknown platform {other}"

    let archToDotNet = function
    | Arch.AArch64 -> "arm64"
    | Arch.X86_64 -> "x64"
    | other -> failwith $"Unknown architecture {other}"

    let package platform arch =
        let platformPart =
            match platform with
            | Platform.Ubuntu20_04 -> "ubuntu-20.04"
            | other -> platformToDotNet other

        $"tdlib.native.{platformPart}-{archToDotNet arch}"

    let ciArtifact platform arch = $"tdlib.native.{platform}.{arch}"
    let packageInputDirectory platform arch =
        $"build/{package platform arch}/runtimes/{platformToDotNet platform}-{archToDotNet arch}/native"

    let buildJob (platform: string) arch = $"build-{platform.Replace('.', '-')}-{arch}"
    let testJob (platform: string) arch = $"test-{platform.Replace('.', '-')}-{arch}"
    let os = function
    | Platform.Ubuntu20_04 -> "linux"
    | Platform.Ubuntu22_04 -> "linux"
    | Platform.MacOS -> "macos"
    | Platform.Windows -> "windows"
    | other -> failwith $"Unknown platform: {other}."

let prepareArtifacts platform resultFileName =
    let os = Names.os platform
    pwshWithResult "Prepare artifact" $"./{os}/prepare-artifacts.ps1" [resultFileName]

let downloadArtifact platform arch =
    step(
        name = $"Download artifact: {platform}.{arch}",
        uses = "actions/download-artifact@v4",
        options = Map.ofList [
            "name", Names.ciArtifact platform arch
            "path", Names.packageInputDirectory platform arch
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
        "PACKAGE_VERSION_BASE", "1.8.30"
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
                $"./{Names.os platform}/build.ps1"
                yield! buildScriptArgs |> Option.toList
            ]
        )
        yield! prepareArtifacts platform $"artifacts/{artifactFileName}"
        step(name = "Upload build result", uses = "actions/upload-artifact@v4", options = Map.ofList [
            "name", Names.ciArtifact platform arch
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
    ) = job (Names.buildJob platform arch) [
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
    ) = job (Names.testJob platform arch) [
        needs(Names.buildJob platform arch)
        runsOn image
        yield! testEnv

        checkoutWithSubmodules
        yield! installScript |> Option.toList |> Seq.map(pwsh "Install")
        downloadArtifact platform arch

        yield! afterDownloadSteps |> Option.toList |> Seq.collect id

        setUpDotNetSdk
        pwsh "Pack NuGet" (
            $"dotnet pack {Names.package platform arch}.proj" +
            " -p:Version=${{ env.PACKAGE_VERSION_BASE }}-test --output build"
        )
        step(name = "NuGet cache", uses = "actions/cache@v4", options = Map.ofList [
            "path", "${{ env.NUGET_PACKAGES }}"
            "key", "${{ runner.os }}.nuget.${{ hashFiles('tdsharp/**/*.csproj') }}"
        ])
        pwsh "Test" $"./common/test.ps1 -PackageName {Names.package platform arch} {testArgs}"
    ]

let workflows = [
    workflow "main" [
        name "Main"
        onPushTo mainBranch
        onPushTags "v*"
        onPullRequestTo mainBranch
        onSchedule(day = DayOfWeek.Monday)
        onWorkflowDispatch

        Workflows.BuildJob(
            image = ubuntu20_04,
            platform = Platform.Ubuntu20_04,
            arch = Arch.X86_64,
            installScript = "./linux/install.ps1 -ForBuild",
            artifactFileName = "libtdjson.so"
        )

        Workflows.BuildJob(
            image = ubuntu22_04,
            platform = Platform.Ubuntu22_04,
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
            image = macOs13,
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
            let dir = Names.packageInputDirectory platform arch
            pwsh
                $"Archive artifact for platform: {platform}.{arch}"
                $"Set-Location {dir} && zip -r $env:GITHUB_WORKSPACE/{Names.ciArtifact platform arch}.zip *"
        ]

        let testLinuxDependencies platform arch =
            pwsh "Verify library dependencies" (
                "./linux/Test-Dependencies.ps1" +
                $" -Platform {platform}" +
                $" -PackageName {Names.package platform arch}"
            )

        Workflows.TestJob(
            image = ubuntu20_04,
            platform = Platform.Ubuntu20_04,
            arch = Arch.X86_64,
            installScript = "./linux/install.ps1 -ForTests",
            testArgs = "-NuGet $env:GITHUB_WORKSPACE/tools/nuget.exe -UseMono",
            afterDownloadSteps = [
                testLinuxDependencies Platform.Ubuntu20_04 Arch.X86_64
            ]
        )

        Workflows.TestJob(
            image = ubuntu22_04,
            platform = Platform.Ubuntu22_04,
            arch = Arch.X86_64,
            installScript = "./linux/install.ps1 -ForTests",
            testArgs = "-NuGet $env:GITHUB_WORKSPACE/tools/nuget.exe -UseMono",
            afterDownloadSteps = [
                testLinuxDependencies Platform.Ubuntu22_04 Arch.X86_64
            ]
        )

        let testMacOsDependencies platform arch =
            pwsh "Verify library dependencies" (
                "./macos/Test-Dependencies.ps1" +
                $" -DotNetArch {Names.archToDotNet arch}" +
                $" -PackageName {Names.package platform arch}"
            )

        Workflows.TestJob(
            image = macOs14,
            platform = Platform.MacOS,
            arch = Arch.AArch64,
            testArgs = "-NuGet nuget",
            afterDownloadSteps = [
                testMacOsDependencies Platform.MacOS Arch.AArch64
            ]
        )

        Workflows.TestJob(
            image = macOs13,
            platform = Platform.MacOS,
            arch = Arch.X86_64,
            testArgs = "-NuGet nuget",
            afterDownloadSteps = [
                testMacOsDependencies Platform.MacOS Arch.X86_64
            ]
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
                pwsh "Verify library dependencies" "./windows/Test-Dependencies.ps1"
            ]
        )

        job "release" [
            runsOn ubuntuLatest
            yield! [
                Names.buildJob Platform.Ubuntu20_04 Arch.X86_64
                Names.buildJob Platform.Ubuntu22_04 Arch.X86_64
                Names.buildJob Platform.MacOS Arch.AArch64
                Names.buildJob Platform.MacOS Arch.X86_64
                Names.buildJob Platform.Windows Arch.X86_64
            ] |> Seq.map needs

            yield! testEnv
            checkout

            yield! downloadAndRepackArtifact Platform.Ubuntu20_04 Arch.X86_64
            yield! downloadAndRepackArtifact Platform.Ubuntu22_04 Arch.X86_64
            yield! downloadAndRepackArtifact Platform.MacOS Arch.AArch64
            yield! downloadAndRepackArtifact Platform.MacOS Arch.X86_64
            yield! downloadAndRepackArtifact Platform.Windows Arch.X86_64

            setUpDotNetSdk

            step(
                name = "Read version from ref",
                id = "version",
                shell = "pwsh",
                run = "\"version=$(if ($env:GITHUB_REF.StartsWith('refs/tags/v')) { $env:GITHUB_REF -replace '^refs/tags/v', '' } else { \"$env:PACKAGE_VERSION_BASE-test\" })\" >> $env:GITHUB_OUTPUT"
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

            let packPackageFor platform arch =
                pwsh
                    $"Pack NuGet package: {platform}.{arch}"
                    (
                        $"dotnet pack {Names.package platform arch}.proj" +
                        " -p:Version=${{ steps.version.outputs.version }} --output build"
                    )

            packPackageFor Platform.Ubuntu20_04 Arch.X86_64
            packPackageFor Platform.Ubuntu22_04 Arch.X86_64
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

                let uploadArchive platform arch =
                    step(
                        name = $"Upload archive: {platform}.{arch}",
                        uses = "actions/upload-release-asset@v1",
                        env = Map.ofList [
                            "GITHUB_TOKEN", "${{ secrets.GITHUB_TOKEN }}"
                        ],
                        options = Map.ofList [
                            "upload_url", "${{ steps.release.outputs.upload_url }}"
                            "asset_name", $"{Names.ciArtifact platform arch}.zip"
                            "asset_path", $"./{Names.ciArtifact platform arch}.zip"
                            "asset_content_type", "application/zip"
                        ]
                    )

                uploadArchive Platform.Ubuntu20_04 Arch.X86_64
                uploadArchive Platform.Ubuntu22_04 Arch.X86_64
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

                let uploadPlatformPackage platform arch =
                    uploadPackage (
                        $"{Names.package platform arch}." + "${{ steps.version.outputs.version }}.nupkg"
                    )

                uploadPlatformPackage Platform.Ubuntu20_04 Arch.X86_64
                uploadPlatformPackage Platform.Ubuntu22_04 Arch.X86_64
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

            let pushPlatformPackage platform arch =
                pushPackage (
                    $"{Names.package platform arch}." + "${{ steps.version.outputs.version }}.nupkg"
                )

            pushPlatformPackage Platform.Ubuntu20_04 Arch.X86_64
            pushPlatformPackage Platform.Ubuntu22_04 Arch.X86_64
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
