// SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: BSL-1.0

#r "nuget: TruePath, 1.10.0"
#r "nuget: TruePath.SystemIo, 1.10.0"
#r "nuget: Octokit, 14.0.0"
#r "nuget: MedallionShell, 1.6.2"

open System
open System.IO
open System.Text.RegularExpressions
open Medallion.Shell
open Octokit
open TruePath
open TruePath.SystemIo

let repoRoot = AbsolutePath __SOURCE_DIRECTORY__

let gitHubActionsFile = repoRoot / "github-actions.fsx"

let readCurrentVersion(): Version =
    if not <| gitHubActionsFile.Exists() then
        failwithf $"{gitHubActionsFile.FileName} not found at expected path: \"{gitHubActionsFile.Value}\"."

    let content = gitHubActionsFile.ReadAllText()
    let regex = Regex "\"PACKAGE_VERSION_BASE\", \"(.*?)\""

    let result = regex.Match content
    if not result.Success then
        failwithf $"Cannot find the PACKAGE_VERSION_BASE defined in \"{gitHubActionsFile.Value}\"."

    result.Groups[1].Value |> Version.Parse

let tagNamePrefix = "tdlib/"
let tagNameToVersion(name: string) =
    let name = name.Substring tagNamePrefix.Length
    if not <| name.StartsWith "v" then failwithf $"Version number is expected to start with 'v', actual: {name}."
    Version.Parse(name.Substring 1)

let readLatestVersion() = task {
    let client = GitHubClient(ProductHeaderValue("tdlib.native"))
    match Environment.GetEnvironmentVariable "GITHUB_TOKEN" |> Option.ofObj with
    | Some token ->
        printfn "Found GitHub token credentials in environment, will use."
        client.Credentials <- Credentials token
    | None -> ()

    let! allTags = client.Repository.GetAllTags("ForNeVeR", "tdlib-versioned")

    return
        allTags
        |> Seq.filter (fun t -> t.Name.StartsWith tagNamePrefix)
        |> Seq.maxBy (fun t -> tagNameToVersion t.Name)
}

let updateGitHubActionsFile version =
    let content = gitHubActionsFile.ReadAllText()
    let versionRegex = Regex "\"PACKAGE_VERSION_BASE\", \"(.*?)\""
    let newContent = versionRegex.Replace(content, $"\"PACKAGE_VERSION_BASE\", \"{version}\"")
    gitHubActionsFile.WriteAllText newContent

let processCommandResult(result: CommandResult) =
    if result.StandardOutput.Length > 0 then printfn $"%s{result.StandardOutput}"
    if result.StandardError.Length > 0 then printfn $"Standard error: %s{result.StandardError}"
    if not result.Success then failwithf $"Command failed with exit code {result.ExitCode}."

let generateGitHubActions() =
    printfn $"Running {gitHubActionsFile.FileName}…"
    Command.Run("dotnet", "fsi", gitHubActionsFile.Value).Result |> processCommandResult
    printfn $"{gitHubActionsFile.FileName} executed successfully."

let updateChangelog(version: string) =
    let changelogFile = repoRoot / "CHANGELOG.md"
    let mutable changelog = changelogFile.ReadAllText()

    let firstReleaseSection = changelog.IndexOf "##"
    let hasUnreleased = changelog.Substring(firstReleaseSection).StartsWith("## [Unreleased]")
    if not hasUnreleased then
        changelog <- changelog.Insert(firstReleaseSection, "## [Unreleased]\n\n")

    changelog <- changelog.Substring(0, firstReleaseSection)
                 + $"## [Unreleased] ({version})"
                 + changelog.Substring(changelog.IndexOf("\n", firstReleaseSection))

    let nextSection = changelog.IndexOf("\n## ", firstReleaseSection)

    let mutable sectionContent = changelog.Substring(firstReleaseSection, nextSection - firstReleaseSection)
    let mutable changedSubsection = sectionContent.IndexOf "### Changed"
    if changedSubsection = -1 then
        sectionContent <- sectionContent.Insert(sectionContent.IndexOf "\n", "### Changed")
        changedSubsection <- sectionContent.IndexOf "### Changed"

    sectionContent <- sectionContent.Insert(
        sectionContent.IndexOf("\n", changedSubsection),
        $"\n- Update to [TDLib v{version}](https://github.com/ForNeVeR/tdlib-versioned/releases/tag/tdlib%%2Fv{version})."
    )

    changelog <- changelog.Substring(0, firstReleaseSection)
                 + sectionContent
                 + changelog.Substring(changelog.IndexOf("\n", nextSection))

    changelogFile.WriteAllText changelog

let tdSources = repoRoot / "td"
let updateGitSubmodule(commitHash: string) =
    printfn "Fetching the Git sources…"
    Command.Run(
        "git",
        arguments = [| "fetch" |],
        options = fun opts -> opts.WorkingDirectory tdSources.Value |> ignore
    ).Result |> processCommandResult
    printfn "Git sources fetched."

    printfn $"Checking out the Git submodule at commit {commitHash}…"
    Command.Run(
        "git",
        arguments = [| "checkout"; commitHash |],
        options = fun opts -> opts.WorkingDirectory tdSources.Value |> ignore
    ).Result |> processCommandResult
    printfn "Git submodule checked out."

let updateTo(tag: RepositoryTag) =
    let version = tag.Name.Substring(tagNamePrefix.Length + 1)
    let commitHash = tag.Commit.Sha

    updateGitSubmodule commitHash
    updateGitHubActionsFile version
    generateGitHubActions()
    updateChangelog version

    {|
        BodyMarkdown = $"Update TDLib to version {version} and regenerate the API definitions."
        BranchName = $"dependencies/tdlib-{version}"
        CommitMessage = $"Update TDLib to {version}"
        PullRequestTitle = $"TDLib {version}"
    |}

let currentVersion = readCurrentVersion()
let latestTag = readLatestVersion().Result
let latestVersion = tagNameToVersion latestTag.Name

let result =
    printfn $"Current version: {currentVersion}, latest version: {latestVersion}."
    if currentVersion < latestVersion then
        printfn $"Updating to {latestVersion}…"
        Some <| updateTo latestTag
    else
        printfn "Nothing to do."
        None

let writeResults() =
    let output = Environment.GetEnvironmentVariable "GITHUB_OUTPUT" |> Option.ofObj
    use outputStream =
        match output with
        | None ->
            printfn "No GITHUB_OUTPUT env var provided, results will be written to the standard output."
            Console.OpenStandardOutput()
        | Some path -> File.OpenWrite path

    use writer = new StreamWriter(outputStream)

    let serializeParameter key (value: string) =
        if value.Contains("\n") then
            let delimiter = Guid.NewGuid()
            writer.Write $"{key}<<{delimiter}\n{value}\n{delimiter}\n"
        else
            writer.Write $"{key}={value}\n"

    let fromBool = function | true -> "true" | false -> "false"

    serializeParameter "has-changes" (fromBool <| Option.isSome result)

    match result with
    | None -> ()
    | Some result ->
        serializeParameter "body" result.BodyMarkdown
        serializeParameter "branch-name" result.BranchName
        serializeParameter "commit-message" result.CommitMessage
        serializeParameter "title" result.PullRequestTitle

writeResults()
