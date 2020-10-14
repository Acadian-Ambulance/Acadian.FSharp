#r "paket:
nuget Fake.BuildServer.TeamCity
nuget Fake.Core.Target
nuget Fake.Core.UserInput
nuget Fake.DotNet.Cli //"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.DotNet
open Fake.BuildServer
open Fake.DotNet.NuGet.NuGet

let packageProject = "Acadian.FSharp"

BuildServer.install [TeamCity.Installer]

let buildVersion = lazy (
    if BuildServer.isLocalBuild then
        let input = UserInput.getUserInput "Package Version: "
        if input |> SemVer.isValid |> not then
            failwithf "%s is not a valid Semantic Version" input
        input
    else BuildServer.buildVersion
)

let nugetApiKey = lazy (
    match BuildServer.buildServer with
    | TeamCity ->
        TeamCity.BuildParameters.Configuration
        |> Map.tryFind "NugetPublishApiKey"
        |> Option.defaultWith (fun () ->
            failwith "Nuget.org API key not found. Please set Config Param 'NugetPublishApiKey' in TeamCity."
        )
    | _ ->
        Environment.environVarOrFail "NugetPublishApiKey"
)

let setVersion (a: MSBuild.CliArguments) =
    let props = ["PackageVersion";"Version";"FileVersion"]
    { a with Properties = props |> List.map (fun p -> p, buildVersion.Value) }

let failOnNonZero failMessage exitCode =
    if exitCode <> 0 then failwith failMessage

Target.create "Build" (fun _ ->
    DotNet.build (fun args -> { args with Configuration = DotNet.Release }) ""
)

Target.create "Test" (fun _ ->
    DotNet.test (fun args -> { args with Configuration = DotNet.Release }) ""
)

Target.create "Pack" (fun _ ->
    DotNet.pack (fun args ->
        { args with
            Configuration = DotNet.Release
            OutputPath = Some "nupkgs"
            MSBuildParams = args.MSBuildParams |> setVersion
        }
    ) packageProject
)

Target.create "Publish" (fun _ ->
    let path = sprintf "nupkgs/%s.%s.nupkg" packageProject buildVersion.Value
    let pushParams (p: NuGetPushParams) =
        { p with Source = Some "https://api.nuget.org/v3/index.json"; ApiKey = Some nugetApiKey.Value }
    DotNet.nugetPush (fun o -> { o with PushParams = o.PushParams |> pushParams }) path
)

open Fake.Core.TargetOperators

"Build"
==> "Test"
==> "Pack"
==> "Publish"

Target.runOrDefault "Build"
