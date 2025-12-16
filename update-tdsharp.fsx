// SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: BSL-1.0

#r "nuget: TruePath, 1.11.0"
#r "nuget: Octokit, 14.0.0"
#r "nuget: MedallionShell, 1.6.2"

open System
open System.IO
open Medallion.Shell
open Octokit
open TruePath

let repoRoot = AbsolutePath __SOURCE_DIRECTORY__
let tdSharpSources = repoRoot / "tdsharp"

let readCurrentCommit() =
    let commandResult =
        Command.Run(
            "git",
            arguments = [| "rev-parse"; "HEAD" |],
            options = fun opts -> opts.WorkingDirectory tdSharpSources.Value |> ignore
        ).Result

    if not commandResult.Success then
        if commandResult.StandardOutput.Length > 0 then printfn $"%s{commandResult.StandardOutput}"
        if commandResult.StandardError.Length > 0 then printfn $"Standard error: %s{commandResult.StandardError}"
        failwithf $"git rev-parse failed with exit code {commandResult.ExitCode}."

    commandResult.StandardOutput.Trim()

let processCommandResult(result: CommandResult, assertSuccess: bool) =
    if result.StandardOutput.Length > 0 then printfn $"%s{result.StandardOutput}"
    if result.StandardError.Length > 0 then printfn $"Standard error: %s{result.StandardError}"
    if assertSuccess && not result.Success then failwithf $"Command failed with exit code {result.ExitCode}."

let assertCommandResult result = processCommandResult(result, true)

let isAncestor (potentialAncestor: string) (potentialDescendant: string) =
    if potentialAncestor = potentialDescendant then false
    else

    let commandResult =
        Command.Run(
            "git",
            arguments = [|
                "merge-base";
                "--is-ancestor"; potentialAncestor; potentialDescendant
            |],
            options = fun opts -> opts.WorkingDirectory tdSharpSources.Value |> ignore
        ).Result
    processCommandResult(commandResult, false)
    match commandResult.ExitCode with
    | 0 -> true
    | 1 -> false
    | o -> failwithf $"git merge-base returned unexpected exit code {o}."

let tagNameToVersion(name: string) =
    if not <| name.StartsWith "v" then failwithf $"Version number is expected to start with 'v', actual: {name}."
    Version.Parse(name.Substring 1)

let readLatestVersion() = task {
    let client = GitHubClient(ProductHeaderValue("tdlib.native"))
    match Environment.GetEnvironmentVariable "GITHUB_TOKEN" |> Option.ofObj with
    | Some token ->
        printfn "Found GitHub token credentials in environment, will use."
        client.Credentials <- Credentials token
    | None -> ()

    let! allTags = client.Repository.GetAllTags("egramtel", "tdsharp")

    return
        allTags
        |> Seq.maxBy (fun t -> tagNameToVersion t.Name)
}

let fetchGitSubmodule() =
    printfn "Initializing the Git submodule…"
    Command.Run(
        "git",
        "submodule", "update", "--init", "--", tdSharpSources.Value
    ).Result |> assertCommandResult
    printfn "Fetching the Git submodule…"
    Command.Run(
        "git",
        arguments = [| "fetch"; "--all" |],
        options = fun opts -> opts.WorkingDirectory tdSharpSources.Value |> ignore
    ).Result |> assertCommandResult
    printfn "Git submodule fetched."

let updateGitSubmodule(commitHash: string) =
    printfn $"Checking out the Git submodule at commit {commitHash}…"
    Command.Run(
        "git",
        arguments = [| "checkout"; commitHash |],
        options = fun opts -> opts.WorkingDirectory tdSharpSources.Value |> ignore
    ).Result |> assertCommandResult
    printfn "Git submodule checked out."

let updateTo(tag: RepositoryTag) =
    let version = tag.Name.Substring 1 // skip the "v" letter
    let commitHash = tag.Commit.Sha

    updateGitSubmodule commitHash

    {|
        BodyMarkdown = $"Update tdsharp to version {version}."
        BranchName = $"dependencies/tdsharp-{version}"
        CommitMessage = $"Update tdsharp to {version}"
        PullRequestTitle = $"tdsharp {version}"
    |}

fetchGitSubmodule()

let currentCommit = readCurrentCommit()
let latestTag = readLatestVersion().Result
let latestCommit = latestTag.Commit.Sha

let result =
    printfn $"Current commit: {currentCommit}, latest commit: {latestCommit}."
    if isAncestor currentCommit latestCommit then
        printfn $"Updating to {latestTag.Name} ({latestCommit})…"
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
