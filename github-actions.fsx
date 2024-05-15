#r "nuget: Generaptor.Library, 1.5.0"
open System

open Generaptor
open Generaptor.GitHubActions
open type Generaptor.GitHubActions.Commands
open type Generaptor.Library.Actions
open type Generaptor.Library.Patterns

let mainBranch = "master"

let linuxImage = "ubuntu-20.04"
let macOsImage = "macos-11"
let windowsImage = "windows-2019"

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
                    run = $"if (!(Test-Path -LiteralPath '{file}')) {{ throw 'File not found' }}"
                )
        ]

        let prepareArtifacts os resultFileName =
            pwshWithResult "Prepare artifact" $"./{os}/prepare-artifacts.ps1" [resultFileName]

        let setUpDotNetSdk = step(name = "Set up .NET SDK", uses = "actions/setup-dotnet@v4", options = Map.ofList [
            "dotnet-version", "7.0.x"
        ])

        job "build-linux" [
            runsOn linuxImage

            step(name = "Checkout", uses = "actions/checkout@v4", options = Map.ofList [
                "submodules", "true"
            ])
            pwsh "Verify encoding" "./common/verify-encoding.ps1"
            pwsh "Install" "./linux/install.ps1 -ForBuild"
            pwsh "Build" "./linux/build.ps1"
            yield! prepareArtifacts "linux" "artifacts/libtdjson.so"
            step(name = "Upload build result", uses = "actions/upload-artifact@v2", options = Map.ofList [
                "name", "tdlib.linux"
                "path", "artifacts/*"
            ])
        ]

        job "build-macos" [
            runsOn macOsImage

            step(name = "Checkout", uses = "actions/checkout@v4", options = Map.ofList [
                "submodules", "true"
            ])
            pwsh "Verify encoding" "./common/verify-encoding.ps1"
            pwsh "Install" "./macos/install.ps1"
            pwsh "Build" "./macos/build.ps1"
            yield! prepareArtifacts "macos" "artifacts/libtdjson.dylib"
            step(name = "Upload build result", uses = "actions/upload-artifact@v2", options = Map.ofList [
                "name", "tdlib.osx"
                "path", "artifacts/*"
            ])
        ]

        job "build-windows" [
            runsOn windowsImage

            step(name = "Checkout", uses = "actions/checkout@v4", options = Map.ofList [
                "submodules", "true"
            ])
            pwsh "Verify encoding" "./common/verify-encoding.ps1"
            pwsh "Build" @"./windows/build.ps1 -VcpkgToolchain c:\vcpkg\scripts\buildsystems\vcpkg.cmake"
            yield! prepareArtifacts "windows" "artifacts/tdjson.dll"
            step(name = "Upload build result", uses = "actions/upload-artifact@v2", options = Map.ofList [
                "name", "tdlib.windows"
                "path", "artifacts/*"
            ])
        ]

        let testEnv =
            [
                "DOTNET_NOLOGO", "1"
                "DOTNET_CLI_TELEMETRY_OPTOUT", "1"
                "NUGET_PACKAGES", "${{ github.workspace }}/.github/nuget-packages"
                "PACKAGE_VERSION_BASE", "1.8.21"
            ] |> Seq.map(fun (k, v) -> setEnv k v)

        job "test-linux" [
            needs "build-linux"
            runsOn linuxImage
            yield! testEnv

            step(name = "Checkout", uses = "actions/checkout@v4", options = Map.ofList [
                "submodules", "true"
            ])
            pwsh "Install" "./linux/install.ps1 -ForTests"
            step(name = "Download Linux artifact", uses = "actions/download-artifact@v4", options = Map.ofList [
                "name", "tdlib.linux"
                "path", "./artifacts"
            ])
            pwsh "Prepare package for testing" "./linux/prepare-package.ps1"
            setUpDotNetSdk
            pwsh "Pack NuGet" "dotnet pack -p:PackageVersion=${{ env.PACKAGE_VERSION_BASE }} --output build"
            // TODO[#64]: Add ${{ github.run_id }} as a patch version
            step(name = "NuGet cache", uses = "actions/cache@v4", options = Map.ofList [
                "path", "${{ env.NUGET_PACKAGES }}"
                "key", "${{ runner.os }}.nuget.${{ hashFiles('tdsharp/**/*.csproj') }}"
            ])
            pwsh "Test" "./common/test.ps1 -NuGet $env:GITHUB_WORKSPACE/tools/nuget.exe -UseMono"
        ]

        job "test-macos" [
            needs "build-macos"
            runsOn macOsImage
            yield! testEnv

            step(name = "Checkout", uses = "actions/checkout@v4", options = Map.ofList [
                "submodules", "true"
            ])
            step(name = "Download macOS artifact", uses = "actions/download-artifact@v4", options = Map.ofList [
                "name", "tdlib.osx"
                "path", "./artifacts"
            ])
            pwsh "Prepare package for testing" "./macos/prepare-package.ps1"
            setUpDotNetSdk
            pwsh "Pack NuGet" "dotnet pack -p:PackageVersion=${{ env.PACKAGE_VERSION_BASE }} --output build"
            // TODO[#64]: Add ${{ github.run_id }} as a patch version
            step(name = "NuGet cache", uses = "actions/cache@v4", options = Map.ofList [
                "path", "${{ env.NUGET_PACKAGES }}"
                "key", "${{ runner.os }}.nuget.${{ hashFiles('tdsharp/**/*.csproj') }}"
            ])
            pwsh "Test" "./common/test.ps1 -NuGet nuget"
        ]

        job "test-windows" [
            needs "build-windows"
            runsOn windowsImage
            yield! testEnv

            step(name = "Checkout", uses = "actions/checkout@v4", options = Map.ofList [
                "submodules", "true"
            ])
            step(name = "Download Windows artifact", uses = "actions/download-artifact@v4", options = Map.ofList [
                "name", "tdlib.windows"
                "path", "./artifacts"
            ])
            step(name = "Cache downloads for Windows", uses = "actions/cache@v4", options = Map.ofList [
                "path", "build/downloads"
                "key", "${{ hashFiles('windows/install.ps1') }}"
            ])

            pwsh "Install dependencies" "./windows/install.ps1"

            pwsh "Windows-specific testing" "./windows/test.ps1"

            pwsh "Prepare package for testing" "./windows/prepare-package.ps1"
            setUpDotNetSdk
            pwsh "Pack NuGet" "dotnet pack -p:PackageVersion=${{ env.PACKAGE_VERSION_BASE }} --output build"
            // TODO[#64]: Add ${{ github.run_id }} as a patch version
            step(name = "NuGet cache", uses = "actions/cache@v4", options = Map.ofList [
                "path", "${{ env.NUGET_PACKAGES }}"
                "key", "${{ runner.os }}.nuget.${{ hashFiles('tdsharp/**/*.csproj') }}"
            ])
            pwsh "Test" "./common/test.ps1 -NuGet nuget"
        ]

        job "release" [
            runsOn linuxImage
            needs "build-linux"
            needs "build-macos"
            needs "build-windows"
            yield! testEnv
            checkout

            step(name = "Download Linux artifact", uses = "actions/download-artifact@v4", options = Map.ofList [
                "name", "tdlib.linux"
                "path", "./build/runtimes/linux-x64/native"
            ])
            pwsh "Archive Linux artifact" "Set-Location ./build/runtimes/linux-x64/native && zip -r $env:GITHUB_WORKSPACE/tdlib.linux.zip *"

            step(name = "Download macOS artifact", uses = "actions/download-artifact@v4", options = Map.ofList [
                "name", "tdlib.osx"
                "path", "./build/runtimes/osx-x64/native"
            ])
            pwsh "Archive macOS artifact" "Set-Location ./build/runtimes/osx-x64/native && zip -r $env:GITHUB_WORKSPACE/tdlib.osx.zip *"

            step(name = "Download Windows artifact", uses = "actions/download-artifact@v4", options = Map.ofList [
                "name", "tdlib.windows"
                "path", "./build/runtimes/win-x64/native"
            ])
            pwsh "Archive Windows artifact" "Set-Location ./build/runtimes/win-x64/native && zip -r $env:GITHUB_WORKSPACE/tdlib.windows.zip *"

            setUpDotNetSdk

            step(
                name = "Read version from ref",
                id = "version",
                shell = "pwsh",
                run = "\"version=$(if ($env:GITHUB_REF.StartsWith('refs/tags/v')) { $env:GITHUB_REF -replace '^refs/tags/v', '' } else { \"$env:PACKAGE_VERSION_BASE-preview\" })\" >> $env:GITHUB_OUTPUT"
            )

            step(
                name = "Prepare the release notes (text)",
                uses = "ForNeVeR/ChangelogAutomation.action@v1",
                options = Map.ofList [
                    "input", "./CHANGELOG.md"
                    "output", "releaseNotes.txt"
                    "format", "PlainText"
                ]
            )
            step(
                name = "Prepare the release notes (Markdown)",
                uses = "ForNeVeR/ChangelogAutomation.action@v1",
                options = Map.ofList [
                    "input", "./CHANGELOG.md"
                    "output", "release-notes.md"
                    "format", "Markdown"
                ]
            )

            pwsh "Update the release notes" "./common/update-release-notes.ps1 ./releaseNotes.txt"

            pwsh "Prepare NuGet package" "dotnet pack -p:PackageVersion=${{ steps.version.outputs.version }} --output build"

            step(name = "Upload NuGet package", uses = "actions/upload-artifact@v4", options = Map.ofList [
                "name", "tdlib.nuget"
                "path", "./build/tdlib.native.${{ steps.version.outputs.version }}.nupkg"
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
                step(
                    name = "Release Linux artifact",
                    uses = "actions/upload-release-asset@v1",
                    env = Map.ofList [
                        "GITHUB_TOKEN", "${{ secrets.GITHUB_TOKEN }}"
                    ],
                    options = Map.ofList [
                        "upload_url", "${{ steps.release.outputs.upload_url }}"
                        "asset_name", "tdlib.linux.zip"
                        "asset_path", "./tdlib.linux.zip"
                        "asset_content_type", "application/zip"
                    ]
                )
                step(
                    name = "Release macOS artifact",
                    uses = "actions/upload-release-asset@v1",
                    env = Map.ofList [
                        "GITHUB_TOKEN", "${{ secrets.GITHUB_TOKEN }}"
                    ],
                    options = Map.ofList [
                        "upload_url", "${{ steps.release.outputs.upload_url }}"
                        "asset_name", "tdlib.osx.zip"
                        "asset_path", "./tdlib.osx.zip"
                        "asset_content_type", "application/zip"
                    ]
                )
                step(
                    name = "Release Windows artifact",
                    uses = "actions/upload-release-asset@v1",
                    env = Map.ofList [
                        "GITHUB_TOKEN", "${{ secrets.GITHUB_TOKEN }}"
                    ],
                    options = Map.ofList [
                        "upload_url", "${{ steps.release.outputs.upload_url }}"
                        "asset_name", "tdlib.windows.zip"
                        "asset_path", "./tdlib.windows.zip"
                        "asset_content_type", "application/zip"
                    ]
                )
                step(
                    name = "Release NuGet package",
                    uses = "actions/upload-release-asset@v1",
                    env = Map.ofList [
                        "GITHUB_TOKEN", "${{ secrets.GITHUB_TOKEN }}"
                    ],
                    options = Map.ofList [
                        "upload_url", "${{ steps.release.outputs.upload_url }}"
                        "asset_name", "tdlib.native.${{ steps.version.outputs.version }}.nupkg"
                        "asset_path", "./build/tdlib.native.${{ steps.version.outputs.version }}.nupkg"
                        "asset_content_type", "application/zip"
                    ]
                )
            ]

            step(
                name = "Push the package to nuget.org",
                condition = "github.event_name == 'push' && contains(github.ref, 'refs/tags/')",
                run = "dotnet nuget push --source https://api.nuget.org/v3/index.json --api-key ${{ secrets.NUGET_KEY }} ./build/tdlib.native.${{ steps.version.outputs.version }}.nupkg"
            )
        ]
    ]
]

EntryPoint.Process fsi.CommandLineArgs workflows
