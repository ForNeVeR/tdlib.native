let licenseHeader = """
# SPDX-FileCopyrightText: 2018-2026 tdlib.native contributors <https://github.com/ForNeVeR/tdlib.native>
#
# SPDX-License-Identifier: BSL-1.0

# This file is auto-generated.""".Trim()

#r "nuget: Generaptor.Library, 1.9.1"

open System
open System.IO
open Generaptor
open Generaptor.GitHubActions
open type Generaptor.GitHubActions.Commands
open type Generaptor.Library.Actions
open type Generaptor.Library.Patterns

let mainBranch = "master"

let macOs14 = "macos-14"
let macOs15 = "macos-15-intel"
let ubuntu22_04 = "ubuntu-22.04"
let ubuntu22_04Arm = "ubuntu-22.04-arm"
let ubuntuLatest = "ubuntu-latest"
let windows2022 = "windows-2022"
let windows11Arm = "windows-11-arm"

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

let checkoutWithSubmodules = step(
    name = "Check out the sources",
    usesSpec = Auto "actions/checkout",
    options = Map.ofList [
        "submodules", "true"
    ]
)

module Platform =
    let [<Literal>] Ubuntu22_04 = "ubuntu-22.04"
    let [<Literal>] MacOS = "macos"
    let [<Literal>] Windows = "windows"

module Arch =
    let [<Literal>] AArch64 = "aarch64"
    let [<Literal>] X86_64 = "x86-64"

module Names =
    let private platformToDotNet = function
    | Platform.Ubuntu22_04 -> "linux"
    | Platform.MacOS -> "osx"
    | Platform.Windows -> "win"
    | other -> failwith $"Unknown platform {other}"

    let archToDotNet = function
    | Arch.AArch64 -> "arm64"
    | Arch.X86_64 -> "x64"
    | other -> failwith $"Unknown architecture {other}"

    let package platform arch =
        $"tdlib.native.{platformToDotNet platform}-{archToDotNet arch}"

    let ciArtifact platform arch = $"tdlib.native.{platform}.{arch}"
    let packageInputDirectory platform arch =
        $"build/{package platform arch}/runtimes/{platformToDotNet platform}-{archToDotNet arch}/native"

    let buildJob (platform: string) arch = $"build-{platform.Replace('.', '-')}-{arch}"
    let testJob (platform: string) arch = $"test-{platform.Replace('.', '-')}-{arch}"
    let os = function
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
        usesSpec = Auto "actions/download-artifact",
        options = Map.ofList [
            "name", Names.ciArtifact platform arch
            "path", Names.packageInputDirectory platform arch
        ]
    )

let setUpDotNetSdk = step(
    name = "Set up .NET SDK",
    usesSpec = Auto "actions/setup-dotnet"
)

let getVersion stepId = step(
    id = stepId,
    name = "Read version",
    shell = "pwsh",
    run = "echo \"version=$(common/Get-Version.ps1 -RefName $env:GITHUB_REF)\" >> $env:GITHUB_OUTPUT"
)

let nuGetPackagePath = "${{ github.workspace }}/.github/nuget-packages"
let dotNetEnv =
    [
        "DOTNET_NOLOGO", "1"
        "DOTNET_CLI_TELEMETRY_OPTOUT", "1"
        "NUGET_PACKAGES", nuGetPackagePath
    ] |> Seq.map(fun (k, v) -> setEnv k v)

let nuGetCache = step(
    name = "NuGet cache",
    usesSpec = Auto "actions/cache",
    options = Map.ofList [
        "path", nuGetPackagePath
        "key", "${{ runner.os }}.nuget.${{ hashFiles('**/*.*proj') }}"
    ]
)

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
             usesSpec = Auto "actions/cache",
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
        step(
            name = "Upload build result",
            usesSpec = Auto "actions/upload-artifact",
            options = Map.ofList [
                "name", Names.ciArtifact platform arch
                "path", "artifacts/*"
            ]
        )
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
        ?installScript: string,
        ?afterDownloadSteps: JobCreationCommand seq
    ) = job (Names.testJob platform arch) [
        needs(Names.buildJob platform arch)
        runsOn image
        yield! dotNetEnv

        checkoutWithSubmodules
        yield! installScript |> Option.toList |> Seq.map(pwsh "Install")
        downloadArtifact platform arch

        yield! afterDownloadSteps |> Option.toList |> Seq.collect id

        setUpDotNetSdk

        let versionStepId = "version"
        getVersion versionStepId

        pwsh "Pack NuGet" (
            $"dotnet pack {Names.package platform arch}.proj" +
            " -p:Version=${{ steps." + versionStepId + ".outputs.version }}-test --output build"
        )
        nuGetCache
        pwsh "Test" $"./common/test.ps1 -PackageName {Names.package platform arch}"
    ]

let workflows = [
    workflow "main" [
        header licenseHeader
        name "Main"
        onPushTo mainBranch
        onPushTo "renovate/**"
        onPushTags "v*"
        onPullRequestTo mainBranch
        onSchedule(day = DayOfWeek.Monday)
        onWorkflowDispatch

        job "licenses" [
            runsOn ubuntuLatest
            step(
                name = "Check out the sources",
                usesSpec = Auto "actions/checkout"
            )
            step(
                name = "REUSE license check",
                usesSpec = Auto "fsfe/reuse-action"
            )
        ]

        job "verify-workflows" [
            runsOn ubuntuLatest
            yield! dotNetEnv

            step(
                usesSpec = Auto "actions/checkout"
            )
            step(
                usesSpec = Auto "actions/setup-dotnet"
            )
            step(
                run = "dotnet fsi ./github-actions.fsx verify"
            )
        ]

        Workflows.BuildJob(
            image = ubuntu22_04,
            platform = Platform.Ubuntu22_04,
            arch = Arch.X86_64,
            installScript = "./linux/install.ps1 -ForBuild",
            artifactFileName = "libtdjson.so"
        )

        Workflows.BuildJob(
            image = ubuntu22_04Arm,
            platform = Platform.Ubuntu22_04,
            arch = Arch.AArch64,
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
            image = macOs15,
            platform = Platform.MacOS,
            arch = Arch.X86_64,
            installScript = "./macos/install.ps1",
            artifactFileName = "libtdjson.dylib"
        )

        let windowsBuildJob image arch =
            Workflows.BuildJob(
                image = image,
                platform = Platform.Windows,
                arch = arch,
                buildScriptArgs =
                    $@"-VcpkgToolchain c:\vcpkg\scripts\buildsystems\vcpkg.cmake -DotNetArch {Names.archToDotNet arch}",
                artifactFileName = "tdjson.dll"
            )

        windowsBuildJob windows2022 Arch.X86_64
        windowsBuildJob windows11Arm Arch.AArch64

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
                $" -DotNetArch {Names.archToDotNet arch}" +
                $" -PackageName {Names.package platform arch}"
            )

        Workflows.TestJob(
            image = ubuntu22_04,
            platform = Platform.Ubuntu22_04,
            arch = Arch.X86_64,
            installScript = "./linux/install.ps1 -ForTests",
            afterDownloadSteps = [
                testLinuxDependencies Platform.Ubuntu22_04 Arch.X86_64
            ]
        )

        Workflows.TestJob(
            image = ubuntu22_04Arm,
            platform = Platform.Ubuntu22_04,
            arch = Arch.AArch64,
            installScript = "./linux/install.ps1 -ForTests",
            afterDownloadSteps = [
                testLinuxDependencies Platform.Ubuntu22_04 Arch.AArch64
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
            afterDownloadSteps = [
                testMacOsDependencies Platform.MacOS Arch.AArch64
            ]
        )

        Workflows.TestJob(
            image = macOs15,
            platform = Platform.MacOS,
            arch = Arch.X86_64,
            afterDownloadSteps = [
                testMacOsDependencies Platform.MacOS Arch.X86_64
            ]
        )

        let testWindowsDependencies arch = [
            step(name = "Cache downloads for Windows", usesSpec = Auto "actions/cache", options = Map.ofList [
                "path", "build/downloads"
                "key", "${{ hashFiles('windows/install.ps1') }}"
            ])
            pwsh "Install dependencies" "./windows/install.ps1"
            pwsh "Verify library dependencies" $"./windows/Test-Dependencies.ps1 -DotNetArch {Names.archToDotNet arch}"
        ]

        Workflows.TestJob(
            image = windows2022,
            platform = Platform.Windows,
            arch = Arch.X86_64,
            afterDownloadSteps = testWindowsDependencies Arch.X86_64
        )

        Workflows.TestJob(
            image = windows11Arm,
            platform = Platform.Windows,
            arch = Arch.AArch64,
            afterDownloadSteps = testWindowsDependencies Arch.AArch64
        )

        job "release" [
            runsOn ubuntuLatest
            yield! [
                Names.buildJob Platform.MacOS Arch.AArch64
                Names.buildJob Platform.MacOS Arch.X86_64
                Names.buildJob Platform.Ubuntu22_04 Arch.AArch64
                Names.buildJob Platform.Ubuntu22_04 Arch.X86_64
                Names.buildJob Platform.Windows Arch.AArch64
                Names.buildJob Platform.Windows Arch.X86_64
            ] |> Seq.map needs

            yield! dotNetEnv
            checkout

            yield! downloadAndRepackArtifact Platform.MacOS Arch.AArch64
            yield! downloadAndRepackArtifact Platform.MacOS Arch.X86_64
            yield! downloadAndRepackArtifact Platform.Ubuntu22_04 Arch.AArch64
            yield! downloadAndRepackArtifact Platform.Ubuntu22_04 Arch.X86_64
            yield! downloadAndRepackArtifact Platform.Windows Arch.AArch64
            yield! downloadAndRepackArtifact Platform.Windows Arch.X86_64

            setUpDotNetSdk

            let versionStepId = "version"
            getVersion versionStepId

            step(
                name = "Prepare the release notes",
                usesSpec = Auto "ForNeVeR/ChangelogAutomation.action",
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
                        " -p:Version=${{ steps." + versionStepId + ".outputs.version }} --output build"
                    )

            packPackageFor Platform.MacOS Arch.AArch64
            packPackageFor Platform.MacOS Arch.X86_64
            packPackageFor Platform.Ubuntu22_04 Arch.AArch64
            packPackageFor Platform.Ubuntu22_04 Arch.X86_64
            packPackageFor Platform.Windows Arch.AArch64
            packPackageFor Platform.Windows Arch.X86_64

            pwsh
                "Prepare NuGet source"
                "common/New-NuGetSource.ps1"
            pwsh
                "Pack NuGet package: main"
                ("dotnet pack tdlib.native.proj -p:Version=${{ steps." + versionStepId + ".outputs.version }} --output build")

            step(name = "Upload NuGet packages", usesSpec = Auto "actions/upload-artifact", options = Map.ofList [
                "name", "tdlib.nuget"
                "path", "./build/*.nupkg"
            ])

            step(
                name = "Verify NuGet packages",
                shell = "pwsh",
                run = "common/Test-Package.ps1"
            )

            yield! ifCalledOnTagPush [
                step(
                    name = "Create release",
                    id = "release",
                    usesSpec = Auto "actions/create-release",
                    env = Map.ofList [
                        "GITHUB_TOKEN", "${{ secrets.GITHUB_TOKEN }}"
                    ],
                    options = Map.ofList [
                        "tag_name", "${{ github.ref }}"
                        "release_name", "v${{ steps." + versionStepId + ".outputs.version }}"
                        "body_path", "./release-notes.md"
                    ]
                )

                let uploadArchive platform arch =
                    step(
                        name = $"Upload archive: {platform}.{arch}",
                        usesSpec = Auto "actions/upload-release-asset",
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

                uploadArchive Platform.MacOS Arch.AArch64
                uploadArchive Platform.MacOS Arch.X86_64
                uploadArchive Platform.Ubuntu22_04 Arch.AArch64
                uploadArchive Platform.Ubuntu22_04 Arch.X86_64
                uploadArchive Platform.Windows Arch.AArch64
                uploadArchive Platform.Windows Arch.X86_64

                let uploadPackage fileName =
                    step(
                        name = $"Upload NuGet package: {fileName}",
                        usesSpec = Auto "actions/upload-release-asset",
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
                        $"{Names.package platform arch}." + "${{ steps." + versionStepId + ".outputs.version }}.nupkg"
                    )

                uploadPlatformPackage Platform.MacOS Arch.AArch64
                uploadPlatformPackage Platform.MacOS Arch.X86_64
                uploadPlatformPackage Platform.Ubuntu22_04 Arch.AArch64
                uploadPlatformPackage Platform.Ubuntu22_04 Arch.X86_64
                uploadPlatformPackage Platform.Windows Arch.AArch64
                uploadPlatformPackage Platform.Windows Arch.X86_64
                uploadPackage ("tdlib.native.${{ steps." + versionStepId + ".outputs.version }}.nupkg")
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
                    $"{Names.package platform arch}." + "${{ steps." + versionStepId + ".outputs.version }}.nupkg"
                )

            pushPlatformPackage Platform.MacOS Arch.AArch64
            pushPlatformPackage Platform.MacOS Arch.X86_64
            pushPlatformPackage Platform.Ubuntu22_04 Arch.AArch64
            pushPlatformPackage Platform.Ubuntu22_04 Arch.X86_64
            pushPlatformPackage Platform.Windows Arch.AArch64
            pushPlatformPackage Platform.Windows Arch.X86_64
            pushPackage ("tdlib.native.${{ steps." + versionStepId + ".outputs.version }}.nupkg")
        ]

        job "verify-encoding" [
            runsOn ubuntuLatest
            step(
                name = "Check out the sources",
                usesSpec = Auto "actions/checkout"
            )
            step(
                name = "Verify encoding",
                shell = "pwsh",
                run = "Install-Module VerifyEncoding -Repository PSGallery -RequiredVersion 2.2.1 -Force && Test-Encoding"
            )
        ]
    ]

    workflow "maintenance" [
        header licenseHeader
        name "Maintenance"
        onPushTo mainBranch
        onPushTo "renovate/**"
        onPullRequestTo mainBranch
        onSchedule(cron = "0 0 * * *")
        onWorkflowDispatch

        let updateJob stepName dependencyName script = job stepName [
            jobPermission(PermissionKind.Contents, AccessKind.Write)
            jobPermission(PermissionKind.PullRequests, AccessKind.Write)

            runsOn ubuntuLatest

            yield! dotNetEnv

            step(
                name = "Check out the sources",
                usesSpec = Auto "actions/checkout"
            )
            nuGetCache
            setUpDotNetSdk

            step(
                id = "update-dependency",
                name = $"Update {dependencyName}",
                shell = "pwsh",
                run = $"dotnet fsi \"{script}\"",
                env = Map.ofList [
                    "GITHUB_TOKEN", "${{ secrets.GITHUB_TOKEN }}"
                ]
            )

            step(
                condition = "(github.event_name == 'schedule' || github.event_name == 'workflow_dispatch') && steps.update-dependency.outputs.has-changes == 'true'",
                name = "Create a pull request",
                usesSpec = Auto "peter-evans/create-pull-request",
                options = Map.ofList [
                    "author", "tdlib-native automation <friedrich@fornever.me>"
                    "body", "${{ steps.update-dependency.outputs.body }}"
                    "branch", "${{ steps.update-dependency.outputs.branch-name }}"
                    "commit-message", "${{ steps.update-dependency.outputs.commit-message }}"
                    "title", "${{ steps.update-dependency.outputs.title }}"
                ]
            )
        ]

        updateJob "update-tdlib" "TDLib" "./update-tdlib.fsx"
        updateJob "update-tdsharp" "tdsharp" "./update-tdsharp.fsx"
    ]
]

exit <| EntryPoint.Process fsi.CommandLineArgs workflows
