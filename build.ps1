#!/usr/bin/env pwsh

param(
    [ValidateNotNullOrEmpty()]
    [string]$Target = "FullBuild",

    [ValidateNotNullOrEmpty()]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [string]$DotnetVerbosity = "minimal",

    [string]$VersionSuffix = "",

    [switch]$ScaleOutApplication
)

#######################################################################
# SHARED VARIABLES

$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

$repositoryDir = (Get-Item $PSScriptRoot).FullName
$srcDir = Join-Path $repositoryDir "src"
$dotnetSolutionFile = Get-ChildItem -Path $srcDir -Filter "*.sln" | Select-Object -First 1
$buildCoreVersionFile = Join-Path $repositoryDir "version.yaml"
$buildCoreVersion = $(Get-Content "$buildCoreVersionFile").Substring("version:".Length).Trim()
$buildVersion = "$buildCoreVersion$VersionSuffix"

$dockerComposeProject = "espm"

# This build system expects following solution layout:
# solution_root/               -- $repositoryDir
#   build.ps1                  -- this file - PowerShell build CLI
#   docker-compose.yaml        -- Docker Compose definition of the complete environment, including all required infrastructure
#   src/                       -- $srcDir
#     Project1/
#       Project1.csproj        -- project base filename matches directory name
#     Project1.Tests/
#       Project1.Tests.csproj  -- tests projects are xUnit-based; project name must have suffix '.Tests'
#     Project2/
#       Project2.fsproj
#       imagename.Dockerfile   -- if the `*.Dockerfile` is present, we'll build Docker image `imagename`
#     Project2.Tests/
#       Project2.Tests.fsproj
#     SolutionName.sln         -- only one '.sln' file in 'src'
#   version.yaml               -- core part of solution SemVer

#######################################################################
# LOGGING

function LogInfo {
    param([ValidateNotNullOrEmpty()] [string]$Message)
    Write-Host -ForegroundColor Green $Message
}

function LogWarning {
    param([ValidateNotNullOrEmpty()] [string]$Message)
    Write-Host -ForegroundColor Yellow "*** $Message"
}

function LogError {
    param([ValidateNotNullOrEmpty()] [string]$Message)
    Write-Host -ForegroundColor Red "*** $Message"
}

function LogStep {
    param(
        [ValidateNotNullOrEmpty()] [string]$Message,
        [ValidateNotNullOrEmpty()] [string]$Title = "STEP"
    )
    Write-Host -ForegroundColor Yellow "--- ${Title}: $Message"
}

function LogTarget {
    param([ValidateNotNullOrEmpty()] [string]$Message)
    Write-Host ""
    Write-Host -ForegroundColor Green "--- TARGET: $Message"
}

function LogCmd {
    param([ValidateNotNullOrEmpty()] [string]$Message)
    Write-Host -ForegroundColor Yellow "--- $Message"
}

#######################################################################
# STEPS

function PreludeStep_ValidateDotnetCli {
    LogStep "Check .NET CLI" -Title "PRELUDE"
    $dotnetCmd = Get-Command dotnet -ErrorAction Ignore
    if (-not $dotnetCmd) {
        LogError ".NET SDK CLI (dotnet) is not available. Refer to https://dotnet.microsoft.com/download for more information."
        Exit 1
    }
}

function PreludeStep_ValidateDockerCli {
    LogStep "Check Docker CLI" -Title "PRELUDE"
    $dockerCmd = Get-Command docker -ErrorAction Ignore
    if (-not $dockerCmd) {
        LogError "Docker CLI (docker) is not available. Refer to https://docs.docker.com/get-docker/ for more information."
        Exit 1
    }
}

function Step_PruneBuild {
    LogStep "PruneBuild"

    $pruneDir = $repositoryDir
    LogWarning "Pruning $pruneDir build artifacts"

    # Prune nested directories
    'bin', 'obj', 'publish', 'TestResults' | ForEach-Object {
        Get-ChildItem -Path $pruneDir -Filter $_ -Directory -Recurse | ForEach-Object { $_.Delete($true) }
    }

    # Prune nested files
    '*.trx', '*.fsx.lock', '*.Tests_*.xml' | ForEach-Object {
        Get-ChildItem -Path $pruneDir -Filter $_ -File -Recurse | ForEach-Object { $_.Delete() }
    }

    # Prune top-level items
    # '.fable', '.ionide', 'build', 'project', 'target', 'node_modules' | ForEach-Object {
    #    $dir = Join-Path $pruneDir $_
    #    if (Test-Path $dir) {
    #        Remove-Item -Path $dir -Recurse -Force
    #    }
    #}
}

function Step_PruneDocker {
    LogStep "PruneDocker"

    $pruneDir = $repositoryDir
    LogWarning "Pruning $pruneDir Docker artifacts"

    'container', 'image', 'volume', 'network' | ForEach-Object {
        LogCmd "docker $_ prune -f"
        & docker $_ prune -f | Out-Null
    }

    $(docker image ls --format "{{.Repository}}:{{.Tag}}") | ForEach-Object {
        if ($_.StartsWith("${dockerComposeProject}-")) {
            LogCmd "docker image rm $_"
            & docker image rm $_ | Out-Null
        }
    }

    $(docker volume ls --format "{{.Name}}") | ForEach-Object {
        if ($_.StartsWith("${dockerComposeProject}_")) {
            LogCmd "docker volume rm $_"
            & docker volume rm $_ | Out-Null
        }
    }
}

function Step_DotnetClean {
    LogStep "dotnet clean $dotnetSolutionFile --verbosity $DotnetVerbosity -nologo"
    & dotnet clean "$dotnetSolutionFile" --verbosity $DotnetVerbosity -nologo
    if (-not $?) { exit $LastExitCode }
}

function Step_DotnetRestore {
    LogStep "dotnet restore $dotnetSolutionFile --verbosity $DotnetVerbosity -nologo"
    & dotnet restore "$dotnetSolutionFile" --verbosity $DotnetVerbosity -nologo
    if (-not $?) { exit $LastExitCode }
}

function Step_DotnetBuild {
    LogStep "dotnet build $dotnetSolutionFile --no-restore --configuration $Configuration --verbosity $DotnetVerbosity -nologo /p:Version=$buildVersion"
    $currentLocation = Get-Location
    try {
        Set-Location $srcDir
        & dotnet build "$dotnetSolutionFile" --no-restore --configuration $Configuration --verbosity $DotnetVerbosity -nologo /p:Version=$buildVersion
        if (-not $?) { exit $LastExitCode }
    }
    finally {
        Set-Location $currentLocation
    }
}

function Step_DotnetPublish {
    param([ValidateNotNullOrEmpty()] [string]$ProjectFile, [ValidateNotNullOrEmpty()] [string]$PublishOutput)
    LogStep "dotnet publish $ProjectFile --output $PublishOutput --configuration $Configuration --verbosity $DotnetVerbosity -nologo /p:Version=$buildVersion"
    & dotnet publish "$ProjectFile" --output "$PublishOutput" --configuration $Configuration --verbosity $DotnetVerbosity -nologo /p:Version=$buildVersion
    if (-not $?) { exit $LastExitCode }
}

function Step_DotnetTest {
    param([ValidateNotNullOrEmpty()] [string]$ProjectFile)
    LogStep "dotnet test $ProjectFile --no-build --configuration $Configuration --logger:trx -nologo"
    & dotnet test "$ProjectFile" --no-build --configuration $Configuration --logger:trx --logger:"console;verbosity=normal" -nologo
    if (-not $?) { exit $LastExitCode }
}

function Step_DockerBuild {
    param([ValidateNotNullOrEmpty()] [string]$DockerFilePath)
    $file = Get-Item $DockerFilePath
    $imageName = $file.BaseName
    $imageTag = "${imageName}:${buildVersion}"
    $dockerfile = $file.Name
    $dir = $file.Directory.FullName

    LogStep "docker build -t $imageTag -f $dockerfile ."
    $currentLocation = Get-Location
    try {
        Set-Location $dir
        & docker build -t $imageTag -f $dockerfile .
        if (-not $?) { exit $LastExitCode }
    }
    finally {
        Set-Location $currentLocation
    }
}

function Get_DockerComposeAppFile {
    if ($ScaleOutApplication) {
        "docker-compose.app-scaled.yaml"
    }
    else {
        "docker-compose.app.yaml"
    }
}

function Get_DockerComposeInfraServicesFile {
    [string]$cpuArchitecture = [System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture
    switch ($cpuArchitecture.ToLower()) {
        "x64" { "docker-compose.infra.services-amd64.yaml" }
        "amd64" { "docker-compose.infra.services-amd64.yaml" }
        "arm64" { "docker-compose.infra.services-arm64.yaml" }
        default { throw "CPU architecture $cpuArchitecture is not supported." }
    }
}

function Step_DockerComposeStart {
    $dockerComposeInfraFile = "docker-compose.infra.yaml"
    $dockerComposeInfraServicesFile = Get_DockerComposeInfraServicesFile
    $dockerComposeAppFile = Get_DockerComposeAppFile
    LogStep "docker compose -p $dockerComposeProject -f $dockerComposeInfraFile -f $dockerComposeInfraServicesFile -f $dockerComposeAppFile up --build --abort-on-container-exit"
    & docker compose -p $dockerComposeProject -f $dockerComposeInfraFile -f $dockerComposeInfraServicesFile -f $dockerComposeAppFile up --build --abort-on-container-exit
    if (-not $?) { exit $LastExitCode }
}

function Step_DockerComposeStartDetached {
    $dockerComposeInfraFile = "docker-compose.infra.yaml"
    $dockerComposeInfraServicesFile = Get_DockerComposeInfraServicesFile
    $dockerComposeAppFile = Get_DockerComposeAppFile
    LogStep "docker compose -p $dockerComposeProject -f $dockerComposeInfraFile -f $dockerComposeInfraServicesFile -f $dockerComposeAppFile up --build --detach"
    & docker compose -p $dockerComposeProject -f $dockerComposeInfraFile -f $dockerComposeInfraServicesFile -f $dockerComposeAppFile up --build --detach
    if (-not $?) { exit $LastExitCode }
}

function Step_DockerComposeStop {
    $dockerComposeInfraFile = "docker-compose.infra.yaml"
    $dockerComposeInfraServicesFile = Get_DockerComposeInfraServicesFile
    $dockerComposeAppFile = Get_DockerComposeAppFile
    LogStep "docker compose -p $dockerComposeProject -f $dockerComposeInfraFile -f $dockerComposeInfraServicesFile -f $dockerComposeAppFile down"
    & docker compose -p $dockerComposeProject -f $dockerComposeInfraFile -f $dockerComposeInfraServicesFile -f $dockerComposeAppFile down
    if (-not $?) { exit $LastExitCode }
}


#######################################################################
# PRELUDE TARGETS

function Target_Prelude_DotnetCli {
    PreludeStep_ValidateDotnetCli
}

function Target_Prelude_DockerCli {
    PreludeStep_ValidateDockerCli
}

function Target_Prelude {
    DependsOn "Prelude.DotnetCli"
    DependsOn "Prelude.DockerCli"
}


#######################################################################
# TARGETS

function Target_Dotnet_Clean {
    DependsOn "Prelude.DotnetCli"

    LogTarget "Dotnet.Clean"
    Step_DotnetClean
}

function Target_Dotnet_Restore {
    DependsOn "Prelude.DotnetCli"

    LogTarget "Dotnet.Restore"
    Step_DotnetRestore
}

function Target_Dotnet_Build {
    DependsOn "Prelude.DotnetCli"
    DependsOn "Dotnet.Restore"

    LogTarget "Dotnet.Build"
    Step_DotnetBuild
}

function Target_Dotnet_Test {
    DependsOn "Prelude.DotnetCli"
    DependsOn "Dotnet.Build"

    LogTarget "Dotnet.Test"
    $projects = Get-ChildItem -Path $srcDir -Filter "*.Tests.?sproj" -Recurse -File
    foreach ($projectFile in $projects) {
        Step_DotnetTest $projectFile
    }
}

function Target_Dotnet_Publish {
    DependsOn "Prelude.DotnetCli"
    DependsOn "Dotnet.Build"

    LogTarget "Dotnet.Publish"
    $dockerfiles = Get-ChildItem -Path $srcDir -Filter "*.Dockerfile" -Recurse -File
    foreach ($dockerFile in $dockerfiles) {
        LogInfo "Dockerfile found: $dockerFile"
        $projectDirectory = $dockerFile.Directory
        $projectFile = Get-ChildItem -Path $projectDirectory -Filter "*.?sproj" | Select-Object -First 1
        $publishOutput = [System.IO.Path]::Combine($projectDirectory, "bin", "publish")
        Step_DotnetPublish $projectFile $publishOutput
    }
}

function Target_Docker_Build {
    DependsOn "Prelude"
    DependsOn "Dotnet.Publish"

    LogTarget "Docker.Build"
    $dockerFiles = Get-ChildItem -Path $srcDir -Filter "*.Dockerfile" -Recurse -File
    foreach ($dockerFile in $dockerFiles) {
        LogInfo "Dockerfile found: $dockerFile"
        Step_DockerBuild $dockerFile
    }
}

function Target_DockerCompose_Start {
    DependsOn "Prelude"
    DependsOn "Dotnet.Publish"

    LogTarget "DockerCompose.Start"
    try {
        Step_DockerComposeStart
    }
    finally {
        # ensure proper cleanup
        Step_DockerComposeStop
    }
}

function Target_DockerCompose_StartDetached {
    DependsOn "Prelude"
    DependsOn "Dotnet.Publish"

    LogTarget "DockerCompose.StartDetached"
    Step_DockerComposeStartDetached
}

function Target_DockerCompose_Stop {
    DependsOn "Prelude.DockerCli"

    LogTarget "DockerCompose.Stop"
    Step_DockerComposeStop
}

function Target_Prune_Build {
    DependsOn "Prelude.DotNetCli"
    LogTarget "Prune.Build"
    Step_PruneBuild
}

function Target_Prune_Docker {
    DependsOn "Prelude.DockerCli"
    LogTarget "Prune.Docker"
    Step_PruneDocker
}

function Target_Prune {
    DependsOn "Prelude"
    DependsOn "Prune.Build"
    DependsOn "Prune.Docker"
}

function Target_FullBuild {
    DependsOn "Prelude"
    DependsOn "Dotnet.Build"
    DependsOn "Dotnet.Test"
    DependsOn "Dotnet.Publish"
}


#######################################################################
# DEPENDENCIES TRACKING

$targetCalls = @{ }
function DependsOn {
    param([ValidateNotNullOrEmpty()] [string]$Target)
    if (-not $targetCalls.ContainsKey($Target)) {
        Invoke_BuildTarget $Target
        $targetCalls.Add($Target, $(Get-Date))
    }
}

function Invoke_BuildTarget {
    param([ValidateNotNullOrEmpty()] [string]$Target)
    $normalizedTarget = $Target.Replace(".", "_")
    Invoke-Expression "Target_$normalizedTarget"
}


#######################################################################
# MAIN ENTRY POINT

$exitResult = 0

$currentLocation = Get-Location
try {
    LogInfo "*** BUILD: $Target ($buildVersion $Configuration) in $repositoryDir"
    Set-Location $repositoryDir

    Invoke_BuildTarget $Target

    Write-Host ""
    LogInfo "DONE"
}
catch [System.Exception] {
    $errorMessage = "$_"
    if ($errorMessage.StartsWith("The term 'Target_$Target' is not recognized") -or
        $errorMessage.StartsWith("The term 'Target_$normalizedTarget' is not recognized")) {
        LogError("Target $Target is not recognized")
    }
    else {
        LogError($errorMessage)
    }
    $exitResult = 1
}
finally {
    $stopwatch.Stop()
    LogInfo "*** Completed in: $($stopwatch.Elapsed)"
    Set-Location $currentLocation
}

Exit $exitResult
